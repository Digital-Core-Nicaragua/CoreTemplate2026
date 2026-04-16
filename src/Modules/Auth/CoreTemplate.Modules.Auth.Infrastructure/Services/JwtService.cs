using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementación del servicio JWT.
/// </summary>
internal sealed class JwtService(
    IOptions<AuthSettings> _settings,
    IOptions<OrganizationSettings> _orgSettings,
    ICurrentBranch _currentBranch) : IJwtService
{
    private readonly AuthSettings _authSettings = _settings.Value;

    /// <inheritdoc/>
    public string GenerarAccessToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email.Valor),
            new(JwtRegisteredClaimNames.Name, usuario.Nombre),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tipo_usuario", usuario.TipoUsuario.ToString())
        };

        if (usuario.TenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", usuario.TenantId.Value.ToString()));
        }

        foreach (var rol in usuario.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol.RolId.ToString()));
        }

        // Incluir branch_id si sucursales está habilitado
        if (_orgSettings.Value.EnableBranches)
        {
            var branchId = _currentBranch.BranchId
                ?? usuario.Sucursales.FirstOrDefault(s => s.EsPrincipal)?.SucursalId;
            if (branchId.HasValue)
                claims.Add(new Claim("branch_id", branchId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _authSettings.JwtIssuer,
            audience: _authSettings.JwtAudience,
            claims: claims,
            expires: ObtenerExpiracionAccessToken(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc/>
    public string GenerarTokenTemporal2FA(Guid usuarioId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new("tipo", "2fa_temp"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _authSettings.JwtIssuer,
            audience: _authSettings.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public Guid? ValidarTokenTemporal2FA(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtSecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _authSettings.JwtIssuer,
                ValidateAudience = true,
                ValidAudience = _authSettings.JwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var tipoClaim = principal.FindFirstValue("tipo");
            if (tipoClaim != "2fa_temp")
            {
                return null;
            }

            var subClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(subClaim, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public DateTime ObtenerExpiracionAccessToken() =>
        DateTime.UtcNow.AddMinutes(_authSettings.AccessTokenExpirationMinutes);

    /// <inheritdoc/>
    public string? ExtraerJti(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Id;
        }
        catch { return null; }
    }

    /// <inheritdoc/>
    public DateTime? ExtraerExpiracion(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo == DateTime.MinValue ? null : jwt.ValidTo;
        }
        catch { return null; }
    }
}
