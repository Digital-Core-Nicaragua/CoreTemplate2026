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
    /// Configura Serilog en el host con enrichers automaticos y escritura a consola y archivo.
    /// Los logs se guardan en logs/app-YYYYMMDD.log con rotacion diaria.
    /// </summary>
    public static IHostBuilder UseCorrelationLogging(this IHostBuilder hostBuilder) =>
        hostBuilder.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true));

    /// <summary>
    /// Agrega el middleware de correlacion al pipeline HTTP.
    /// Debe llamarse antes de UseSerilogRequestLogging().
    /// </summary>
    public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<CoreTemplate.Logging.Middleware.CorrelationMiddleware>();
}
