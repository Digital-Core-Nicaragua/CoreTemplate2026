using CoreTemplate.Modules.Auth.Domain.Events;
using CoreTemplate.Modules.EmailTemplates.Application.Abstractions;
using CoreTemplate.Logging.Abstractions;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CoreTemplate.Modules.EmailTemplates.Application.EventHandlers;

// Los eventos de Auth implementan IDomainEvent pero no INotification de MediatR.
// Se usan wrappers de notificación para publicarlos via MediatR.IPublisher
// desde los handlers de Auth. Aquí registramos los handlers que los consumen.

public record RestablecimientoSolicitadoNotification(RestablecimientoSolicitadoEvent Evento) : INotification;
public record UsuarioBloqueadoNotification(UsuarioBloqueadoEvent Evento) : INotification;
public record PasswordCambiadoNotification(PasswordCambiadoEvent Evento) : INotification;
public record DosFactoresActivadoNotification(DosFactoresActivadoEvent Evento) : INotification;
public record UsuarioRegistradoNotification(UsuarioRegistradoEvent Evento) : INotification;

internal sealed class RestablecimientoSolicitadoHandler(
    IEmailTemplateSender sender,
    IConfiguration config,
    IAppLogger logger) : INotificationHandler<RestablecimientoSolicitadoNotification>
{
    public async Task Handle(RestablecimientoSolicitadoNotification notification, CancellationToken ct)
    {
        var ev = notification.Evento;
        if (!config.GetValue("EmailTemplateSettings:Handlers:RestablecimientoSolicitado", true)) return;

        var baseUrl = config["AppSettings:Url"] ?? string.Empty;
        var result = await sender.EnviarAsync(new EnviarConPlantillaRequest(
            "auth.reset-password",
            ev.Email,
            new Dictionary<string, string>
            {
                ["NombreUsuario"] = ev.Email,
                ["LinkReset"] = $"{baseUrl}/reset-password?token={ev.Token}",
                ["ExpiraEn"] = $"{(ev.ExpiraEn - DateTime.UtcNow).TotalHours:F0} hora(s)"
            }), ct);

        if (!result.Exitoso)
            logger.ForContext<RestablecimientoSolicitadoHandler>()
                  .Warning("Fallo al enviar email de reset a {Email}: {Error}", ev.Email, result.MensajeError ?? string.Empty);
    }
}

internal sealed class UsuarioBloqueadoHandler(
    IEmailTemplateSender sender,
    IConfiguration config,
    IAppLogger logger) : INotificationHandler<UsuarioBloqueadoNotification>
{
    public async Task Handle(UsuarioBloqueadoNotification notification, CancellationToken ct)
    {
        var ev = notification.Evento;
        if (!config.GetValue("EmailTemplateSettings:Handlers:UsuarioBloqueado", true)) return;

        var result = await sender.EnviarAsync(new EnviarConPlantillaRequest(
            "auth.cuenta-bloqueada",
            ev.Email,
            new Dictionary<string, string>
            {
                ["NombreUsuario"] = ev.Email,
                ["BloqueadaHasta"] = ev.BloqueadoHasta.ToString("dd/MM/yyyy HH:mm") + " UTC"
            }), ct);

        if (!result.Exitoso)
            logger.ForContext<UsuarioBloqueadoHandler>()
                  .Warning("Fallo al enviar email de bloqueo a {Email}: {Error}", ev.Email, result.MensajeError ?? string.Empty);
    }
}

internal sealed class PasswordCambiadoHandler(
    IEmailTemplateSender sender,
    IConfiguration config,
    IAppLogger logger) : INotificationHandler<PasswordCambiadoNotification>
{
    public async Task Handle(PasswordCambiadoNotification notification, CancellationToken ct)
    {
        var ev = notification.Evento;
        if (!config.GetValue("EmailTemplateSettings:Handlers:PasswordCambiado", true)) return;

        var result = await sender.EnviarAsync(new EnviarConPlantillaRequest(
            "auth.password-cambiado",
            ev.Email,
            new Dictionary<string, string>
            {
                ["NombreUsuario"] = ev.Email,
                ["FechaCambio"] = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC"
            }), ct);

        if (!result.Exitoso)
            logger.ForContext<PasswordCambiadoHandler>()
                  .Warning("Fallo al enviar email de cambio de password a {Email}: {Error}", ev.Email, result.MensajeError ?? string.Empty);
    }
}

internal sealed class DosFactoresActivadoHandler(
    IEmailTemplateSender sender,
    IConfiguration config,
    IAppLogger logger) : INotificationHandler<DosFactoresActivadoNotification>
{
    public async Task Handle(DosFactoresActivadoNotification notification, CancellationToken ct)
    {
        var ev = notification.Evento;
        if (!config.GetValue("EmailTemplateSettings:Handlers:DosFactoresActivado", true)) return;

        var result = await sender.EnviarAsync(new EnviarConPlantillaRequest(
            "auth.2fa-activado",
            ev.Email,
            new Dictionary<string, string>
            {
                ["NombreUsuario"] = ev.Email,
                ["FechaActivacion"] = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC"
            }), ct);

        if (!result.Exitoso)
            logger.ForContext<DosFactoresActivadoHandler>()
                  .Warning("Fallo al enviar email de 2FA activado a {Email}: {Error}", ev.Email, result.MensajeError ?? string.Empty);
    }
}

internal sealed class UsuarioRegistradoHandler(
    IEmailTemplateSender sender,
    IConfiguration config,
    IAppLogger logger) : INotificationHandler<UsuarioRegistradoNotification>
{
    public async Task Handle(UsuarioRegistradoNotification notification, CancellationToken ct)
    {
        var ev = notification.Evento;
        if (!config.GetValue("EmailTemplateSettings:Handlers:UsuarioRegistrado", false)) return;

        var baseUrl = config["AppSettings:Url"] ?? string.Empty;
        var result = await sender.EnviarAsync(new EnviarConPlantillaRequest(
            "auth.bienvenida",
            ev.Email,
            new Dictionary<string, string>
            {
                ["NombreUsuario"] = ev.Nombre,
                ["LinkAcceso"] = baseUrl
            }), ct);

        if (!result.Exitoso)
            logger.ForContext<UsuarioRegistradoHandler>()
                  .Warning("Fallo al enviar email de bienvenida a {Email}: {Error}", ev.Email, result.MensajeError ?? string.Empty);
    }
}
