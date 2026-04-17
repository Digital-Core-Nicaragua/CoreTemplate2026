using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CoreTemplate.Monitoring.Configuration;

/// <summary>
/// Extensiones para configurar health checks y sus endpoints.
/// </summary>
public static class MonitoringExtensions
{
    /// <summary>
    /// Registra los health checks base (liveness).
    /// Para agregar checks de DB o Redis usar AddDatabaseHealthCheck() y AddRedisHealthCheck().
    /// </summary>
    public static IHealthChecksBuilder AddCoreMonitoring(this IServiceCollection services) =>
        services.AddHealthChecks();

    /// <summary>
    /// Mapea los endpoints de health checks en el pipeline HTTP:
    /// /health, /health/ready, /health/live, /health/detail (solo Development).
    /// </summary>
    public static IApplicationBuilder UseHealthCheckEndpoints(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var isDevelopment = configuration["ASPNETCORE_ENVIRONMENT"] == "Development"
            || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        // /health — resumen general
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    duration = report.TotalDuration.TotalMilliseconds
                });
                await context.Response.WriteAsync(result);
            }
        });

        // /health/ready — Kubernetes readiness probe
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // /health/live — Kubernetes liveness probe
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false // solo verifica que la app responde
        });

        // /health/detail — detalle completo (solo Development/Staging)
        if (isDevelopment)
        {
            app.UseHealthChecks("/health/detail", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        duration = report.TotalDuration.TotalMilliseconds,
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds,
                            error = e.Value.Exception?.Message
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });
        }

        return app;
    }
}
