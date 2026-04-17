using CoreTemplate.Auditing.Abstractions;
using CoreTemplate.Auditing.Models;
using CoreTemplate.Auditing.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Auditing.Services;

/// <summary>
/// Implementacion de <see cref="IAuditService"/> que persiste en la tabla Shared.AuditLogs.
/// </summary>
internal sealed class AuditService(AuditDbContext db) : IAuditService
{
    public async Task LogAsync(AuditLog auditLog, CancellationToken ct = default)
    {
        db.AuditLogs.Add(auditLog);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<AuditLog>> GetHistorialAsync(
        string nombreEntidad,
        string entidadId,
        CancellationToken ct = default) =>
        await db.AuditLogs
            .Where(a => a.NombreEntidad == nombreEntidad && a.EntidadId == entidadId)
            .OrderByDescending(a => a.OcurridoEn)
            .ToListAsync(ct);
}
