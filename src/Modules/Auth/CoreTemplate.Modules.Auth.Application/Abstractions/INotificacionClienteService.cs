namespace CoreTemplate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Contrato para el envío de notificaciones a clientes del portal.
/// CoreTemplate define la interfaz — cada sistema implementa el proveedor (Twilio, AWS SNS, etc.).
/// Solo se usa cuando <c>CustomerPortalSettings:RegistroPorTelefono:Enabled = true</c>.
/// </summary>
public interface INotificacionClienteService
{
    /// <summary>Envía un código OTP al número de teléfono por WhatsApp.</summary>
    Task EnviarOtpWhatsAppAsync(string telefono, string codigo, CancellationToken ct = default);

    /// <summary>Envía un código OTP al número de teléfono por SMS.</summary>
    Task EnviarOtpSmsAsync(string telefono, string codigo, CancellationToken ct = default);
}
