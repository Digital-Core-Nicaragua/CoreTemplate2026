using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

internal sealed class SucursalRepository(AuthDbContext _db) : ISucursalRepository
{
    public Task<Sucursal?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Sucursales.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Sucursal?> GetByCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Sucursales.FirstOrDefaultAsync(
            s => s.Codigo == codigo.ToUpperInvariant() && s.TenantId == tenantId, ct);

    public Task<List<Sucursal>> GetAllAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Sucursales
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Nombre)
            .ToListAsync(ct);

    public Task<bool> ExistsByCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Sucursales.AnyAsync(
            s => s.Codigo == codigo.ToUpperInvariant() && s.TenantId == tenantId, ct);

    public async Task AddAsync(Sucursal sucursal, CancellationToken ct = default)
    {
        await _db.Sucursales.AddAsync(sucursal, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Sucursal sucursal, CancellationToken ct = default)
    {
        _db.Sucursales.Update(sucursal);
        await _db.SaveChangesAsync(ct);
    }
}
