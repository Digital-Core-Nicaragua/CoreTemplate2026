using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace CoreTemplate.Monitoring.HealthChecks;

/// <summary>
/// Health check que verifica la conectividad con Redis usando PingAsync().
/// Solo se registra cuando EnableTokenBlacklist = true y Provider = Redis.
/// </summary>
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var db = redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis accesible.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se puede conectar a Redis.", ex);
        }
    }
}
