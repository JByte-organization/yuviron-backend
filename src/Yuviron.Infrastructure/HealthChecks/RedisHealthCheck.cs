using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Yuviron.Infrastructure.HealthChecks;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            var pong = await db.PingAsync().WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);

            return pong >= TimeSpan.Zero
                ? HealthCheckResult.Healthy("Redis is available")
                : HealthCheckResult.Unhealthy("Redis is unresponsive");
        }
        catch (TimeoutException)
        {
            return HealthCheckResult.Unhealthy("Redis ping timed out (Fail-Fast)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error connecting to Redis", ex);
        }
    }
}