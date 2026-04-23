# Email — Requerimientos No Funcionales

> **Building Block:** CoreTemplate.Email
> **Fecha:** 2026-04-22

---

## RNF-EMAIL-001: Intercambiabilidad de proveedor
**Categoría:** Mantenibilidad

- Cambiar de proveedor requiere solo modificar `appsettings.json`
- Cero cambios en módulos consumidores
- La abstracción `IEmailSender` no expone detalles de ningún proveedor
- Agregar un nuevo proveedor requiere solo: implementar `IEmailSender` + registrar en DI

---

## RNF-EMAIL-002: Aislamiento de fallos
**Categoría:** Resiliencia

- Un fallo en el envío de correo NO debe interrumpir el flujo de negocio principal
- El consumidor decide si el fallo es crítico o puede ignorarse
- `IEmailSender` retorna `EmailResult` — nunca lanza excepciones al consumidor
- Ejemplo: si falla el correo de reset de contraseña, el token ya fue guardado en BD;
  el usuario puede reintentar la solicitud

---

## RNF-EMAIL-003: Configuración segura
**Categoría:** Seguridad

- Las credenciales del proveedor (ApiKey, SecretKey, Password SMTP) NUNCA van en código
- Se leen exclusivamente desde `IConfiguration` (appsettings, variables de entorno, secrets)
- En producción se recomienda usar variables de entorno o AWS Secrets Manager
- El contenido de los correos no se registra en logs

---

## RNF-EMAIL-004: Tiempo de respuesta
**Categoría:** Rendimiento

- El envío es siempre asíncrono (`async/await`)
- Timeout configurable por proveedor (default: 30 segundos)
- No bloquea el hilo del request HTTP del módulo consumidor

---

## RNF-EMAIL-005: Observabilidad
**Categoría:** Operabilidad

- Cada envío (exitoso o fallido) genera una entrada en el log estructurado
- Los logs incluyen: proveedor, destinatario (enmascarado si se requiere), asunto, resultado
- Compatible con el sistema de correlación (`X-Correlation-Id`) de `CoreTemplate.Logging`

---

## RNF-EMAIL-006: Extensibilidad
**Categoría:** Mantenibilidad

- Agregar soporte para templates (Razor, Scriban) no requiere cambiar la abstracción
- Agregar reintentos automáticos (Polly) no requiere cambiar los consumidores
- La abstracción está preparada para envío en cola (background job) en el futuro

---

**Fecha:** 2026-04-22
