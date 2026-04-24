using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using CoreTemplate.Modules.PdfTemplates.Domain.Repositories;
using CoreTemplate.Modules.PdfTemplates.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure.Repositories;

internal sealed class PdfPlantillaRepository(PdfTemplatesDbContext db) : IPdfPlantillaRepository
{
    public async Task<PdfPlantilla?> ObtenerPorCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Plantillas globales (TenantId = null) requieren IgnoreQueryFilters
        if (tenantId is null)
            return await db.Plantillas
                .IgnoreQueryFilters()
                .Where(p => p.Codigo == codigo.ToLowerInvariant() && p.TenantId == null)
                .FirstOrDefaultAsync(ct);

        return await db.Plantillas
            .Where(p => p.Codigo == codigo.ToLowerInvariant() && p.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PdfPlantilla?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Plantillas.FindAsync([id], ct);

    public async Task<IReadOnlyList<PdfPlantilla>> ListarAsync(
        string? modulo = null, bool? soloActivos = null, CancellationToken ct = default)
    {
        var query = db.Plantillas.AsQueryable();
        if (modulo is not null) query = query.Where(p => p.Modulo == modulo);
        if (soloActivos is not null) query = query.Where(p => p.EsActivo == soloActivos);
        return await query.OrderBy(p => p.Modulo).ThenBy(p => p.Codigo).ToListAsync(ct);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default)
    {
        if (tenantId is null)
            return await db.Plantillas.IgnoreQueryFilters()
                .AnyAsync(p => p.Codigo == codigo.ToLowerInvariant() && p.TenantId == null, ct);

        return await db.Plantillas
            .AnyAsync(p => p.Codigo == codigo.ToLowerInvariant() && p.TenantId == tenantId, ct);
    }

    public async Task GuardarAsync(PdfPlantilla plantilla, CancellationToken ct = default)
    {
        await db.Plantillas.AddAsync(plantilla, ct);
        await db.SaveChangesAsync(ct);
        plantilla.ClearDomainEvents();
    }

    public async Task ActualizarAsync(PdfPlantilla plantilla, CancellationToken ct = default)
    {
        db.Plantillas.Update(plantilla);
        await db.SaveChangesAsync(ct);
        plantilla.ClearDomainEvents();
    }
}
