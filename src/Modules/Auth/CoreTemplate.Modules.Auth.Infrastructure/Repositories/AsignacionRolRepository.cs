using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

internal sealed class AsignacionRolRepository(AuthDbContext _db) : IAsignacionRolRepository
{
    public Task<AsignacionRol?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.AsignacionesRol.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<AsignacionRol>> GetByUsuarioSucursalAsync(
        Guid usuarioId, Guid sucursalId, CancellationToken ct = default) =>
        _db.AsignacionesRol
            .Where(a => a.UsuarioId == usuarioId && a.SucursalId == sucursalId)
            .ToListAsync(ct);

    public Task<List<AsignacionRol>> GetByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        _db.AsignacionesRol
            .Where(a => a.UsuarioId == usuarioId)
            .ToListAsync(ct);

    public Task<bool> ExisteAsync(
        Guid usuarioId, Guid sucursalId, Guid rolId, CancellationToken ct = default) =>
        _db.AsignacionesRol.AnyAsync(
            a => a.UsuarioId == usuarioId && a.SucursalId == sucursalId && a.RolId == rolId, ct);

    public async Task AddAsync(AsignacionRol asignacion, CancellationToken ct = default)
    {
        await _db.AsignacionesRol.AddAsync(asignacion, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(AsignacionRol asignacion, CancellationToken ct = default)
    {
        _db.AsignacionesRol.Remove(asignacion);
        await _db.SaveChangesAsync(ct);
    }
}
