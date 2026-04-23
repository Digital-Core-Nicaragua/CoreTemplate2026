# Email — Requerimientos Funcionales

> **Building Block:** CoreTemplate.Email
> **Fecha:** 2026-04-22
> **Total:** 8 RF

---

## Contexto

`CoreTemplate.Email` es un building block transversal de infraestructura.
No es un módulo de negocio — no tiene aggregates propios ni base de datos.
Cualquier módulo del sistema puede inyectar `IEmailSender` y enviar correos
sin conocer el proveedor subyacente.

**Módulos consumidores previstos:**
- Auth → reset de contraseña, notificaciones de seguridad
- RRHH → notificaciones a candidatos, comunicados internos
- Nómina → comprobantes de pago
- Contabilidad → facturas, estados de cuenta
- Cualquier módulo futuro que requiera notificaciones por correo

---

## RF-EMAIL-001: Envío de correo simple
**Prioridad:** Crítica

### Descripción
El sistema permite enviar un correo electrónico con asunto y cuerpo HTML a un destinatario.

### Criterios de Aceptación
- Recibe: dirección de correo, nombre del destinatario (opcional), asunto, cuerpo HTML
- Retorna un resultado con indicador de éxito/fallo y mensaje de error si aplica
- El remitente se configura en `appsettings.json` (no se pasa por código)
- Si el envío falla, retorna `EmailResult` con `Exitoso = false` y descripción del error
- No lanza excepciones al consumidor — encapsula el error en el resultado

---

## RF-EMAIL-002: Envío de correo con adjuntos
**Prioridad:** Alta

### Descripción
El sistema permite adjuntar uno o más archivos a un correo electrónico.

### Criterios de Aceptación
- Recibe adjuntos como `byte[]` con nombre y tipo MIME
- Soporta múltiples adjuntos en un mismo envío
- Si no hay adjuntos, el comportamiento es idéntico a RF-EMAIL-001
- Tamaño máximo de adjuntos configurable (default: 10 MB total)

---

## RF-EMAIL-003: Envío a múltiples destinatarios
**Prioridad:** Media

### Descripción
El sistema permite enviar un correo a múltiples destinatarios en un solo llamado.

### Criterios de Aceptación
- Soporta lista de destinatarios principales (Para)
- Soporta destinatarios en copia (CC) opcionales
- Máximo de destinatarios configurable (default: 50)

---

## RF-EMAIL-004: Proveedor configurable sin cambio de código
**Prioridad:** Crítica

### Descripción
El proveedor de envío de correos se configura en `appsettings.json`.
Cambiar de proveedor no requiere modificar ningún módulo consumidor.

### Criterios de Aceptación
- Proveedores soportados: `Mailjet`, `Smtp`, `SendGrid`
- Configuración: `"EmailSettings": { "Provider": "Mailjet" }`
- Si el proveedor configurado no existe → error al iniciar la aplicación (fail-fast)
- Cada proveedor tiene su propia sección de configuración en appsettings

---

## RF-EMAIL-005: Proveedor Mailjet
**Prioridad:** Crítica

### Descripción
Implementación del envío usando la API transaccional de Mailjet.

### Criterios de Aceptación
- Configuración: `ApiKey`, `SecretKey`, `FromEmail`, `FromName`
- Usa el SDK oficial `Mailjet.Api`
- Valida la respuesta de Mailjet y mapea errores a `EmailResult`
- Soporta adjuntos en formato Base64 (requerido por Mailjet)

---

## RF-EMAIL-006: Proveedor SMTP
**Prioridad:** Alta

### Descripción
Implementación del envío usando SMTP estándar (para desarrollo local o servidores propios).

### Criterios de Aceptación
- Configuración: `Host`, `Port`, `UseSsl`, `Username`, `Password`
- Funciona sin credenciales (relay local para desarrollo)
- Soporta adjuntos nativamente via `System.Net.Mail`

---

## RF-EMAIL-007: Proveedor SendGrid (preparado)
**Prioridad:** Baja

### Descripción
Interfaz y estructura preparada para SendGrid. Implementación completa cuando se requiera.

### Criterios de Aceptación
- La abstracción `IEmailSender` ya soporta SendGrid sin cambios
- Clase `SendGridEmailSender` con estructura base creada
- Activar con `"Provider": "SendGrid"` en appsettings

---

## RF-EMAIL-008: Registro de resultado en log
**Prioridad:** Alta

### Descripción
Cada intento de envío queda registrado en el log estructurado del sistema.

### Criterios de Aceptación
- Log de éxito: destinatario, asunto, proveedor, messageId (si el proveedor lo retorna)
- Log de fallo: destinatario, asunto, proveedor, mensaje de error
- Usa `IAppLogger` del building block `CoreTemplate.Logging`
- No registra el contenido del correo (privacidad)

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 3 |
| Alta | 3 |
| Media | 1 |
| Baja | 1 |
| **Total** | **8** |

---

**Fecha:** 2026-04-22
