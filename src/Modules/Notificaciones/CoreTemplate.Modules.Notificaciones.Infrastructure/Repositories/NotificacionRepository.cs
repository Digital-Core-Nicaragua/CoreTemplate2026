using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using CoreTemplate.Modules.Notificaciones.Domain.Repositories;
using CoreTemplate.Modules.Notificaciones.Infrastructure.Persistence;
using CoreTemplate.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Notificaciones.Infrastructure.Repositories;

internal sealed class NotificacionRepository(NotificacionesDbContext db) : INotificacionRepository
{
    public async Task<Notificacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Notificaciones.FindAsync([id], ct);

    public async Task<PagedResult<Notificacion>> ListarPorUsuarioAsync(
        Guid usuarioId, bool? soloNoLeidas, int pagina, int tamano, CancellationToken ct = default)
    {
        var query = db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .AsQueryable();

        if (soloNoLeidas == true)
            query = query.Where(n => !n.EsLeida);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreadaEn)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(ct);

        return new PagedResult<Notificacion>(items, pagina, tamano, total);
    }

    public async Task<int> ContarNoLeidasAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && !n.EsLeida, ct);

    public async Task<IReadOnlyList<Notificacion>> ObtenerNoLeidasAsync(
        Guid usuarioId, CancellationToken ct = default) =>
        await db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.EsLeida)
            .OrderByDescending(n => n.CreadaEn)
            .ToListAsync(ct);

    public async Task GuardarAsync(Notificacion notificacion, CancellationToken ct = default)
    {
        var entry = db.Entry(notificacion);
        if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            await db.Notificaciones.AddAsync(notificacion, ct);
        await db.SaveChangesAsync(ct);
        notificacion.ClearDomainEvents();
    }

    public async Task MarcarTodasComoLeidasAsync(Guid usuarioId, CancellationToken ct = default)
    {
        await db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.EsLeida)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.EsLeida, true)
                .SetProperty(n => n.LeidaEn, DateTime.UtcNow), ct);
    }
}
