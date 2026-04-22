
using CoreTemplate.Modules.Auth.Application.Abstractions;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Valida idTokens de Google usando la librería oficial Google.Apis.Auth.
/// El frontend obtiene el idToken con Google Sign-In y lo envía al backend.
/// El backend lo valida aquí sin redireccionamientos OAuth.
/// </summary>
internal sealed class GoogleOAuthService(
    IOptions<CustomerPortalSettings> _settings) : IProveedorOAuthService
{
    public async Task<OAuthUsuarioInfo?> ValidarTokenAsync(string token, CancellationToken ct = default)
    {
        var config = _settings.Value.OAuth.Google;
        if (!config.Enabled)
        {
            return null;
        }

        try
        {
            // Valida la firma, expiración y audience del idToken con los servidores de Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(token,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [config.ClientId]
                });

            // Separar nombre y apellido — Google retorna el nombre completo en "Name"
            var partes = (payload.Name ?? "").Split(' ', 2);
            var nombre = partes.Length > 0 ? partes[0] : payload.Email;
            var apellido = partes.Length > 1 ? partes[1] : string.Empty;

            return new OAuthUsuarioInfo(
                ExternalId: payload.Subject,
                Email: payload.Email,
                Nombre: nombre,
                Apellido: apellido);
        }
        catch
        {
            // Token inválido, expirado o no emitido para este ClientId
            return null;
        }
    }
}
