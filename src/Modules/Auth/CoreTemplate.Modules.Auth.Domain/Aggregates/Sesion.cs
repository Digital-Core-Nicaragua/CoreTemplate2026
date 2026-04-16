using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate que representa una sesión activa de un usuario.
/// Reemplaza a RefreshToken como entidad gestionable con información de contexto.
/// </summary>
public sealed class Sesion : AggregateRoot<Guid>
{
    /// <summary>ID del usuario propietario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>ID del tenant. Null si single-tenant.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Hash SHA256 del refresh token.</summary>
    public string RefreshTokenHash { get; private set; } = string.Empty;

    /// <summary>Canal desde donde se originó la sesión.</summary>
    public CanalAcceso Canal { get; private set; }

    /// <summary>Nombre del dispositivo o cliente.</summary>
    public string Dispositivo { get; private set; } = string.Empty;

    /// <summary>IP de origen.</summary>
    public string Ip { get; private set; } = string.Empty;

    /// <summary>User agent del cliente.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    /// <summary>Fecha de último uso (renovación o actividad).</summary>
    public DateTime UltimaActividad { get; private set; }

    /// <summary>Fecha de expiración del refresh token.</summary>
    public DateTime ExpiraEn { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    /// <summary>Indica si la sesión está activa.</summary>
    public bool EsActiva { get; private set; }

    /// <summary>Indica si la sesión es válida (activa y no expirada).</summary>
    public bool EsValida => EsActiva && DateTime.UtcNow < ExpiraEn;

    private Sesion() { }

    /// <summary>Crea una nueva sesión activa.</summary>
    public static Sesion Crear(
        Guid usuarioId,
        Guid? tenantId,
        string refreshTokenHash,
        DateTime expiraEn,
        CanalAcceso canal,
        string ip,
        string userAgent,
        string dispositivo = "") => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        TenantId = tenantId,
        RefreshTokenHash = refreshTokenHash,
        Canal = canal,
        Dispositivo = dispositivo,
        Ip = ip,
        UserAgent = userAgent,
        UltimaActividad = DateTime.UtcNow,
        ExpiraEn = expiraEn,
        CreadoEn = DateTime.UtcNow,
        EsActiva = true
    };

    /// <summary>
    /// Rota el refresh token de la sesión y actualiza la última actividad.
    /// </summary>
    public void Renovar(string nuevoRefreshTokenHash, DateTime nuevaExpiracion)
    {
        RefreshTokenHash = nuevoRefreshTokenHash;
        ExpiraEn = nuevaExpiracion;
        UltimaActividad = DateTime.UtcNow;
    }

    /// <summary>Revoca la sesión marcándola como inactiva.</summary>
    public void Revocar()
    {
        EsActiva = false;
    }
}
