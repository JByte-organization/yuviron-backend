using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Infrastructure.Caching;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class CachingExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
        
        services.AddSingleton<IConnectionMultiplexer>(_ => 
        {
            var options = ConfigurationOptions.Parse(redisConnectionString!);
            options.AbortOnConnectFail = false; 
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
