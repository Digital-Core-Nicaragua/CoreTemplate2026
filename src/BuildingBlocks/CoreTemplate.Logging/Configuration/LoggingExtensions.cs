using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CoreTemplate.Logging.Configuration;

/// <summary>
/// Extensiones para configurar Serilog con enrichers de correlacion, entorno y proceso.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configura Serilog en el host con enrichers automaticos:
    /// MachineName, Environment, ProcessId, ThreadId.
    /// </summary>
    public static IHostBuilder UseCorrelationLogging(this IHostBuilder hostBuilder) =>
        hostBuilder.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console());

    /// <summary>
    /// Agrega el middleware de correlacion al pipeline HTTP.
    /// Debe llamarse antes de UseSerilogRequestLogging().
    /// </summary>
    public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<CoreTemplate.Logging.Middleware.CorrelationMiddleware>();
}
