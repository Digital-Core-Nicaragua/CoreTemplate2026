namespace CoreTemplate.Auditing.Abstractions;

/// <summary>
/// Contexto de auditoria del request actual.
/// Provee los datos del actor para enriquecer los registros de auditoria.
/// </summary>
public interface IAuditContext
{
    Guid? UsuarioId { get; }
    Guid? TenantId { get; }
    string? DireccionIp { get; }
    string? UserAgent { get; }
    string? CorrelationId { get; }
}
