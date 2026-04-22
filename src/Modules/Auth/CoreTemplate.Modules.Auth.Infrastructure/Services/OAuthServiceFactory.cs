using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementación del factory de OAuth services.
/// Resuelve GoogleOAuthService o FacebookOAuthService según el proveedor indicado.
/// Al usar IServiceProvider internamente, evita registrar IProveedorOAuthService
/// sin clave (lo que causaría ambigüedad con múltiples implementaciones).
/// </summary>
internal sealed class OAuthServiceFactory(IServiceProvider _serviceProvider) : IOAuthServiceFactory
{
    public IProveedorOAuthService Resolver(TipoProveedorOAuth proveedor) => proveedor switch
    {
        TipoProveedorOAuth.Google => _serviceProvider.GetRequiredService<GoogleOAuthService>(),
        TipoProveedorOAuth.Facebook => _serviceProvider.GetRequiredService<FacebookOAuthService>(),
        _ => throw new NotSupportedException($"El proveedor OAuth '{proveedor}' no está soportado.")
    };
}
