using System.Text.Json;
using CoreTemplate.Auditing.Abstractions;
using CoreTemplate.Auditing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreTemplate.Auditing.Interceptors;

/// <summary>
/// Interceptor de EF Core que captura automaticamente los cambios
/// Added, Modified y Deleted antes de cada SaveChangesAsync.
/// Registrar en BaseDbContext via AddInterceptors().
/// </summary>
public sealed class AuditSaveChangesInterceptor(IAuditContext auditContext) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, ct);

        var auditLogs = GenerarAuditLogs(eventData.Context);

        // Los AuditLogs se guardan en el mismo SaveChanges del contexto que los origino
        // Solo si el contexto tiene un DbSet<AuditLog> (es decir, es AuditDbContext)
        // Para otros contextos, se ignoran (la auditoria explicita usa IAuditService)
        if (eventData.Context is CoreTemplate.Auditing.Persistence.AuditDbContext auditDb)
        {
            auditDb.AuditLogs.AddRange(auditLogs);
        }

        return await base.SavingChangesAsync(eventData, result, ct);
    }

    private List<AuditLog> GenerarAuditLogs(DbContext context)
    {
        var logs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var accion = entry.State switch
            {
                EntityState.Added => AuditActionType.Created,
                EntityState.Modified => AuditActionType.Updated,
                EntityState.Deleted => AuditActionType.Deleted,
                _ => AuditActionType.Updated
            };

            var entidadId = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString()
                ?? string.Empty;

            var valoresAnteriores = entry.State == EntityState.Added
                ? null
                : JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));

            var valoresNuevos = entry.State == EntityState.Deleted
                ? null
                : JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));

            logs.Add(new AuditLog
            {
                NombreEntidad = entry.Metadata.ClrType.Name,
                EntidadId = entidadId,
                Accion = accion,
                ValoresAnteriores = valoresAnteriores,
                ValoresNuevos = valoresNuevos,
                UsuarioId = auditContext.UsuarioId,
                TenantId = auditContext.TenantId,
                DireccionIp = auditContext.DireccionIp,
                UserAgent = auditContext.UserAgent,
                CorrelationId = auditContext.CorrelationId,
                OcurridoEn = DateTime.UtcNow
            });
        }

        return logs;
    }
}
