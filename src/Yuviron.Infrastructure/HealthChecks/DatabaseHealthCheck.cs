using Microsoft.Extensions.Diagnostics.HealthChecks;
using Yuviron.Infrastructure.Persistence; 

namespace Yuviron.Infrastructure.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var canConnect = await _dbContext.Database.CanConnectAsync(cts.Token);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("MySQL is unreachable");
            }

            return HealthCheckResult.Healthy("MySQL is available");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("MySQL connection timed out (Fail-Fast)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error connecting to MySQL", ex);
        }
    }
}