using CoreTemplate.Email.Abstractions;
using CoreTemplate.Email.Settings;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Email.Providers.SendGrid;

/// <summary>
/// Implementación de IEmailSender para SendGrid.
/// Instalar paquete: SendGrid (Twilio.SendGrid)
/// </summary>
internal sealed class SendGridEmailSender(IOptions<SendGridSettings> options) : IEmailSender
{
    private readonly SendGridSettings _settings = options.Value;

    public Task<EmailResult> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default)
    {
        // TODO: Implementar con SendGrid SDK
        // var client = new SendGridClient(_settings.ApiKey);
        // var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        // var to = new EmailAddress(mensaje.Para, mensaje.NombreDestinatario);
        // var msg = MailHelper.CreateSingleEmail(from, to, mensaje.Asunto, null, mensaje.CuerpoHtml);
        // var response = await client.SendEmailAsync(msg, ct);
        throw new NotImplementedException("SendGrid no está implementado aún. Cambia Provider a 'Mailjet' o 'Smtp'.");
    }
}
