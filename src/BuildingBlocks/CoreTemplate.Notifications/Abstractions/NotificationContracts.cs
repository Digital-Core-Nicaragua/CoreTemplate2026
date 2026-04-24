namespace CoreTemplate.Notifications.Abstractions;

public enum TipoNotificacion { Info, Exito, Advertencia, Error, Seguridad }

/// <summary>Modelo de notificación a enviar.</summary>
public record NotificationMessage(
    Guid UsuarioId,
    string Titulo,
    string Mensaje,
    TipoNotificacion Tipo = TipoNotificacion.Info,
    string? Url = null,
    Guid? TenantId = null);

/// <summary>Resultado del intento de envío.</summary>
public record NotificationResult(bool Exitoso, bool EntregadaEnTiempoReal);

/// <summary>
/// Contrato para enviar notificaciones.
/// Siempre guarda en BD y además intenta entrega en tiempo real via SignalR.
/// Si el usuario no está conectado, la notificación queda en BD para entrega posterior.
/// </summary>
public interface INotificationSender
{
    Task<NotificationResult> EnviarAsync(
        NotificationMessage mensaje, CancellationToken ct = default);

    Task EnviarATenantAsync(
        Guid tenantId, string titulo, string mensaje,
        TipoNotificacion tipo = TipoNotificacion.Info,
        CancellationToken ct = default);
}
