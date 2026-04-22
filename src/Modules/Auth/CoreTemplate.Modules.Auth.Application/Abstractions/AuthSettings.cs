namespace CoreTemplate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Configuración del módulo Auth leída desde appsettings.json.
/// Sección: <c>AuthSettings</c>.
/// </summary>
public sealed class AuthSettings
{
    public const string SectionName = "AuthSettings";

    public string JwtSecretKey { get; init; } = string.Empty;
    public string JwtIssuer { get; init; } = "CoreTemplate";
    public string JwtAudience { get; init; } = "CoreTemplate";
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 7;
    public bool TwoFactorEnabled { get; init; } = false;
    public bool TwoFactorRequired { get; init; } = false;
    public int PasswordResetTokenExpirationHours { get; init; } = 1;
    public int MaxSesionesSimultaneas { get; init; } = 5;
    public AccionAlLlegarLimiteSesiones AccionAlLlegarLimiteSesiones { get; init; } = AccionAlLlegarLimiteSesiones.CerrarMasAntigua;
    public bool EnableTokenBlacklist { get; init; } = true;
    public bool UseActionCatalog { get; init; } = false;
    /// <summary>Work factor de BCrypt. Default 12 (producción). Usar 4 en desarrollo para mayor velocidad.</summary>
    public int BcryptWorkFactor { get; init; } = 12;
}

/// <summary>
/// Acción a tomar cuando un usuario supera el límite de sesiones simultáneas.
/// </summary>
public enum AccionAlLlegarLimiteSesiones
{
    /// <summary>Cierra automáticamente la sesión más antigua.</summary>
    CerrarMasAntigua = 1,

    /// <summary>Rechaza el nuevo login con error descriptivo.</summary>
    BloquearNuevoLogin = 2
}

/// <summary>
/// Configuración de bloqueo de cuenta por intentos fallidos.
/// Sección: <c>LockoutSettings</c>.
/// </summary>
public sealed class LockoutSettings
{
    public const string SectionName = "LockoutSettings";

    public int MaxFailedAttempts { get; init; } = 5;
    public int LockoutDurationMinutes { get; init; } = 15;
    public bool AutoUnlock { get; init; } = true;
}

/// <summary>
/// Política de contraseñas.
/// Sección: <c>PasswordPolicy</c>.
/// </summary>
public sealed class PasswordPolicySettings
{
    public const string SectionName = "PasswordPolicy";

    public int MinLength { get; init; } = 8;
    public bool RequireUppercase { get; init; } = true;
    public bool RequireLowercase { get; init; } = true;
    public bool RequireDigit { get; init; } = true;
    public bool RequireSpecialChar { get; init; } = false;
}

/// <summary>
/// Configuración del backend de Token Blacklist.
/// Sección: <c>TokenBlacklistSettings</c>.
/// </summary>
public sealed class TokenBlacklistSettings
{
    public const string SectionName = "TokenBlacklistSettings";

    /// <summary>InMemory (default) o Redis.</summary>
    public string Provider { get; init; } = "InMemory";

    /// <summary>Connection string de Redis. Solo requerido si Provider = Redis.</summary>
    public string RedisConnectionString { get; init; } = string.Empty;
}

/// <summary>
/// Configuración de la estructura organizacional.
/// Sección: <c>OrganizationSettings</c>.
/// </summary>
public sealed class OrganizationSettings
{
    public const string SectionName = "OrganizationSettings";

    /// <summary>Habilita el soporte de sucursales por usuario.</summary>
    public bool EnableBranches { get; init; } = false;
}

/// <summary>
/// Configuración del portal de clientes externos.
/// Sección: <c>CustomerPortalSettings</c>.
/// <para>
/// Cuando <c>EnableCustomerPortal = false</c> (default), todos los endpoints
/// del portal están deshabilitados y la tabla <c>Auth.UsuariosCliente</c> no se usa.
/// </para>
/// </summary>
public sealed class CustomerPortalSettings
{
    public const string SectionName = "CustomerPortalSettings";

    /// <summary>Activa el portal de clientes. Default: false.</summary>
    public bool EnableCustomerPortal { get; init; } = false;

    /// <summary>
    /// Si true, cualquier visitante puede registrarse como cliente.
    /// Si false, el registro está cerrado y el endpoint retorna 403. Default: true.
    /// </summary>
    public bool RegistroHabilitado { get; init; } = true;

    /// <summary>
    /// Si true, el cliente debe verificar su email antes de poder hacer login.
    /// Si false, el estado pasa directamente a Active al registrarse. Default: true.
    /// </summary>
    public bool RequireEmailVerification { get; init; } = true;

    /// <summary>
    /// Si true, el cliente debe verificar su teléfono además del email.
    /// Requiere que el teléfono sea proporcionado al registrarse. Default: false.
    /// </summary>
    public bool RequirePhoneVerification { get; init; } = false;

    /// <summary>
    /// Si true, el cliente puede ver y cerrar sus propias sesiones activas.
    /// Default: false.
    /// </summary>
    public bool EnableSessionManagement { get; init; } = false;

    /// <summary>Configuración de proveedores OAuth externos.</summary>
    public CustomerPortalOAuthSettings OAuth { get; init; } = new();

    /// <summary>Configuración del registro por teléfono (WhatsApp/SMS).</summary>
    public RegistroPorTelefonoSettings RegistroPorTelefono { get; init; } = new();
}

/// <summary>
/// Configuración de proveedores OAuth para el portal de clientes.
/// </summary>
public sealed class CustomerPortalOAuthSettings
{
    /// <summary>Configuración de Google OAuth.</summary>
    public GoogleOAuthSettings Google { get; init; } = new();

    /// <summary>Configuración de Facebook OAuth.</summary>
    public FacebookOAuthSettings Facebook { get; init; } = new();
}

/// <summary>
/// Configuración de Google OAuth 2.0.
/// El frontend obtiene el idToken y lo envía al backend para validación.
/// </summary>
public sealed class GoogleOAuthSettings
{
    /// <summary>Activa el login con Google. Default: false.</summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Client ID de la app en Google Cloud Console.
    /// Se usa para validar que el idToken fue emitido para esta app.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;
}

/// <summary>
/// Configuración de Facebook OAuth.
/// El frontend obtiene el accessToken y lo envía al backend para validación.
/// </summary>
public sealed class FacebookOAuthSettings
{
    /// <summary>Activa el login con Facebook. Default: false.</summary>
    public bool Enabled { get; init; } = false;

    /// <summary>App ID de la app en Facebook Developers.</summary>
    public string AppId { get; init; } = string.Empty;

    /// <summary>App Secret de la app en Facebook Developers.</summary>
    public string AppSecret { get; init; } = string.Empty;
}

/// <summary>
/// Configuración del registro e identificación por teléfono (WhatsApp/SMS).
/// Cuando está habilitado, los clientes pueden registrarse y hacer login
/// usando su número de teléfono + OTP en lugar de email + contraseña.
/// </summary>
public sealed class RegistroPorTelefonoSettings
{
    /// <summary>Activa el registro e identificación por teléfono. Default: false.</summary>
    public bool Enabled { get; init; } = false;

    /// <summary>Canal de envío del OTP: "WhatsApp" o "SMS". Default: WhatsApp.</summary>
    public string Proveedor { get; init; } = "WhatsApp";

    /// <summary>Minutos de validez del OTP. Default: 10.</summary>
    public int OtpExpirationMinutes { get; init; } = 10;
}
