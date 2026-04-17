using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CoreTemplate.Monitoring.HealthChecks;

/// <summary>
/// Health check que verifica la conectividad con la base de datos
/// usando CanConnectAsync() del DbContext configurado.
/// </summary>
public sealed class DatabaseHealthCheck<TContext>(TContext db) : IHealthCheck
    where TContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync(ct);
            return canConnect
                ? HealthCheckResult.Healthy("Base de datos accesible.")
                : HealthCheckResult.Unhealthy("No se puede conectar a la base de datos.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al verificar la base de datos.", ex);
        }
    }
}
