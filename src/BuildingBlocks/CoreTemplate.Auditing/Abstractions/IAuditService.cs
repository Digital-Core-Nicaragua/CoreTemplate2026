using CoreTemplate.Auditing.Models;

namespace CoreTemplate.Auditing.Abstractions;

/// <summary>
/// Servicio para registrar acciones de auditoria de forma explicita.
/// Para auditoria automatica de CRUD usar AuditSaveChangesInterceptor.
/// </summary>
public interface IAuditService
{
    /// <summary>Registra una accion de auditoria.</summary>
    Task LogAsync(AuditLog auditLog, CancellationToken ct = default);

    /// <summary>Obtiene el historial de auditoria de una entidad.</summary>
    Task<IEnumerable<AuditLog>> GetHistorialAsync(
        string nombreEntidad,
        string entidadId,
        CancellationToken ct = default);
}
