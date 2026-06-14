using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));
            options.UseMySql(connectionString, serverVersion, builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                builder.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
            });
        });

        services.AddScoped<AppDbContextInitializer>();

        return services;
    }
}
