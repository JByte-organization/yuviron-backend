using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Application.Abstractions; // Namespace интерфейса
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Получаем строку подключения
        var connectionString = configuration.GetConnectionString("Default");

        // Твоя проверка из тестового проекта
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");
        }

        // 2. Подключаем MySQL (Pomelo) с твоими настройками версии
        services.AddDbContext<AppDbContext>(options =>
        {
            // Используем версию сервера как в твоем тесте (8.0.43)
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));

            options.UseMySql(connectionString, serverVersion, builder =>
            {
                // Рекомендуется включить повторные попытки (Retry) для облака (AWS)
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        // 3. Связываем интерфейс IApplicationDbContext с реализацией AppDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // 4. Регистрируем Инициализатор БД (для миграций)
        services.AddScoped<AppDbContextInitializer>();

        return services;
    }
}