using CoreTemplate.Logging.Abstractions;
using CoreTemplate.Logging.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Logging;

/// <summary>
/// Registro de dependencias del building block Logging.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra ICorrelationContext e IAppLogger en el contenedor de DI.
    /// Llamar desde AddInfrastructureBase() o desde Program.cs.
    /// </summary>
    public static IServiceCollection AddCoreLogging(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<IAppLogger, AppLogger>();
        return services;
    }
}
