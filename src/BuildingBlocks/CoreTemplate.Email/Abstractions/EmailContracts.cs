namespace CoreTemplate.Email.Abstractions;

/// <summary>
/// Representa un archivo adjunto a incluir en un correo electrónico.
/// </summary>
/// <param name="NombreArchivo">Nombre del archivo con extensión. Ej: "comprobante.pdf"</param>
/// <param name="Contenido">Contenido binario del archivo.</param>
/// <param name="ContentType">Tipo MIME del archivo. Ej: "application/pdf"</param>
public record EmailAdjunto(string NombreArchivo, byte[] Contenido, string ContentType);

/// <summary>
/// Modelo que representa un correo electrónico a enviar.
/// Los módulos consumidores construyen este objeto y lo pasan a <see cref="IEmailSender"/>.
/// En la mayoría de casos los módulos usan <c>IEmailTemplateSender</c> en lugar de
/// construir <c>EmailMessage</c> directamente.
/// </summary>
/// <param name="Para">Dirección de correo del destinatario principal.</param>
/// <param name="Asunto">Asunto del correo. Puede contener variables ya renderizadas.</param>
/// <param name="CuerpoHtml">Cuerpo HTML del correo. Puede contener variables ya renderizadas.</param>
/// <param name="NombreDestinatario">Nombre para mostrar del destinatario. Opcional.</param>
/// <param name="CC">Destinatarios en copia. Opcional.</param>
/// <param name="Adjuntos">Archivos adjuntos. Opcional.</param>
public record EmailMessage(
    string Para,
    string Asunto,
    string CuerpoHtml,
    string? NombreDestinatario = null,
    IEnumerable<string>? CC = null,
    IEnumerable<EmailAdjunto>? Adjuntos = null);

/// <summary>
/// Resultado de un intento de envío de correo electrónico.
/// <para>
/// <c>IEmailSender</c> nunca lanza excepciones al consumidor — siempre retorna
/// este resultado. El consumidor decide si el fallo es crítico o puede ignorarse.
/// </para>
/// </summary>
public record EmailResult(bool Exitoso, string? MessageId = null, string? MensajeError = null)
{
    /// <summary>Crea un resultado exitoso con el MessageId opcional del proveedor.</summary>
    public static EmailResult Ok(string? messageId = null) => new(true, messageId);

    /// <summary>Crea un resultado fallido con la descripción del error.</summary>
    public static EmailResult Fallo(string error) => new(false, null, error);
}

/// <summary>
/// Contrato para el envío de correos electrónicos.
/// <para>
/// El proveedor activo (Mailjet, SMTP, SendGrid) se configura en
/// <c>appsettings.json → EmailSettings:Provider</c> y se resuelve
/// automáticamente en el DI. Cambiar de proveedor no requiere modificar
/// ningún módulo consumidor.
/// </para>
/// <para>
/// Los módulos de negocio NO deben inyectar esta interfaz directamente.
/// Deben usar <c>IEmailTemplateSender</c> del módulo EmailTemplates para
/// garantizar diseño corporativo consistente en todos los correos.
/// </para>
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Envía un correo electrónico usando el proveedor configurado.
    /// Nunca lanza excepciones — los errores se encapsulan en <see cref="EmailResult"/>.
    /// </summary>
    Task<EmailResult> EnviarAsync(EmailMessage mensaje, CancellationToken ct = default);
}
