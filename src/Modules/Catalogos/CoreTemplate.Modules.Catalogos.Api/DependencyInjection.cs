using CoreTemplate.Modules.Catalogos.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Catalogos.Api;

/// <summary>
/// Registro de la capa Api del modulo Catalogos.
/// El registro de Infrastructure se hace desde CoreTemplate.Modules.Catalogos.Infrastructure.DependencyInjection.
/// Para registrar el modulo completo usar AddCatalogosModule() desde el Host.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCatalogosApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCatalogosApplication(configuration);
        return services;
    }
}
