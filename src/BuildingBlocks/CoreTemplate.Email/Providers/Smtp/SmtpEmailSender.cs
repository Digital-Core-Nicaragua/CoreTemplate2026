using CoreTemplate.Email.Abstractions;
using CoreTemplate.Email.Settings;
using CoreTemplate.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CoreTemplate.Email.Providers.Smtp;

internal sealed class SmtpEmailSender(
    IOptions<SmtpSettings> options,
    IAppLogger logger) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;
    private readonly IAppLogger _logger = logger.ForContext<SmtpEmailSender>();

    public async Task<EmailResult> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                    ? null
                    : new NetworkCredential(_settings.Username, _settings.Password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = mensaje.Asunto,
                Body = mensaje.CuerpoHtml,
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(mensaje.Para, mensaje.NombreDestinatario ?? mensaje.Para));

            if (mensaje.CC is not null)
                foreach (var cc in mensaje.CC)
                    mail.CC.Add(cc);

            if (mensaje.Adjuntos is not null)
                foreach (var adj in mensaje.Adjuntos)
                    mail.Attachments.Add(new Attachment(new MemoryStream(adj.Contenido), adj.NombreArchivo, adj.ContentType));

            await client.SendMailAsync(mail, ct);

            _logger.Info("Correo enviado via SMTP a {Para}, asunto: {Asunto}", mensaje.Para, mensaje.Asunto);
            return EmailResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Excepcion al enviar correo via SMTP a {Para}", mensaje.Para);
            return EmailResult.Fallo(ex.Message);
        }
    }
}
