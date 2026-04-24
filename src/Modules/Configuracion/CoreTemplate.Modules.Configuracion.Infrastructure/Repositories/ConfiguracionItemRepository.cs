using CoreTemplate.Modules.Configuracion.Domain.Aggregates;
using CoreTemplate.Modules.Configuracion.Domain.Repositories;
using CoreTemplate.Modules.Configuracion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Configuracion.Infrastructure.Repositories;

/// <summary>
/// Repositorio de parámetros de configuración.
/// Los parámetros globales (TenantId = null) requieren IgnoreQueryFilters()
/// para no ser bloqueados por el QueryFilter automático de BaseDbContext.
/// </summary>
internal sealed class ConfiguracionItemRepository(ConfiguracionDbContext db) : IConfiguracionItemRepository
{
    public async Task<ConfiguracionItem?> ObtenerPorClaveAsync(
        string clave, Guid? tenantId = null, CancellationToken ct = default)
    {
        var claveNorm = clave.ToLowerInvariant();

        if (tenantId is null)
            return await db.Items.IgnoreQueryFilters()
                .Where(i => i.Clave == claveNorm && i.TenantId == null)
                .FirstOrDefaultAsync(ct);

        return await db.Items
            .Where(i => i.Clave == claveNorm && i.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ConfiguracionItem>> ListarAsync(
        string? grupo = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        // Listar globales + los del tenant actual (si aplica)
        var query = db.Items.IgnoreQueryFilters()
            .Where(i => i.TenantId == null || i.TenantId == tenantId);

        if (grupo is not null)
            query = query.Where(i => i.Grupo == grupo);

        var todos = await query.OrderBy(i => i.Grupo).ThenBy(i => i.Clave).ToListAsync(ct);

        // Si hay versión del tenant, tiene prioridad sobre la global
        if (tenantId is not null)
        {
            var clavesTenant = todos.Where(i => i.TenantId == tenantId).Select(i => i.Clave).ToHashSet();
            return todos.Where(i => i.TenantId == tenantId || !clavesTenant.Contains(i.Clave)).ToList();
        }

        return todos;
    }

    public async Task<bool> ExisteClaveAsync(string clave, Guid? tenantId = null, CancellationToken ct = default)
    {
        var claveNorm = clave.ToLowerInvariant();
        return await db.Items.IgnoreQueryFilters()
            .AnyAsync(i => i.Clave == claveNorm && i.TenantId == tenantId, ct);
    }

    public async Task GuardarAsync(ConfiguracionItem item, CancellationToken ct = default)
    {
        await db.Items.AddAsync(item, ct);
        await db.SaveChangesAsync(ct);
        item.ClearDomainEvents();
    }

    public async Task ActualizarAsync(ConfiguracionItem item, CancellationToken ct = default)
    {
        db.Items.Update(item);
        await db.SaveChangesAsync(ct);
        item.ClearDomainEvents();
    }
}
