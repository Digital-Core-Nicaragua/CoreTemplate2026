using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using CoreTemplate.Modules.Catalogos.Domain.Repositories;
using CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;
using CoreTemplate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Repositories;

internal sealed class CatalogoItemRepository(CatalogosDbContext _db) : ICatalogoItemRepository
{
    public Task<CatalogoItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.CatalogoItems.FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<bool> ExistsByCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.CatalogoItems.AnyAsync(i => i.Codigo == codigo.ToUpperInvariant(), ct);

    public async Task<PagedResult<CatalogoItem>> GetPagedAsync(
        int pagina, int tamanoPagina, bool? soloActivos = null, string? busqueda = null, CancellationToken ct = default)
    {
        var query = _db.CatalogoItems.AsQueryable();

        if (soloActivos.HasValue)
        {
            query = query.Where(i => i.EsActivo == soloActivos.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim().ToUpperInvariant();
            query = query.Where(i => i.Codigo.Contains(b) || i.Nombre.ToUpper().Contains(b));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.Codigo)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new PagedResult<CatalogoItem>(items, pagina, tamanoPagina, total);
    }

    public async Task AddAsync(CatalogoItem item, CancellationToken ct = default)
    {
        await _db.CatalogoItems.AddAsync(item, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CatalogoItem item, CancellationToken ct = default)
    {
        _db.CatalogoItems.Update(item);
        await _db.SaveChangesAsync(ct);
    }
}
