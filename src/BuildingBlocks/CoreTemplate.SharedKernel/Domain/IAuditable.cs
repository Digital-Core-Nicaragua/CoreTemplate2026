namespace CoreTemplate.SharedKernel.Domain;

/// <summary>
/// Interfaz marcadora para indicar que un Aggregate o Entity
/// debe ser auditado automáticamente por el AuditSaveChangesInterceptor.
/// Los cambios (Created, Updated, Deleted) se registran en Shared.AuditLogs.
/// </summary>
public interface IAuditable { }
