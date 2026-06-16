using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Infrastructure.Services;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventBus, MassTransitEventBus>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            
            // Automatically add all consumers in the assembly
            x.AddConsumers(Assembly.GetExecutingAssembly());

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "127.0.0.1";
                var user = configuration["RabbitMQ:Username"] ?? "guest";
                var pass = configuration["RabbitMQ:Password"] ?? "guest";
                var vhost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(host, vhost, h => {
                    h.Username(user);
                    h.Password(pass);
                });
                
                cfg.ReceiveEndpoint("api_background_tasks", e =>
                {
                    e.ConfigureConsumers(context); 
                });
            });
        });

        return services;
    }
}
