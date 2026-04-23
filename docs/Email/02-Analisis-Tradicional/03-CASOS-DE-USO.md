# Email — Casos de Uso

> **Building Block:** CoreTemplate.Email
> **Fecha:** 2026-04-22

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| **Módulo Consumidor** | Sistema | Cualquier módulo que inyecta `IEmailSender` (Auth, RRHH, Nómina, Contabilidad, etc.) |
| **Proveedor de Email** | Sistema Externo | Mailjet, SMTP, SendGrid |
| **Destinatario** | Humano | Persona que recibe el correo |

---

## CU-EMAIL-001: Enviar correo simple

**Actor:** Módulo Consumidor
**Precondición:** `IEmailSender` registrado en DI, proveedor configurado en appsettings

**Flujo principal:**
1. El módulo construye un `EmailMessage` con: Para, Asunto, CuerpoHtml
2. Llama a `IEmailSender.EnviarAsync(mensaje)`
3. El building block selecciona el proveedor configurado
4. El proveedor envía el correo
5. Retorna `EmailResult { Exitoso = true, MessageId = "..." }`

**Flujo alternativo — fallo de proveedor:**
1. El proveedor retorna error o lanza excepción
2. El building block captura el error
3. Registra el fallo en log
4. Retorna `EmailResult { Exitoso = false, MensajeError = "..." }`
5. El módulo consumidor decide cómo manejar el fallo

**Ejemplos de uso por módulo:**

```
Auth       → "Restablece tu contraseña" (token de reset)
Auth       → "Tu cuenta ha sido bloqueada"
RRHH       → "Tu solicitud fue recibida" (notificación a candidato)
RRHH       → "Comunicado interno al empleado"
Nómina     → "Tu comprobante de pago está disponible" (con PDF adjunto)
Contabilidad → "Factura adjunta" (con PDF adjunto)
```

---

## CU-EMAIL-002: Enviar correo con adjunto

**Actor:** Módulo Consumidor
**Precondición:** Archivo disponible como `byte[]`

**Flujo principal:**
1. El módulo construye `EmailMessage` incluyendo `Adjuntos`:
   - `NombreArchivo`: "comprobante-enero-2025.pdf"
   - `Contenido`: `byte[]` del archivo
   - `ContentType`: "application/pdf"
2. Llama a `IEmailSender.EnviarAsync(mensaje)`
3. El proveedor adjunta el archivo y envía
4. Retorna `EmailResult { Exitoso = true }`

**Postcondición:** El destinatario recibe el correo con el archivo adjunto descargable

---

## CU-EMAIL-003: Cambiar proveedor de email

**Actor:** Administrador del sistema (configuración)
**Precondición:** Nuevo proveedor soportado por el building block

**Flujo principal:**
1. Administrador modifica `appsettings.json`:
   ```json
   "EmailSettings": { "Provider": "SendGrid" }
   ```
2. Agrega la sección de configuración del nuevo proveedor
3. Reinicia la aplicación
4. El building block registra automáticamente la nueva implementación
5. Todos los módulos consumidores usan el nuevo proveedor sin cambios

**Postcondición:** El sistema envía correos con el nuevo proveedor

---

## CU-EMAIL-004: Diagnóstico de fallo de envío

**Actor:** Módulo Consumidor / Administrador
**Precondición:** Un envío falló

**Flujo principal:**
1. `IEmailSender.EnviarAsync` retorna `EmailResult { Exitoso = false }`
2. El log estructurado registra: proveedor, destinatario, asunto, error
3. El administrador consulta los logs con el `X-Correlation-Id` del request
4. Identifica si el fallo es de configuración, credenciales o del proveedor externo

---

## Matriz de uso por módulo

| Módulo | CU-001 | CU-002 | Casos de uso típicos |
|---|---|---|---|
| Auth | ✅ | — | Reset password, bloqueo de cuenta |
| RRHH | ✅ | ✅ | Notificaciones candidatos, comunicados |
| Nómina | ✅ | ✅ | Comprobantes de pago (PDF adjunto) |
| Contabilidad | ✅ | ✅ | Facturas, estados de cuenta |
| Módulo futuro | ✅ | ✅ | Cualquier notificación por correo |

---

**Fecha:** 2026-04-22
