using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;

namespace CoreTemplate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Contrato del servicio JWT para generar y validar tokens.
/// La implementación vive en Infrastructure.
/// </summary>
public interface IJwtService
{
    /// <summary>Genera un AccessToken JWT para el usuario indicado.</summary>
    string GenerarAccessToken(Usuario usuario);

    /// <summary>Genera un RefreshToken aleatorio seguro.</summary>
    string GenerarRefreshToken();

    /// <summary>
    /// Genera un token temporal de corta duración para el flujo de 2FA.
    /// Expira en 5 minutos.
    /// </summary>
    string GenerarTokenTemporal2FA(Guid usuarioId);

    /// <summary>
    /// Valida un token temporal de 2FA y retorna el UsuarioId si es válido.
    /// </summary>
    Guid? ValidarTokenTemporal2FA(string token);

    /// <summary>Fecha de expiración del próximo AccessToken.</summary>
    DateTime ObtenerExpiracionAccessToken();

    /// <summary>Extrae el JTI (JWT ID) de un token sin validar su firma.</summary>
    string? ExtraerJti(string token);

    /// <summary>Extrae la fecha de expiración de un token sin validar su firma.</summary>
    DateTime? ExtraerExpiracion(string token);
}

/// <summary>
/// Contrato del servicio de contraseñas para hash y verificación.
/// La implementación usa BCrypt en Infrastructure.
/// </summary>
public interface IPasswordService
{
    /// <summary>Genera el hash BCrypt de una contraseña en texto plano.</summary>
    string HashPassword(string password);

    /// <summary>Verifica si una contraseña en texto plano coincide con su hash.</summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Valida que la contraseña cumpla la política de seguridad configurada.
    /// Retorna los errores encontrados o lista vacía si es válida.
    /// </summary>
    IReadOnlyList<string> ValidarPolitica(string password);
}

/// <summary>
/// Contrato del servicio TOTP para 2FA.
/// La implementación usa Otp.NET en Infrastructure.
/// </summary>
public interface ITotpService
{
    /// <summary>Genera una clave secreta TOTP aleatoria.</summary>
    string GenerarSecretKey();

    /// <summary>
    /// Genera el URI para el QR code compatible con Google Authenticator.
    /// Formato: otpauth://totp/{issuer}:{email}?secret={key}&issuer={issuer}
    /// </summary>
    string GenerarQrCodeUri(string email, string secretKey, string issuer);

    /// <summary>Valida un código TOTP de 6 dígitos contra la clave secreta.</summary>
    bool ValidarCodigo(string secretKey, string codigo);

    /// <summary>Genera 8 códigos de recuperación aleatorios.</summary>
    IReadOnlyList<string> GenerarCodigosRecuperacion();

    /// <summary>Genera el hash de un código de recuperación para almacenarlo.</summary>
    string HashCodigoRecuperacion(string codigo);

    /// <summary>Verifica si un código de recuperación coincide con su hash.</summary>
    bool VerificarCodigoRecuperacion(string codigo, string hash);
}

/// <summary>
/// Servicio de blacklist de tokens JWT.
/// Permite invalidar tokens antes de su expiración natural.
/// Backend configurable: InMemory (desarrollo) o Redis (producción).
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>Agrega un JTI a la blacklist con TTL igual al tiempo restante del token.</summary>
    Task AgregarAsync(string jti, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>Verifica si un JTI está en la blacklist.</summary>
    Task<bool> EstaEnBlacklistAsync(string jti, CancellationToken ct = default);
}

/// <summary>
/// Servicio que gestiona la lógica de límites de sesiones simultáneas.
/// La implementación vive en Infrastructure.
/// </summary>
public interface ISesionService
{
    /// <summary>
    /// Verifica el límite de sesiones y aplica la acción configurada.
    /// Si la acción es CerrarMasAntigua, revoca la sesión más antigua.
    /// Si la acción es BloquearNuevoLogin, retorna false.
    /// </summary>
    /// <returns>True si se puede crear la nueva sesión. False si debe bloquearse.</returns>
    Task<bool> VerificarYAplicarLimiteAsync(Guid usuarioId, TipoUsuario tipoUsuario, CancellationToken ct = default);
}
