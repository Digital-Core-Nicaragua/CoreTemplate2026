using CoreTemplate.Modules.Notificaciones.Application.Commands;
using CoreTemplate.Modules.Notificaciones.Application.DTOs;
using CoreTemplate.Modules.Notificaciones.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Notificaciones.Application.Queries;

// ─── Listar mis notificaciones ────────────────────────────────────────────────

public record GetMisNotificacionesQuery(
    Guid UsuarioId,
    bool? SoloNoLeidas = null,
    int Pagina = 1,
    int Tamano = 20) : IRequest<Result<PagedResult<NotificacionDto>>>;

internal sealed class GetMisNotificacionesHandler(INotificacionRepository repo)
    : IRequestHandler<GetMisNotificacionesQuery, Result<PagedResult<NotificacionDto>>>
{
    public async Task<Result<PagedResult<NotificacionDto>>> Handle(
        GetMisNotificacionesQuery q, CancellationToken ct)
    {
        var paged = await repo.ListarPorUsuarioAsync(q.UsuarioId, q.SoloNoLeidas, q.Pagina, q.Tamano, ct);
        var dtos = new PagedResult<NotificacionDto>(
            paged.Items.Select(n => n.ToDto()).ToList(),
            paged.Pagina, paged.TamanoPagina, paged.Total);
        return Result<PagedResult<NotificacionDto>>.Success(dtos);
    }
}

// ─── Conteo de no leídas ──────────────────────────────────────────────────────

public record GetConteoNoLeidasQuery(Guid UsuarioId) : IRequest<Result<ConteoNoLeidasDto>>;

internal sealed class GetConteoNoLeidasHandler(INotificacionRepository repo)
    : IRequestHandler<GetConteoNoLeidasQuery, Result<ConteoNoLeidasDto>>
{
    public async Task<Result<ConteoNoLeidasDto>> Handle(
        GetConteoNoLeidasQuery q, CancellationToken ct)
    {
        var count = await repo.ContarNoLeidasAsync(q.UsuarioId, ct);
        return Result<ConteoNoLeidasDto>.Success(new ConteoNoLeidasDto(count));
    }
}
