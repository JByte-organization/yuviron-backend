using Microsoft.AspNetCore.Diagnostics.HealthChecks; 
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Yuviron.Api.Middlewares;
using Yuviron.Application;
using Yuviron.Infrastructure;
using Yuviron.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] 
                   ?? builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

var otlpHeaders = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"];


// =========================================================================
// PART 0: SETTING UP LOGGING (Serilog + Seq + Native OTLP Aspire)
// =========================================================================

// 1. Configuring Serilog (for Seq and Console)
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
}, preserveStaticLogger: false, writeToProviders: true);

// 2. Configuring native Microsoft OTLP to send logs to Aspire
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Yuviron.Api")); 

    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        logging.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            if (!string.IsNullOrWhiteSpace(otlpHeaders))
            {
                options.Headers = otlpHeaders;
            }
        });
    }
});

// =========================================================================
// PART 1: REGISTRATION OF SERVICES (DI Container)
// =========================================================================

// 1.0 OpenTelemetry (Traces and metrics)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Yuviron.Api"))
            .AddAspNetCoreInstrumentation(options => {
                options.Filter = httpContext => {
                    var path = httpContext.Request.Path.Value;

                    return path is null ||
                           (!path.Equals("/health/live", StringComparison.OrdinalIgnoreCase) &&
                            !path.Equals("/health/ready", StringComparison.OrdinalIgnoreCase));
                };
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("Yuviron.*");

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options => {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                if (!string.IsNullOrWhiteSpace(otlpHeaders))
                {
                    options.Headers = otlpHeaders;
                }
            });
        }
    })
    .WithMetrics(metrics => {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Yuviron.Api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options => {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                if (!string.IsNullOrWhiteSpace(otlpHeaders))
                {
                    options.Headers = otlpHeaders;
                }
            });
        }
    });

// 1.1 Architectural layers
builder.Services.AddApplication();

// CONNECTING THE INFRASTRUCTURE
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiBackgroundServices(builder.Configuration);

// 1.2 Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("client", new OpenApiInfo 
    { 
        Title = "Yuviron Client API", 
        Version = "v1",
        Description = "API для клиентского приложения (поиск, авторизация, файлы и т.д.)"
    });

    c.SwaggerDoc("admin", new OpenApiInfo 
    { 
        Title = "Yuviron Admin API", 
        Version = "v1",
        Description = "API для панели администратора"
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var relativePath = apiDesc.RelativePath;
        if (string.IsNullOrEmpty(relativePath)) return false;

        bool isAdminRoute = relativePath.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase);

        if (docName == "admin") return isAdminRoute;
        if (docName == "client") return !isAdminRoute;

        return false;
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT access token without the Bearer prefix.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",        
        BearerFormat = "JWT"   
    });

    c.OperationFilter<Yuviron.Api.Common.SecurityRequirementsOperationFilter>();
});

// 1.3 CORS (Permissions for frontend)
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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear(); 

    options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1"));
    options.KnownProxies.Add(System.Net.IPAddress.Parse("::1"));
});

// 1.4 Global error handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 1.5 Rate Limiter
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

    options.AddPolicy("AuthPolicy", context =>
    {
        // Теперь RemoteIpAddress будет содержать реальный IP пользователя (благодаря ForwardedHeaders)
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5, 
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0 
        });
    });
});

// =========================================================================
// BUILDING THE APPLICATION
// =========================================================================
var app = builder.Build();

// =========================================================================
// Part 2: INITIALIZATION AND HTTP PIPELINE (Middlewares)
// Attention: The order of app.Use... calls is of great importance!
// =========================================================================

app.UseForwardedHeaders();

// 2.1 Initializing the Database (Migrations and Seed)
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
        logger.LogCritical(ex, "FATAL: An error occurred during database initialisation. The application will not start.");
        
        throw; 
    }
}

// 2.2 Error catching (Must be at the very beginning of the pipeline)
app.UseExceptionHandler();

// 2.3 Swagger UI (For development only or if enabled in the config)
var swaggerEnabled = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/client/swagger.json", "Client API");
        c.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin API");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 2.4 Basic protections and rules (CORS and limits before authorization processing)
app.UseCors("YuvironCorsPolicy");
app.UseRateLimiter();

// 2.5 Distribution of static files (ТОЛЬКО публичные папки)
var storageRoot = builder.Configuration["FILE_STORAGE_ROOT"] 
                  ?? Environment.GetEnvironmentVariable("FILE_STORAGE_ROOT") 
                  ?? "/var/yuviron/storage";

var publicFolders = new[] { "avatars", "covers", "banners" };

foreach (var folder in publicFolders)
{
    var folderPath = Path.Combine(storageRoot, folder);
    
    if (!Directory.Exists(folderPath))
    {
        Directory.CreateDirectory(folderPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(folderPath),
        RequestPath = $"/storage/{folder}",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            var origin = ctx.Context.Request.Headers["Origin"].ToString();
            var isAllowed = allowedOrigins.Contains(origin) || allowedOrigins.Contains("*");

            if (isAllowed && !string.IsNullOrEmpty(origin))
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            }
        }
    });
}

// 2.6 Authentication and Authorization (Strictly in that order!)
app.UseAuthentication();
app.UseAuthorization();

// 2.7 Endpoint routing (Controllers and HealthChecks)
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

// START SERVER
app.Run();