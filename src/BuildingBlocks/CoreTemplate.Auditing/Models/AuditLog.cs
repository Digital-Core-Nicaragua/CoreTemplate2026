namespace CoreTemplate.Auditing.Models;

/// <summary>
/// Registro de auditoria de una accion en el sistema.
/// Se persiste en la tabla Shared.AuditLogs.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>ID del tenant. Null si single-tenant.</summary>
    public Guid? TenantId { get; init; }

    /// <summary>ID del usuario que realizo la accion. Null si es anonimo o sistema.</summary>
    public Guid? UsuarioId { get; init; }

    /// <summary>Nombre del aggregate o entidad afectada. Ej: "Usuario", "CatalogoItem".</summary>
    public string NombreEntidad { get; init; } = string.Empty;

    /// <summary>ID del aggregate afectado.</summary>
    public string EntidadId { get; init; } = string.Empty;

    /// <summary>Tipo de accion realizada.</summary>
    public AuditActionType Accion { get; init; }

    /// <summary>Estado anterior de la entidad en JSON. Null para Created.</summary>
    public string? ValoresAnteriores { get; init; }

    /// <summary>Estado nuevo de la entidad en JSON. Null para Deleted.</summary>
    public string? ValoresNuevos { get; init; }

    /// <summary>Fecha y hora UTC de la accion.</summary>
    public DateTime OcurridoEn { get; init; } = DateTime.UtcNow;

    /// <summary>IP del cliente que realizo la accion.</summary>
    public string? DireccionIp { get; init; }

    /// <summary>User-Agent del cliente.</summary>
    public string? UserAgent { get; init; }

    /// <summary>ID de correlacion del request HTTP.</summary>
    public string? CorrelationId { get; init; }
}
