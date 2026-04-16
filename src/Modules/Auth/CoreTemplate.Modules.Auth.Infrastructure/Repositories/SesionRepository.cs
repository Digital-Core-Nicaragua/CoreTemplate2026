using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

internal sealed class SesionRepository(AuthDbContext _db) : ISesionRepository
{
    public Task<Sesion?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Sesiones.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Sesion?> GetActivaByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default) =>
        _db.Sesiones.FirstOrDefaultAsync(
            s => s.RefreshTokenHash == refreshTokenHash && s.EsActiva, ct);

    public Task<List<Sesion>> GetActivasByUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        _db.Sesiones
            .Where(s => s.UsuarioId == usuarioId && s.EsActiva && s.ExpiraEn > DateTime.UtcNow)
            .OrderByDescending(s => s.UltimaActividad)
            .ToListAsync(ct);

    public Task<int> ContarActivasAsync(Guid usuarioId, CancellationToken ct = default) =>
        _db.Sesiones.CountAsync(
            s => s.UsuarioId == usuarioId && s.EsActiva && s.ExpiraEn > DateTime.UtcNow, ct);

    public Task<Sesion?> GetMasAntiguaActivaAsync(Guid usuarioId, CancellationToken ct = default) =>
        _db.Sesiones
            .Where(s => s.UsuarioId == usuarioId && s.EsActiva && s.ExpiraEn > DateTime.UtcNow)
            .OrderBy(s => s.UltimaActividad)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Sesion sesion, CancellationToken ct = default)
    {
        await _db.Sesiones.AddAsync(sesion, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Sesion sesion, CancellationToken ct = default)
    {
        _db.Sesiones.Update(sesion);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevocarTodasAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var sesiones = await _db.Sesiones
            .Where(s => s.UsuarioId == usuarioId && s.EsActiva)
            .ToListAsync(ct);

        foreach (var sesion in sesiones)
        {
            sesion.Revocar();
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task LimpiarExpiradosAsync(int diasAntiguedad = 30, CancellationToken ct = default)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-diasAntiguedad);
        var expiradas = await _db.Sesiones
            .Where(s => !s.EsActiva && s.CreadoEn < fechaLimite)
            .ToListAsync(ct);

        _db.Sesiones.RemoveRange(expiradas);
        await _db.SaveChangesAsync(ct);
    }
}
