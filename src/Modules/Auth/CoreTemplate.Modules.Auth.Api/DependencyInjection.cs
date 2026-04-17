using CoreTemplate.Modules.Auth.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Auth.Api;

/// <summary>
/// Registro de la capa Api del modulo Auth.
/// El registro de Infrastructure se hace desde CoreTemplate.Modules.Auth.Infrastructure.DependencyInjection.
/// Para registrar el modulo completo usar AddAuthModule() desde el Host.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuthApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthApplication(configuration);
        return services;
    }
}
