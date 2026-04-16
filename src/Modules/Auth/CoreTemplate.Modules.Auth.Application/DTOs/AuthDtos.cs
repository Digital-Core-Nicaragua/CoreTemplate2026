using CoreTemplate.Modules.Auth.Domain.Enums;

namespace CoreTemplate.Modules.Auth.Application.DTOs;

/// <summary>Detalle completo de un usuario para respuestas de consulta.</summary>
public sealed record UsuarioDto(
    Guid Id,
    Guid? TenantId,
    string Email,
    string Nombre,
    EstadoUsuario Estado,
    bool TwoFactorActivo,
    DateTime? UltimoAcceso,
    DateTime CreadoEn,
    IReadOnlyList<string> Roles);

/// <summary>Resumen de usuario para listados paginados.</summary>
public sealed record UsuarioResumenDto(
    Guid Id,
    string Email,
    string Nombre,
    EstadoUsuario Estado,
    DateTime? UltimoAcceso,
    IReadOnlyList<string> Roles);

/// <summary>Detalle de un rol con sus permisos.</summary>
public sealed record RolDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    bool EsSistema,
    IReadOnlyList<string> Permisos);

/// <summary>Resumen de rol para listados.</summary>
public sealed record RolResumenDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    bool EsSistema,
    int CantidadPermisos);

/// <summary>Respuesta completa del login exitoso.</summary>
public sealed record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiraEn,
    UsuarioDto Usuario);

/// <summary>
/// Respuesta de login cuando el usuario tiene 2FA activo.
/// El cliente debe enviar el código TOTP para obtener el token definitivo.
/// </summary>
public sealed record Login2FARequeridoDto(
    string TokenTemporal,
    bool Requires2FA);

/// <summary>Respuesta de renovación de tokens.</summary>
public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiraEn);

/// <summary>Respuesta de activación de 2FA con QR y códigos de recuperación.</summary>
public sealed record Activar2FAResponseDto(
    string QrCodeUri,
    string SecretKey,
    IReadOnlyList<string> CodigosRecuperacion);

/// <summary>Información de una sesión activa del usuario.</summary>
public sealed record SesionDto(
    Guid Id,
    CanalAcceso Canal,
    string Dispositivo,
    string Ip,
    string UserAgent,
    DateTime UltimaActividad,
    DateTime ExpiraEn,
    DateTime CreadoEn);
