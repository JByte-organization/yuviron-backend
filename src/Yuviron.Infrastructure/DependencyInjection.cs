using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Infrastructure.DependencyInjectionModules;

namespace Yuviron.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new ActivitySource("Yuviron.Infrastructure"));

        services
            .AddPersistence(configuration)
            .AddAuthenticationInternal(configuration)
            .AddCaching(configuration)
            .AddOptionsInternal(configuration)
            .AddServices()
            .AddHealthChecksInternal();

        return services;
    }

    public static IServiceCollection AddApiBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMessaging(configuration)
            .AddBackgroundServices();

        return services;
    }
}
