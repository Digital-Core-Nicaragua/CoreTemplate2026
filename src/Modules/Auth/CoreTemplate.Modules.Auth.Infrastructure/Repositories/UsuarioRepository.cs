using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using CoreTemplate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

internal sealed class UsuarioRepository(AuthDbContext _db) : IUsuarioRepository
{
    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Usuarios
            .Include("_roles")
            .Include("_refreshTokens")
            .Include("_tokensRestablecimiento")
            .Include("_codigosRecuperacion")
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> GetByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Usuarios
            .Include("_roles")
            .Include("_refreshTokens")
            .Include("_tokensRestablecimiento")
            .Include("_codigosRecuperacion")
            .FirstOrDefaultAsync(u => u.Email.Valor == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByEmailAsync(string email, Guid? tenantId = null, CancellationToken ct = default) =>
        _db.Usuarios.AnyAsync(u => u.Email.Valor == email.ToLowerInvariant(), ct);

    public Task<Usuario?> GetByTokenRestablecimientoAsync(string token, CancellationToken ct = default) =>
        _db.Usuarios
            .Include("_roles")
            .Include("_refreshTokens")
            .Include("_tokensRestablecimiento")
            .Include("_codigosRecuperacion")
            .FirstOrDefaultAsync(u =>
                u.TokensRestablecimiento.Any(t => t.Token == token && !t.EsUsado && t.ExpiraEn > DateTime.UtcNow), ct);

    public async Task<PagedResult<Usuario>> GetPagedAsync(int pagina, int tamanoPagina, EstadoUsuario? estado = null, CancellationToken ct = default)
    {
        var query = _db.Usuarios.AsQueryable();

        if (estado.HasValue)
        {
            query = query.Where(u => u.Estado == estado.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include("_roles")
            .OrderBy(u => u.CreadoEn)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

        return new PagedResult<Usuario>(items, pagina, tamanoPagina, total);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
    {
        await _db.Usuarios.AddAsync(usuario, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Usuario usuario, CancellationToken ct = default)
    {
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync(ct);
    }
}
