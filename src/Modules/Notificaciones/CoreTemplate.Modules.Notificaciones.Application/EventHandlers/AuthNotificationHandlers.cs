using CoreTemplate.Modules.Auth.Domain.Events;
using CoreTemplate.Modules.Notificaciones.Application.Commands;
using CoreTemplate.Notifications.Abstractions;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CoreTemplate.Modules.Notificaciones.Application.EventHandlers;

// Wrappers INotification para eventos de Auth (IDomainEvent no implementa INotification de MediatR)
public record UsuarioBloqueadoNotificacionNotification(UsuarioBloqueadoEvent Evento) : INotification;
public record PasswordCambiadoNotificacionNotification(PasswordCambiadoEvent Evento) : INotification;

internal sealed class UsuarioBloqueadoNotificacionHandler(
    ISender sender, IConfiguration config)
    : INotificationHandler<UsuarioBloqueadoNotificacionNotification>
{
    public async Task Handle(UsuarioBloqueadoNotificacionNotification notification, CancellationToken ct)
    {
        if (!config.GetValue("NotificationSettings:Handlers:UsuarioBloqueado", true)) return;

        var ev = notification.Evento;
        await sender.Send(new EnviarNotificacionCommand(
            ev.UsuarioId,
            "Cuenta bloqueada temporalmente",
            $"Tu cuenta fue bloqueada hasta {ev.BloqueadoHasta:dd/MM/yyyy HH:mm} UTC.",
            TipoNotificacion.Advertencia), ct);
    }
}

internal sealed class PasswordCambiadoNotificacionHandler(
    ISender sender, IConfiguration config)
    : INotificationHandler<PasswordCambiadoNotificacionNotification>
{
    public async Task Handle(PasswordCambiadoNotificacionNotification notification, CancellationToken ct)
    {
        if (!config.GetValue("NotificationSettings:Handlers:PasswordCambiado", true)) return;

        var ev = notification.Evento;
        await sender.Send(new EnviarNotificacionCommand(
            ev.UsuarioId,
            "Tu contraseña fue cambiada",
            "Si no realizaste este cambio, cierra sesión en todos los dispositivos.",
            TipoNotificacion.Seguridad,
            "/perfil/sesiones"), ct);
    }
}
