using CoreTemplate.Auditing;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Logging;
using CoreTemplate.Monitoring;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Infrastructure;

/// <summary>
/// Extensiones de registro de dependencias para la infraestructura base.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureBase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Settings
        services.Configure<TenantSettings>(
            configuration.GetSection(TenantSettings.SectionName));

        services.Configure<DatabaseSettings>(
            configuration.GetSection(DatabaseSettings.SectionName));

        // Implementaciones de SharedKernel.Abstractions
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<ICurrentTenant, CurrentTenantService>();
        services.AddScoped<ICurrentBranch, CurrentBranchService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        // Logging estructurado con correlacion
        services.AddCoreLogging();

        // Auditoria automatica y explicita
        services.AddCoreAuditing(configuration);

        // Health checks
        services.AddCoreMonitoring();

        return services;
    }
}
