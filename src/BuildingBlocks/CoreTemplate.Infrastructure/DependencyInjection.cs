using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Infrastructure;

/// <summary>
/// Extensiones de registro de dependencias para la infraestructura base.
/// <para>
/// Registra los servicios transversales disponibles para todos los módulos:
/// <see cref="ICurrentUser"/>, <see cref="ICurrentTenant"/> y las clases de configuración.
/// </para>
/// <para>
/// Se llama una sola vez en <c>Program.cs</c> antes de registrar los módulos:
/// <code>
/// builder.Services.AddInfrastructureBase(builder.Configuration);
/// </code>
/// </para>
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura base en el contenedor de DI.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    public static IServiceCollection AddInfrastructureBase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── HttpContextAccessor (requerido por CurrentUserService y CurrentTenantService) ──
        services.AddHttpContextAccessor();

        // ─── Settings ─────────────────────────────────────────────────────────
        services.Configure<TenantSettings>(
            configuration.GetSection(TenantSettings.SectionName));

        services.Configure<DatabaseSettings>(
            configuration.GetSection(DatabaseSettings.SectionName));

        // ─── Servicios transversales ──────────────────────────────────────────
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<ICurrentTenant, CurrentTenantService>();
        services.AddScoped<ICurrentBranch, CurrentBranchService>();

        return services;
    }
}
