namespace CoreTemplate.Email.Settings;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string Provider { get; set; } = "Smtp";
}

public sealed class MailjetSettings
{
    public const string SectionName = "MailjetSettings";
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public sealed class SmtpSettings
{
    public const string SectionName = "SmtpSettings";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; } = false;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public sealed class SendGridSettings
{
    public const string SectionName = "SendGridSettings";
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
