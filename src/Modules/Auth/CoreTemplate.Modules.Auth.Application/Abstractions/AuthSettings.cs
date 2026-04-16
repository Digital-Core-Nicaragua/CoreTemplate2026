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
