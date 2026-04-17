using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using CoreTemplate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

// RolRepository

internal sealed class RolRepository(AuthDbContext _db) : IRolRepository
{
    public Task<Rol?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Roles
            .Include(r => r.Permisos)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<Rol>> GetAllAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Roles
            .Include(r => r.Permisos)
            .OrderBy(r => r.Nombre)
            .ToListAsync(ct);

    public Task<bool> ExistsByNombreAsync(string nombre, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Roles.AnyAsync(r => r.Nombre == nombre, ct);

    public async Task AddAsync(Rol rol, CancellationToken ct = default)
    {
        await _db.Roles.AddAsync(rol, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Rol rol, CancellationToken ct = default)
    {
        _db.Roles.Update(rol);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Rol rol, CancellationToken ct = default)
    {
        _db.Roles.Remove(rol);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> TieneUsuariosAsync(Guid rolId, CancellationToken ct = default) =>
        _db.Usuarios.AnyAsync(u => u.Roles.Any(r => r.RolId == rolId), ct);
}

// PermisoRepository

internal sealed class PermisoRepository(AuthDbContext _db) : IPermisoRepository
{
    public Task<Permiso?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Permisos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Permiso?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        _db.Permisos.FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    public Task<List<Permiso>> GetAllAsync(CancellationToken ct = default) =>
        _db.Permisos.OrderBy(p => p.Modulo).ThenBy(p => p.Codigo).ToListAsync(ct);

    public Task<List<Permiso>> GetByModuloAsync(string modulo, CancellationToken ct = default) =>
        _db.Permisos.Where(p => p.Modulo == modulo).OrderBy(p => p.Codigo).ToListAsync(ct);

    public async Task AddAsync(Permiso permiso, CancellationToken ct = default)
    {
        await _db.Permisos.AddAsync(permiso, ct);
        await _db.SaveChangesAsync(ct);
    }
}

// RegistroAuditoriaRepository

internal sealed class RegistroAuditoriaRepository(AuthDbContext _db) : IRegistroAuditoriaRepository
{
    public async Task AddAsync(RegistroAuditoria registro, CancellationToken ct = default)
    {
        await _db.RegistrosAuditoria.AddAsync(registro, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<RegistroAuditoria>> GetByUsuarioAsync(
        Guid usuarioId, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var query = _db.RegistrosAuditoria
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.CreadoEn);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new PagedResult<RegistroAuditoria>(items, pagina, tamanoPagina, total);
    }
}
