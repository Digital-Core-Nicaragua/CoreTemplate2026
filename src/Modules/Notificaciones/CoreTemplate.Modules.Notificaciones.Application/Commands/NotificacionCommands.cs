using CoreTemplate.Modules.Notificaciones.Application.DTOs;
using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using CoreTemplate.Modules.Notificaciones.Domain.Repositories;
using CoreTemplate.Notifications.Abstractions;
using CoreTemplate.SharedKernel;
using MediatR;

// Alias para evitar ambigüedad — TipoNotificacion existe en el building block y se usa en el Domain
using Tipo = CoreTemplate.Notifications.Abstractions.TipoNotificacion;

namespace CoreTemplate.Modules.Notificaciones.Application.Commands;

// ─── Enviar notificación ──────────────────────────────────────────────────────

public record EnviarNotificacionCommand(
    Guid UsuarioId,
    string Titulo,
    string Mensaje,
    Tipo Tipo = Tipo.Info,
    string? Url = null,
    Guid? TenantId = null) : IRequest<Result<NotificacionDto>>;

internal sealed class EnviarNotificacionHandler(
    INotificacionRepository repo,
    INotificationSender sender)
    : IRequestHandler<EnviarNotificacionCommand, Result<NotificacionDto>>
{
    public async Task<Result<NotificacionDto>> Handle(EnviarNotificacionCommand cmd, CancellationToken ct)
    {
        var mensaje = new NotificationMessage(cmd.UsuarioId, cmd.Titulo, cmd.Mensaje,
            cmd.Tipo, cmd.Url, cmd.TenantId);
        var sendResult = await sender.EnviarAsync(mensaje, ct);

        var result = Notificacion.Crear(cmd.UsuarioId, cmd.Titulo, cmd.Mensaje,
            cmd.Tipo, cmd.Url, sendResult.EntregadaEnTiempoReal, cmd.TenantId);

        if (!result.IsSuccess)
            return Result<NotificacionDto>.Failure(result.Error!);

        await repo.GuardarAsync(result.Value!, ct);
        return Result<NotificacionDto>.Success(result.Value!.ToDto());
    }
}

// ─── Marcar como leída ────────────────────────────────────────────────────────

public record MarcarComoLeidaCommand(Guid NotificacionId, Guid UsuarioId) : IRequest<Result>;

internal sealed class MarcarComoLeidaHandler(INotificacionRepository repo)
    : IRequestHandler<MarcarComoLeidaCommand, Result>
{
    public async Task<Result> Handle(MarcarComoLeidaCommand cmd, CancellationToken ct)
    {
        var notificacion = await repo.ObtenerPorIdAsync(cmd.NotificacionId, ct);
        if (notificacion is null || notificacion.UsuarioId != cmd.UsuarioId)
            return Result.Failure("Notificación no encontrada.");

        var result = notificacion.MarcarComoLeida();
        if (!result.IsSuccess) return result;

        await repo.GuardarAsync(notificacion, ct);
        return Result.Success();
    }
}

// ─── Marcar todas como leídas ─────────────────────────────────────────────────

public record MarcarTodasComoLeidasCommand(Guid UsuarioId) : IRequest<Result>;

internal sealed class MarcarTodasComoLeidasHandler(INotificacionRepository repo)
    : IRequestHandler<MarcarTodasComoLeidasCommand, Result>
{
    public async Task<Result> Handle(MarcarTodasComoLeidasCommand cmd, CancellationToken ct)
    {
        await repo.MarcarTodasComoLeidasAsync(cmd.UsuarioId, ct);
        return Result.Success();
    }
}

// ─── Mapeo ────────────────────────────────────────────────────────────────────

internal static class NotificacionExtensions
{
    public static NotificacionDto ToDto(this Notificacion n) => new(
        n.Id, n.Titulo, n.Mensaje, n.Tipo.ToString(),
        n.Url, n.EsLeida, n.CreadaEn, n.LeidaEn);
}
