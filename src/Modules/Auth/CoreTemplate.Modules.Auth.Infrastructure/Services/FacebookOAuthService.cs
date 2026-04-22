using CoreTemplate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Valida accessTokens de Facebook usando la Graph API.
/// Flujo:
/// 1. Verifica el token con /debug_token (requiere App Token = AppId|AppSecret)
/// 2. Obtiene los datos del usuario con /me?fields=id,name,email
/// </summary>
internal sealed class FacebookOAuthService(
    IOptions<CustomerPortalSettings> _settings,
    HttpClient _httpClient) : IProveedorOAuthService
{
    public async Task<OAuthUsuarioInfo?> ValidarTokenAsync(string token, CancellationToken ct = default)
    {
        var config = _settings.Value.OAuth.Facebook;
        if (!config.Enabled) return null;

        try
        {
            // Paso 1: verificar el token con el App Token de Facebook
            var appToken = $"{config.AppId}|{config.AppSecret}";
            var debugUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={appToken}";

            var debugResponse = await _httpClient.GetFromJsonAsync<FacebookDebugResponse>(debugUrl, ct);
            if (debugResponse?.Data is null || !debugResponse.Data.IsValid)
                return null;

            // Verificar que el token fue emitido para esta app
            if (debugResponse.Data.AppId != config.AppId)
                return null;

            // Paso 2: obtener datos del usuario
            var userUrl = $"https://graph.facebook.com/me?fields=id,name,first_name,last_name,email&access_token={token}";
            var userResponse = await _httpClient.GetFromJsonAsync<FacebookUserResponse>(userUrl, ct);
            if (userResponse is null) return null;

            return new OAuthUsuarioInfo(
                ExternalId: userResponse.Id,
                Email: userResponse.Email ?? $"{userResponse.Id}@facebook.com",
                Nombre: userResponse.FirstName ?? userResponse.Name ?? string.Empty,
                Apellido: userResponse.LastName ?? string.Empty);
        }
        catch
        {
            return null;
        }
    }

    // DTOs internos para deserializar las respuestas de Facebook Graph API
    private sealed record FacebookDebugResponse(
        [property: JsonPropertyName("data")] FacebookDebugData? Data);

    private sealed record FacebookDebugData(
        [property: JsonPropertyName("app_id")] string AppId,
        [property: JsonPropertyName("is_valid")] bool IsValid);

    private sealed record FacebookUserResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("first_name")] string? FirstName,
        [property: JsonPropertyName("last_name")] string? LastName,
        [property: JsonPropertyName("email")] string? Email);
}
