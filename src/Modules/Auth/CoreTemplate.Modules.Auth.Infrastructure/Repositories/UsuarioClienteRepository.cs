using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using CoreTemplate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de clientes del portal.
/// Incluye siempre la colección de Proveedores para que el aggregate
/// esté completo al cargarse (necesario para VincularProveedor, etc.).
/// </summary>
internal sealed class UsuarioClienteRepository(AuthDbContext _db) : IUsuarioClienteRepository
{
    public Task<UsuarioCliente?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.UsuariosCliente
            .Include(c => c.Proveedores)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<UsuarioCliente?> GetByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default)
    {
        var emailNorm = email.Trim().ToLowerInvariant();
        return _db.UsuariosCliente
            .Include(c => c.Proveedores)
            .FirstOrDefaultAsync(c =>
                c.Email == emailNorm &&
                (tenantId == null || c.TenantId == tenantId), ct);
    }

    public Task<UsuarioCliente?> GetByTelefonoAsync(string telefono, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.UsuariosCliente
            .Include(c => c.Proveedores)
            .FirstOrDefaultAsync(c =>
                c.Telefono == telefono.Trim() &&
                (tenantId == null || c.TenantId == tenantId), ct);

    public Task<bool> ExistsByTelefonoAsync(string telefono, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.UsuariosCliente.AnyAsync(c =>
            c.Telefono == telefono.Trim() &&
            (tenantId == null || c.TenantId == tenantId), ct);

    public Task<UsuarioCliente?> GetByExternalIdAsync(
        TipoProveedorOAuth proveedor,
        string externalId,
        Guid? tenantId = null,
        CancellationToken ct = default) =>
        _db.UsuariosCliente
            .Include(c => c.Proveedores)
            .FirstOrDefaultAsync(c =>
                c.Proveedores.Any(p => p.Proveedor == proveedor && p.ExternalId == externalId) &&
                (tenantId == null || c.TenantId == tenantId), ct);

    public Task<bool> ExistsByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default)
    {
        var emailNorm = email.Trim().ToLowerInvariant();
        return _db.UsuariosCliente.AnyAsync(c =>
            c.Email == emailNorm &&
            (tenantId == null || c.TenantId == tenantId), ct);
    }

    public Task<UsuarioCliente?> GetByTokenRestablecimientoAsync(string token, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.UsuariosCliente
            .Include(c => c.Proveedores)
            .FirstOrDefaultAsync(c =>
                c.TokenRestablecimiento == token &&
                c.TokenRestablecimientoExpiraEn > DateTime.UtcNow &&
                (tenantId == null || c.TenantId == tenantId), ct);

    public async Task<PagedResult<UsuarioCliente>> GetPagedAsync(
        Guid? tenantId,
        EstadoUsuarioCliente? estado = null,
        int pagina = 1,
        int tamanoPagina = 20,
        CancellationToken ct = default)
    {
        var query = _db.UsuariosCliente.AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(c => c.TenantId == tenantId.Value);

        if (estado.HasValue)
            query = query.Where(c => c.Estado == estado.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreadoEn)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new PagedResult<UsuarioCliente>(items, pagina, tamanoPagina, total);
    }

    public async Task AddAsync(UsuarioCliente cliente, CancellationToken ct = default)
    {
        await _db.UsuariosCliente.AddAsync(cliente, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UsuarioCliente cliente, CancellationToken ct = default)
    {
        _db.UsuariosCliente.Update(cliente);
        await _db.SaveChangesAsync(ct);
    }
}
