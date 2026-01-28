using Yuviron.Api.Middlewares;
using Yuviron.Application;
using Yuviron.Infrastructure;
using Yuviron.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Подключаем слои ---
// Внутри AddInfrastructure вызовется код подключения к БД, который мы написали выше
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Стандартные сервисы API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// --- 2. AUTOMIGRATIONS (DEV ONLY) ---
// Используем Initializer, чтобы красиво накатить миграции и сиды
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();

    try
    {
        await initializer.InitialiseAsync(); // Применит db.Database.Migrate()
        await initializer.SeedAsync();     // Добавит начальные данные
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialisation.");
    }
}

// --- 3. Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- 4. HTTPS ---
// Твоя логика: отключаем редирект на проде (полезно для AWS Load Balancer)
if (!app.Environment.IsDevelopment())
{
    // Временно отключаем HTTPS редирект, пока не настроен сертификат/ALB
    // app.UseHttpsRedirection();
}
else
{
    // В Dev режиме можно оставить, или тоже закомментировать, если мешает
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();

app.Run();