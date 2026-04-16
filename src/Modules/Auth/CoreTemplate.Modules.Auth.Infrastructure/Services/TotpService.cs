using System.Security.Cryptography;
using System.Text;
using CoreTemplate.Modules.Auth.Application.Abstractions;
using OtpNet;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementación del servicio TOTP para 2FA usando Otp.NET.
/// Compatible con Google Authenticator, Authy y Microsoft Authenticator.
/// </summary>
internal sealed class TotpService : ITotpService
{
    private const int CodigosRecuperacionCantidad = 8;
    private const int CodigoRecuperacionLongitud = 10;

    /// <inheritdoc/>
    public string GenerarSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    /// <inheritdoc/>
    public string GenerarQrCodeUri(string email, string secretKey, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    /// <inheritdoc/>
    public bool ValidarCodigo(string secretKey, string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length != 6)
        {
            return false;
        }

        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes);

            // Ventana de ±1 período (30 segundos) para compensar desfase de reloj
            return totp.VerifyTotp(codigo, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GenerarCodigosRecuperacion()
    {
        var codigos = new List<string>(CodigosRecuperacionCantidad);

        for (int i = 0; i < CodigosRecuperacionCantidad; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(CodigoRecuperacionLongitud);
            // Formato legible: XXXXX-XXXXX
            var hex = Convert.ToHexString(bytes)[..CodigoRecuperacionLongitud];
            codigos.Add($"{hex[..5]}-{hex[5..]}");
        }

        return codigos;
    }

    /// <inheritdoc/>
    public string HashCodigoRecuperacion(string codigo)
    {
        var bytes = Encoding.UTF8.GetBytes(codigo.Replace("-", "").ToUpperInvariant());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    /// <inheritdoc/>
    public bool VerificarCodigoRecuperacion(string codigo, string hash)
    {
        var hashCalculado = HashCodigoRecuperacion(codigo);
        return string.Equals(hashCalculado, hash, StringComparison.OrdinalIgnoreCase);
    }
}
