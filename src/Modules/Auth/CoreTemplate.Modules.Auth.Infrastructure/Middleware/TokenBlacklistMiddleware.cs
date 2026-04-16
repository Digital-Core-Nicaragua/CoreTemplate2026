using CoreTemplate.Modules.Auth.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace CoreTemplate.Modules.Auth.Infrastructure.Middleware;

/// <summary>
/// Middleware que verifica si el JTI del AccessToken está en la blacklist.
/// Se ejecuta después de UseAuthentication.
/// Si el token está en la blacklist, retorna 401 aunque el token sea válido.
/// </summary>
public sealed class TokenBlacklistMiddleware(
    RequestDelegate _next,
    ITokenBlacklistService _blacklist)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtraerToken(context);

        if (token is not null)
        {
            var jti = ExtraerJti(token);
            if (jti is not null && await _blacklist.EstaEnBlacklistAsync(jti, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "El token ha sido revocado.",
                    errors = new[] { "El token ha sido revocado." }
                });
                return;
            }
        }

        await _next(context);
    }

    private static string? ExtraerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..].Trim();
        }
        return null;
    }

    private static string? ExtraerJti(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token).Id;
        }
        catch { return null; }
    }
}
