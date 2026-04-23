using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;
using CoreTemplate.Modules.EmailTemplates.Domain.Repositories;
using CoreTemplate.Modules.EmailTemplates.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Repositories;

/// <summary>
/// Repositorio de plantillas de correo electrónico.
/// <para>
/// Nota sobre multi-tenant: las plantillas globales del sistema tienen <c>TenantId = null</c>.
/// Al buscarlas se usa <c>IgnoreQueryFilters()</c> para evitar que el QueryFilter automático
/// de <c>BaseDbContext</c> las oculte cuando hay un tenant activo en el contexto.
/// Las plantillas específicas de un tenant sí pasan por el QueryFilter normal.
/// </para>
/// </summary>

internal sealed class EmailTemplateRepository(EmailTemplatesDbContext db) : IEmailTemplateRepository
{
    public async Task<EmailTemplate?> ObtenerPorCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Las plantillas globales (TenantId = null) requieren IgnoreQueryFilters
        // porque el QueryFilter de BaseDbContext filtraría por el tenant del request
        if (tenantId is null)
            return await db.Plantillas
                .IgnoreQueryFilters()
                .Where(t => t.Codigo == codigo.ToLowerInvariant() && t.TenantId == null)
                .FirstOrDefaultAsync(ct);

        return await db.Plantillas
            .Where(t => t.Codigo == codigo.ToLowerInvariant() && t.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EmailTemplate?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Plantillas.FindAsync([id], ct);

    public async Task<IReadOnlyList<EmailTemplate>> ListarAsync(string? modulo = null, bool? soloActivos = null, CancellationToken ct = default)
    {
        var query = db.Plantillas.AsQueryable();
        if (modulo is not null) query = query.Where(t => t.Modulo == modulo);
        if (soloActivos is not null) query = query.Where(t => t.EsActivo == soloActivos);
        return await query.OrderBy(t => t.Modulo).ThenBy(t => t.Codigo).ToListAsync(ct);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default)
    {
        if (tenantId is null)
            return await db.Plantillas
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Codigo == codigo.ToLowerInvariant() && t.TenantId == null, ct);

        return await db.Plantillas
            .AnyAsync(t => t.Codigo == codigo.ToLowerInvariant() && t.TenantId == tenantId, ct);
    }

    public async Task GuardarAsync(EmailTemplate template, CancellationToken ct = default)
    {
        var entry = db.Entry(template);
        if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            db.Plantillas.Add(template);
        await db.SaveChangesAsync(ct);
        template.ClearDomainEvents();
    }
}
