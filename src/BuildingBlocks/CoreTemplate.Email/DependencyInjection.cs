using CoreTemplate.Email.Abstractions;
using CoreTemplate.Email.Providers.Mailjet;
using CoreTemplate.Email.Providers.SendGrid;
using CoreTemplate.Email.Providers.Smtp;
using CoreTemplate.Email.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Email;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        var provider = configuration[$"{EmailSettings.SectionName}:Provider"] ?? "Smtp";

        switch (provider.ToLowerInvariant())
        {
            case "mailjet":
                services.Configure<MailjetSettings>(configuration.GetSection(MailjetSettings.SectionName));
                services.AddScoped<IEmailSender, MailjetEmailSender>();
                break;

            case "smtp":
                services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                break;

            case "sendgrid":
                services.Configure<SendGridSettings>(configuration.GetSection(SendGridSettings.SectionName));
                services.AddScoped<IEmailSender, SendGridEmailSender>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Proveedor de email '{provider}' no reconocido. Valores válidos: Mailjet, Smtp, SendGrid.");
        }

        return services;
    }
}
