using CoreTemplate.Email.Abstractions;
using CoreTemplate.Email.Settings;
using CoreTemplate.Logging.Abstractions;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Email.Providers.Mailjet;

internal sealed class MailjetEmailSender(
    IOptions<MailjetSettings> options,
    IAppLogger logger) : IEmailSender
{
    private readonly MailjetSettings _settings = options.Value;
    private readonly IAppLogger _logger = logger.ForContext<MailjetEmailSender>();

    public async Task<EmailResult> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default)
    {
        try
        {
            var client = new MailjetClient(_settings.ApiKey, _settings.SecretKey);

            var builder = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(_settings.FromEmail, _settings.FromName))
                .WithSubject(mensaje.Asunto)
                .WithHtmlPart(mensaje.CuerpoHtml)
                .WithTo(new SendContact(mensaje.Para, mensaje.NombreDestinatario ?? mensaje.Para));

            if (mensaje.CC is not null)
                foreach (var cc in mensaje.CC)
                    builder.WithCc(new SendContact(cc));

            if (mensaje.Adjuntos is not null)
                builder.WithAttachments(mensaje.Adjuntos.Select(a =>
                    new Attachment(a.NombreArchivo, a.ContentType, Convert.ToBase64String(a.Contenido))).ToList());

            var response = await client.SendTransactionalEmailAsync(builder.Build());
            var msg = response.Messages.FirstOrDefault();

            if (msg?.Status != "success")
            {
                var error = msg?.Errors?.FirstOrDefault()?.ErrorMessage ?? "Error desconocido de Mailjet.";
                _logger.Warning("Mailjet fallo al enviar a {Para}: {Error}", mensaje.Para, error);
                return EmailResult.Fallo(error);
            }

            var messageId = msg.To?.FirstOrDefault()?.MessageID.ToString();
            _logger.Info("Correo enviado via Mailjet a {Para}, asunto: {Asunto}", mensaje.Para, mensaje.Asunto);
            return EmailResult.Ok(messageId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Excepcion al enviar correo via Mailjet a {Para}", mensaje.Para);
            return EmailResult.Fallo(ex.Message);
        }
    }
}
