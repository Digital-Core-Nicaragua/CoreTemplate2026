using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using CoreTemplate.Modules.Notificaciones.Domain.Repositories;
using CoreTemplate.Notifications.Abstractions;
using CoreTemplate.Notifications.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoreTemplate.Modules.Notificaciones.Infrastructure.Services;

/// <summary>
/// Servicio que entrega notificaciones pendientes al usuario cuando se conecta al Hub.
/// Se llama desde NotificationHub.OnConnectedAsync via IHubFilter o desde el Hub directamente.
/// </summary>
public sealed class NotificacionPendienteService(
    INotificacionRepository repo,
    IHubContext<NotificationHub> hub)
{
    public async Task EntregarPendientesAsync(Guid usuarioId, string connectionId, CancellationToken ct = default)
    {
        var pendientes = await repo.ObtenerNoLeidasAsync(usuarioId, ct);
        if (!pendientes.Any()) return;

        foreach (var n in pendientes.Where(x => !x.EntregadaEnTiempoReal))
        {
            await hub.Clients.Client(connectionId).SendAsync("RecibirNotificacion", new
            {
                id = n.Id,
                titulo = n.Titulo,
                mensaje = n.Mensaje,
                tipo = n.Tipo.ToString(),
                url = n.Url,
                creadaEn = n.CreadaEn
            }, ct);
        }
    }
}
