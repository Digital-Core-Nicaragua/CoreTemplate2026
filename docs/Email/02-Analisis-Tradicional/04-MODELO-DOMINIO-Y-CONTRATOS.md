# Email — Modelo de Dominio y Contratos

> **Building Block:** CoreTemplate.Email
> **Fecha:** 2026-04-22

---

## Nota sobre el modelo

`CoreTemplate.Email` es infraestructura pura — no tiene aggregates, entidades ni base de datos.
Su "modelo" son los contratos (interfaces y records) que definen cómo los módulos
interactúan con el servicio de email.

---

## Contratos principales

### IEmailSender

```
IEmailSender
  + EnviarAsync(EmailMessage mensaje) : Task<EmailResult>
```

Contrato único. Todos los proveedores implementan esta interfaz.
Los módulos consumidores solo conocen esta interfaz.

---

### EmailMessage

```
EmailMessage (record)
  + Para            : string          -- dirección de correo del destinatario
  + Asunto          : string          -- asunto del correo
  + CuerpoHtml      : string          -- contenido HTML del correo
  + NombreDestinatario : string?      -- nombre para mostrar (opcional)
  + CC              : string[]?       -- destinatarios en copia (opcional)
  + Adjuntos        : EmailAdjunto[]? -- archivos adjuntos (opcional)
```

---

### EmailAdjunto

```
EmailAdjunto (record)
  + NombreArchivo   : string   -- "comprobante-enero-2025.pdf"
  + Contenido       : byte[]   -- contenido binario del archivo
  + ContentType     : string   -- "application/pdf", "image/jpeg", etc.
```

---

### EmailResult

```
EmailResult (record)
  + Exitoso         : bool
  + MessageId       : string?  -- ID del mensaje retornado por el proveedor (si aplica)
  + MensajeError    : string?  -- descripción del error si Exitoso = false
```

---

## Configuración en appsettings

```json
{
  "EmailSettings": {
    "Provider": "Mailjet"
  },
  "MailjetSettings": {
    "ApiKey": "<api-key>",
    "SecretKey": "<secret-key>",
    "FromEmail": "noreply@misistema.com",
    "FromName": "Mi Sistema"
  },
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 1025,
    "UseSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@misistema.com",
    "FromName": "Mi Sistema"
  },
  "SendGridSettings": {
    "ApiKey": "<api-key>",
    "FromEmail": "noreply@misistema.com",
    "FromName": "Mi Sistema"
  }
}
```

---

## Estructura de proyectos

```
src/BuildingBlocks/CoreTemplate.Email/
  Abstractions/
    IEmailSender.cs
    EmailMessage.cs
    EmailAdjunto.cs
    EmailResult.cs
  Providers/
    Mailjet/
      MailjetEmailSender.cs
      MailjetSettings.cs
    Smtp/
      SmtpEmailSender.cs
      SmtpSettings.cs
    SendGrid/
      SendGridEmailSender.cs       -- estructura base, implementación pendiente
      SendGridSettings.cs
  Settings/
    EmailSettings.cs               -- solo contiene "Provider"
  DependencyInjection.cs           -- lee Provider, registra implementación correcta
  CoreTemplate.Email.csproj
```

---

## Diagrama de dependencias

```
Módulo Auth
Módulo RRHH          ──► IEmailSender ──► MailjetEmailSender ──► Mailjet API
Módulo Nómina                         ──► SmtpEmailSender    ──► Servidor SMTP
Módulo Contabilidad                   ──► SendGridEmailSender ──► SendGrid API
```

Los módulos solo dependen de `IEmailSender`.
El proveedor activo se resuelve en tiempo de ejecución según configuración.

---

## Cómo lo usa un módulo (ejemplo Auth)

```csharp
// En el handler de SolicitarRestablecimientoCommand
public class SolicitarRestablecimientoHandler(
    IUsuarioRepository repo,
    IEmailSender emailSender)
{
    public async Task<Result> Handle(SolicitarRestablecimientoCommand cmd)
    {
        var usuario = await repo.ObtenerPorEmailAsync(cmd.Email);
        // ... lógica de negocio ...

        var resultado = await emailSender.EnviarAsync(new EmailMessage(
            Para: usuario.Email.Valor,
            Asunto: "Restablece tu contraseña",
            CuerpoHtml: $"<p>Tu token es: <b>{token}</b></p>",
            NombreDestinatario: usuario.Nombre
        ));

        // El fallo de email no interrumpe el flujo — el token ya está guardado
        if (!resultado.Exitoso)
            logger.LogWarning("Fallo al enviar email de reset: {Error}", resultado.MensajeError);

        return Result.Success();
    }
}
```

---

**Fecha:** 2026-04-22
