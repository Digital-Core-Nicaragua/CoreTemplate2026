using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoreTemplate.Notifications.Hubs;

/// <summary>
/// SignalR Hub para notificaciones en tiempo real.
/// Requiere JWT válido — el token se pasa como query string: ?access_token={jwt}
/// (requerido por SignalR para WebSockets).
/// Cada usuario se une a su grupo personal "user-{userId}" y al grupo de su tenant "tenant-{tenantId}".
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (!string.IsNullOrEmpty(tenantId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (!string.IsNullOrEmpty(tenantId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");

        await base.OnDisconnectedAsync(exception);
    }
}
