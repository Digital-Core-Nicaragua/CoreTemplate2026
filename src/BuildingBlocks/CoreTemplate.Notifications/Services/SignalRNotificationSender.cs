using CoreTemplate.Notifications.Abstractions;
using CoreTemplate.Notifications.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoreTemplate.Notifications.Services;

/// <summary>
/// Implementación de INotificationSender usando SignalR.
/// Solo maneja la entrega en tiempo real — la persistencia en BD
/// la hace el módulo Notificaciones que envuelve este sender.
/// </summary>
public sealed class SignalRNotificationSender(IHubContext<NotificationHub> hub) : INotificationSender
{
    public async Task<NotificationResult> EnviarAsync(
        NotificationMessage mensaje, CancellationToken ct = default)
    {
        try
        {
            await hub.Clients
                .Group($"user-{mensaje.UsuarioId}")
                .SendAsync("RecibirNotificacion", new
                {
                    titulo = mensaje.Titulo,
                    mensaje = mensaje.Mensaje,
                    tipo = mensaje.Tipo.ToString(),
                    url = mensaje.Url,
                    creadaEn = DateTime.UtcNow
                }, ct);

            return new NotificationResult(true, EntregadaEnTiempoReal: true);
        }
        catch
        {
            // Si SignalR falla (usuario desconectado), no es un error crítico
            return new NotificationResult(true, EntregadaEnTiempoReal: false);
        }
    }

    public async Task EnviarATenantAsync(
        Guid tenantId, string titulo, string mensaje,
        TipoNotificacion tipo = TipoNotificacion.Info,
        CancellationToken ct = default)
    {
        await hub.Clients
            .Group($"tenant-{tenantId}")
            .SendAsync("RecibirNotificacion", new
            {
                titulo,
                mensaje,
                tipo = tipo.ToString(),
                url = (string?)null,
                creadaEn = DateTime.UtcNow
            }, ct);
    }
}
