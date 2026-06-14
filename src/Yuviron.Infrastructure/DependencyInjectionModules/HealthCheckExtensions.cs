using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Yuviron.Infrastructure.HealthChecks;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecksInternal(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddCheck<DatabaseHealthCheck>("mysql", tags: new[] { "ready" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" })
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", null, new[] { "ready" });

        return services;
    }
}
