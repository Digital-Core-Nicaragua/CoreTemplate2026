using CoreTemplate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de contraseñas usando BCrypt.
/// Work factor configurable para balancear seguridad y rendimiento.
/// </summary>
internal sealed class PasswordService(
    IOptions<PasswordPolicySettings> _policy,
    IOptions<AuthSettings> _authSettings) : IPasswordService
{
    private readonly PasswordPolicySettings _policySettings = _policy.Value;

    /// <inheritdoc/>
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: _authSettings.Value.BcryptWorkFactor);

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);

    /// <inheritdoc/>
    public IReadOnlyList<string> ValidarPolitica(string password)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errores.Add("La contraseña es requerida.");
            return errores;
        }

        if (password.Length < _policySettings.MinLength)
        {
            errores.Add($"La contraseña debe tener al menos {_policySettings.MinLength} caracteres.");
        }

        if (_policySettings.RequireUppercase && !password.Any(char.IsUpper))
        {
            errores.Add("La contraseña debe contener al menos una letra mayúscula.");
        }

        if (_policySettings.RequireLowercase && !password.Any(char.IsLower))
        {
            errores.Add("La contraseña debe contener al menos una letra minúscula.");
        }

        if (_policySettings.RequireDigit && !password.Any(char.IsDigit))
        {
            errores.Add("La contraseña debe contener al menos un número.");
        }

        if (_policySettings.RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            errores.Add("La contraseña debe contener al menos un carácter especial.");
        }

        return errores;
    }
}
