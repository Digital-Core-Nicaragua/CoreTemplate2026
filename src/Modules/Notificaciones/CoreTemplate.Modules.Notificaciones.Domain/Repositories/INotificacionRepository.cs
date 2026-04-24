using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Notificaciones.Domain.Repositories;

public interface INotificacionRepository
{
    Task<Notificacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Notificacion>> ListarPorUsuarioAsync(
        Guid usuarioId, bool? soloNoLeidas, int pagina, int tamano, CancellationToken ct = default);
    Task<int> ContarNoLeidasAsync(Guid usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<Notificacion>> ObtenerNoLeidasAsync(Guid usuarioId, CancellationToken ct = default);
    Task GuardarAsync(Notificacion notificacion, CancellationToken ct = default);
    Task MarcarTodasComoLeidasAsync(Guid usuarioId, CancellationToken ct = default);
}
