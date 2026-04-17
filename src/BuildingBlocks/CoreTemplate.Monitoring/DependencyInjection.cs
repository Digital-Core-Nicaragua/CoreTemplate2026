using CoreTemplate.Monitoring.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CoreTemplate.Monitoring;

/// <summary>
/// Registro de dependencias del building block Monitoring.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los health checks base y IHealthCheckService.
    /// Retorna IHealthChecksBuilder para encadenar checks adicionales.
    /// </summary>
    public static IHealthChecksBuilder AddCoreMonitoring(this IServiceCollection services)
    {
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        return services.AddHealthChecks();
    }
}

/// <summary>
/// Implementacion de IHealthCheckService que delega al sistema de health checks de ASP.NET.
/// </summary>
file sealed class HealthCheckService(
    Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService healthCheckService)
    : IHealthCheckService
{
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        var report = await healthCheckService.CheckHealthAsync(ct);
        return report.Status == HealthStatus.Healthy;
    }
}
