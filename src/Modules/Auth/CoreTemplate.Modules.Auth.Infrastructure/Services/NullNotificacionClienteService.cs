using CoreTemplate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementacion no-op de INotificacionClienteService.
/// Se usa cuando RegistroPorTelefono.Enabled = false o cuando el sistema
/// no ha registrado su propia implementacion (Twilio, AWS SNS, etc.).
/// Solo loguea el OTP — util para desarrollo y pruebas.
/// </summary>
internal sealed class NullNotificacionClienteService(
    ILogger<NullNotificacionClienteService> _logger) : INotificacionClienteService
{
    public Task EnviarOtpWhatsAppAsync(string telefono, string codigo, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "[DEV] OTP WhatsApp para {Telefono}: {Codigo} — Implementa INotificacionClienteService para envio real.",
            telefono, codigo);
        return Task.CompletedTask;
    }

    public Task EnviarOtpSmsAsync(string telefono, string codigo, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "[DEV] OTP SMS para {Telefono}: {Codigo} — Implementa INotificacionClienteService para envio real.",
            telefono, codigo);
        return Task.CompletedTask;
    }
}
