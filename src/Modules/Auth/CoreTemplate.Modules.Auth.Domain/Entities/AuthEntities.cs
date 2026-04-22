using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Entities;

/// <summary>
/// Relación entre un usuario y un rol asignado.
/// </summary>
public sealed class UsuarioRol : Entity<Guid>
{
    /// <summary>ID del usuario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>ID del rol asignado.</summary>
    public Guid RolId { get; private set; }

    /// <summary>Fecha en que se asignó el rol.</summary>
    public DateTime AsignadoEn { get; private set; }

    private UsuarioRol() { }

    internal static UsuarioRol Crear(Guid usuarioId, Guid rolId) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        RolId = rolId,
        AsignadoEn = DateTime.UtcNow
    };
}

/// <summary>
/// Relación entre un rol y un permiso asignado.
/// </summary>
public sealed class RolPermiso : Entity<Guid>
{
    /// <summary>ID del rol.</summary>
    public Guid RolId { get; private set; }

    /// <summary>ID del permiso.</summary>
    public Guid PermisoId { get; private set; }

    private RolPermiso() { }

    internal static RolPermiso Crear(Guid rolId, Guid permisoId) => new()
    {
        Id = Guid.NewGuid(),
        RolId = rolId,
        PermisoId = permisoId
    };
}

/// <summary>
/// Refresh token para renovar el AccessToken sin re-autenticarse.
/// Se rota en cada uso — el token anterior se revoca al emitir uno nuevo.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    /// <summary>ID del usuario propietario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>Valor del token (hash SHA256).</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>Fecha de expiración.</summary>
    public DateTime ExpiraEn { get; private set; }

    /// <summary>Fecha en que fue revocado. Null si aún es válido.</summary>
    public DateTime? RevocarEn { get; private set; }

    /// <summary>Indica si el token fue revocado.</summary>
    public bool EsRevocado { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    /// <summary>IP desde donde se creó el token.</summary>
    public string CreadoDesdeIp { get; private set; } = string.Empty;

    /// <summary>Indica si el token está activo (no revocado y no expirado).</summary>
    public bool EsActivo => !EsRevocado && DateTime.UtcNow < ExpiraEn;

    private RefreshToken() { }

    internal static RefreshToken Crear(Guid usuarioId, string token, DateTime expiraEn, string ip) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Token = token,
        ExpiraEn = expiraEn,
        EsRevocado = false,
        CreadoEn = DateTime.UtcNow,
        CreadoDesdeIp = ip
    };

    internal void Revocar()
    {
        EsRevocado = true;
        RevocarEn = DateTime.UtcNow;
    }
}

/// <summary>
/// Token de un solo uso para restablecer la contraseña.
/// Expira en el tiempo configurado (default: 1 hora).
/// </summary>
public sealed class TokenRestablecimiento : Entity<Guid>
{
    /// <summary>ID del usuario propietario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>Valor del token (hash SHA256).</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>Fecha de expiración.</summary>
    public DateTime ExpiraEn { get; private set; }

    /// <summary>Fecha en que fue usado. Null si aún no se usó.</summary>
    public DateTime? UsadoEn { get; private set; }

    /// <summary>Indica si el token ya fue usado.</summary>
    public bool EsUsado { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    /// <summary>Indica si el token es válido (no usado y no expirado).</summary>
    public bool EsValido => !EsUsado && DateTime.UtcNow < ExpiraEn;

    private TokenRestablecimiento() { }

    internal static TokenRestablecimiento Crear(Guid usuarioId, string token, int horasExpiracion) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Token = token,
        ExpiraEn = DateTime.UtcNow.AddHours(horasExpiracion),
        EsUsado = false,
        CreadoEn = DateTime.UtcNow
    };

    internal void Usar()
    {
        EsUsado = true;
        UsadoEn = DateTime.UtcNow;
    }
}

/// <summary>
/// Código de recuperación de 2FA de un solo uso.
/// Se generan 8 códigos al activar el 2FA.
/// </summary>
public sealed class CodigoRecuperacion2FA : Entity<Guid>
{
    /// <summary>ID del usuario propietario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>Hash del código de recuperación.</summary>
    public string CodigoHash { get; private set; } = string.Empty;

    /// <summary>Fecha en que fue usado. Null si aún no se usó.</summary>
    public DateTime? UsadoEn { get; private set; }

    /// <summary>Indica si el código ya fue usado.</summary>
    public bool EsUsado { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    private CodigoRecuperacion2FA() { }

    internal static CodigoRecuperacion2FA Crear(Guid usuarioId, string codigoHash) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        CodigoHash = codigoHash,
        EsUsado = false,
        CreadoEn = DateTime.UtcNow
    };

    internal void Usar()
    {
        EsUsado = true;
        UsadoEn = DateTime.UtcNow;
    }
}

/// <summary>
/// Registro inmutable de un evento de seguridad (login, logout, bloqueo, etc.).
/// Los registros de auditoría nunca se modifican ni eliminan.
/// </summary>
public sealed class RegistroAuditoria : Entity<Guid>
{
    /// <summary>ID del tenant (null si single-tenant).</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>ID del usuario. Null si el login falló con email inexistente.</summary>
    public Guid? UsuarioId { get; private set; }

    /// <summary>Email usado en el intento.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Tipo de evento registrado.</summary>
    public EventoAuditoria Evento { get; private set; }

    /// <summary>Nivel de severidad del evento.</summary>
    public SeveridadAuditoria Severidad { get; private set; }

    /// <summary>IP de origen de la solicitud.</summary>
    public string Ip { get; private set; } = string.Empty;

    /// <summary>User agent del cliente.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    /// <summary>Información adicional del evento.</summary>
    public string? Detalle { get; private set; }

    /// <summary>Fecha UTC del evento.</summary>
    public DateTime CreadoEn { get; private set; }

    private RegistroAuditoria() { }

    public static RegistroAuditoria Crear(
        Guid? tenantId,
        Guid? usuarioId,
        string email,
        EventoAuditoria evento,
        string ip,
        string userAgent,
        string? detalle = null) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        UsuarioId = usuarioId,
        Email = email,
        Evento = evento,
        Severidad = ResolverSeveridad(evento),
        Ip = ip,
        UserAgent = userAgent,
        Detalle = detalle,
        CreadoEn = DateTime.UtcNow
    };

    private static SeveridadAuditoria ResolverSeveridad(EventoAuditoria evento) => evento switch
    {
        EventoAuditoria.LoginFallido         => SeveridadAuditoria.Alta,
        EventoAuditoria.DosFactoresFallido   => SeveridadAuditoria.Alta,
        EventoAuditoria.CuentaBloqueada      => SeveridadAuditoria.Critica,
        EventoAuditoria.DosFactoresDesactivado => SeveridadAuditoria.Media,
        EventoAuditoria.RestablecimientoSolicitado => SeveridadAuditoria.Media,
        EventoAuditoria.RestablecimientoCompletado => SeveridadAuditoria.Media,
        EventoAuditoria.DosFactoresActivado  => SeveridadAuditoria.Media,
        _                                    => SeveridadAuditoria.Info
    };
}
