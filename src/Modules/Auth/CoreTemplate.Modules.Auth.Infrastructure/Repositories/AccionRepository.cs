using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

internal sealed class AccionRepository(AuthDbContext _db) : IAccionRepository
{
    public Task<Accion?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Acciones.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Accion?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        _db.Acciones.FirstOrDefaultAsync(a => a.Codigo == codigo, ct);

    public Task<List<Accion>> GetAllAsync(string? modulo = null, CancellationToken ct = default)
    {
        var query = _db.Acciones.AsQueryable();
        if (!string.IsNullOrWhiteSpace(modulo))
            query = query.Where(a => a.Modulo == modulo);
        return query.OrderBy(a => a.Modulo).ThenBy(a => a.Codigo).ToListAsync(ct);
    }

    public Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default) =>
        _db.Acciones.AnyAsync(a => a.Codigo == codigo, ct);

    public async Task AddAsync(Accion accion, CancellationToken ct = default)
    {
        await _db.Acciones.AddAsync(accion, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Accion accion, CancellationToken ct = default)
    {
        _db.Acciones.Update(accion);
        await _db.SaveChangesAsync(ct);
    }
}
