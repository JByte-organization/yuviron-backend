using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks; 
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Threading.RateLimiting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using Yuviron.Api.Middlewares;
using Yuviron.Application;
using Yuviron.Infrastructure;
using Yuviron.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// ЧАСТЬ 0: НАСТРОЙКА ЛОГИРОВАНИЯ (Serilog + Seq)
// =========================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
    .WriteTo.OpenTelemetry(options => 
    {
        options.Endpoint = "http://localhost:4317";
        options.Protocol = OtlpProtocol.Grpc; 
        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = "Yuviron.Api"
        };
    })
    .CreateLogger();

builder.Host.UseSerilog();

// =========================================================================
// ЧАСТЬ 1: РЕГИСТРАЦИЯ СЕРВИСОВ (DI Container)
// =========================================================================

// 1.0 OpenTelemetry (Трассировка и графики)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => 
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Yuviron.Api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("Yuviron.*")
            .AddOtlpExporter(options => 
            {
                options.Endpoint = new Uri("http://localhost:4317"); 
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; 
            });
    })
    .WithMetrics(metrics => 
    {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Yuviron.Api"))
            .AddAspNetCoreInstrumentation() // Статистика по HTTP запросам
            .AddHttpClientInstrumentation() // Статистика по вызовам Jamendo
            .AddRuntimeInstrumentation()    // Самое важное: CPU, RAM, Garbage Collector
            .AddProcessInstrumentation()   // Данные о процессе
            .AddOtlpExporter(options => 
            {
                options.Endpoint = new Uri("http://localhost:4317");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    });

// 1.1 Архитектурные слои
builder.Services.AddApplication();

// ПОДКЛЮЧАЕМ ИНФРАСТРУКТУРУ
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiBackgroundServices(builder.Configuration);

// 1.2 Контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT access token without the Bearer prefix.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",        
        BearerFormat = "JWT"   
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// 1.3 CORS (Разрешения для фронтенда)
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
                     ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("YuvironCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 1.4 Глобальная обработка ошибок
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 1.5 Rate Limiter (Защита от DDoS и брутфорса)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.OnRejected = async (context, token) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(new 
        { 
            error = "Too Many Requests", 
            message = "Please wait a minute before trying again." 
        }, cancellationToken: token);
    };

    options.AddFixedWindowLimiter(policyName: "AuthPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5; 
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; 
    });
});

// =========================================================================
// СБОРКА ПРИЛОЖЕНИЯ
// =========================================================================
var app = builder.Build();

// =========================================================================
// ЧАСТЬ 2: ИНИЦИАЛИЗАЦИЯ И HTTP-ПАЙПЛАЙН (Middlewares)
// Внимание: Порядок вызовов app.Use... имеет огромное значение!
// =========================================================================

// 2.1 Инициализация базы данных (Миграции и Seed)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var initializer = services.GetRequiredService<AppDbContextInitializer>();
        await initializer.InitialiseAsync();
        await initializer.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialisation.");
    }
}

// 2.2 Перехват ошибок (Должен быть в самом начале пайплайна)
app.UseExceptionHandler();

// 2.3 Swagger UI (Только для разработки или если включен в конфиге)
var swaggerEnabled = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 2.4 Базовые защиты и правила (CORS и лимиты до обработки авторизации)
app.UseCors("YuvironCorsPolicy");
app.UseRateLimiter();

// 2.5 Раздача статических файлов (Музыка, Обложки)
var storageRoot = builder.Configuration["FILE_STORAGE_ROOT"] 
                  ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                  ?? "/var/yuviron/storage";

if (!Directory.Exists(storageRoot))
{
    Directory.CreateDirectory(storageRoot);
}

var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
contentTypeProvider.Mappings[".ts"] = "video/MP2T";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRoot),
    RequestPath = "/storage",
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Разрешаем плеерам читать аудио-файлы
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});

// 2.6 Аутентификация и Авторизация (Строго в таком порядке!)
app.UseAuthentication();
app.UseAuthorization();

// 2.7 Маршрутизация эндпоинтов (Контроллеры и HealthChecks)
app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// ЗАПУСК СЕРВЕРА
app.Run();