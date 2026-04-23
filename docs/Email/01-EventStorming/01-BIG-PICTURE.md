# Event Storming — Big Picture
# Building Block: CoreTemplate.Email

> **Nivel:** Big Picture + Process Level
> **Fecha:** 2026-04-22
> **Nota:** Email es infraestructura transversal — no tiene Bounded Context propio.
> Los eventos de dominio pertenecen a los módulos consumidores.
> Este documento captura los eventos de infraestructura y los puntos de integración.

---

## Leyenda

| Símbolo | Color | Elemento |
|---|---|---|
| 🟠 | Naranja | Evento de infraestructura |
| 🔵 | Azul | Comando |
| 🟣 | Morado | Política |
| 🔴 | Rojo | Hotspot |
| ⚡ | — | Evento externo (del módulo consumidor) |
| 🤖 | — | Sistema / Módulo consumidor |

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| 🤖 **Módulo Auth** | Sistema | Envía correos de seguridad (reset, bloqueo) |
| 🤖 **Módulo RRHH** | Sistema | Notificaciones a candidatos y empleados |
| 🤖 **Módulo Nómina** | Sistema | Comprobantes de pago |
| 🤖 **Módulo Contabilidad** | Sistema | Facturas, estados de cuenta |
| 🌐 **Mailjet** | Sistema Externo | Proveedor de envío transaccional |
| 🌐 **SMTP Server** | Sistema Externo | Servidor de correo (dev/producción propia) |
| 🌐 **SendGrid** | Sistema Externo | Proveedor alternativo (preparado) |

---

## Flujo: Envío de correo simple

```
🤖 Módulo Consumidor → 🔵 EnviarCorreo { para, asunto, cuerpoHtml }
    IEmailSender → Seleccionar proveedor según configuración
    IEmailSender → Construir mensaje para el proveedor
    IEmailSender → Llamar API del proveedor

    3a. SI envío exitoso:
        🟠 CorreoEnviado { destinatario, asunto, proveedor, messageId }
        → Retornar EmailResult { Exitoso = true }

    3b. SI fallo del proveedor:
        🟠 CorreoFallido { destinatario, asunto, proveedor, error }
        → Retornar EmailResult { Exitoso = false, MensajeError }
        🟣 POLÍTICA: Registrar en log estructurado (nunca lanzar excepción al consumidor)
```

---

## Flujo: Envío con adjunto

```
🤖 Módulo Consumidor → 🔵 EnviarCorreoConAdjunto { para, asunto, html, adjuntos[] }
    IEmailSender → Validar tamaño total de adjuntos
    
    2a. SI supera límite configurado:
        🟠 AdjuntoRechazado { razon: "TamanioExcedido" }
        → Retornar EmailResult { Exitoso = false }
    
    2b. SI tamaño válido:
        IEmailSender → Convertir adjuntos al formato del proveedor
        IEmailSender → Enviar con adjuntos
        🟠 CorreoConAdjuntoEnviado { destinatario, asunto, cantidadAdjuntos, proveedor }
        → Retornar EmailResult { Exitoso = true }
```

---

## Flujo: Cambio de proveedor (operación de configuración)

```
🤖 Administrador → Modifica appsettings.json { Provider: "SendGrid" }
    🔵 ReiniciarAplicacion
    DependencyInjection → Leer EmailSettings.Provider
    
    3a. SI proveedor no existe:
        🟠 ProveedorEmailInvalido { provider: "Desconocido" }
        → Aplicación no inicia (fail-fast)
    
    3b. SI proveedor válido:
        DependencyInjection → Registrar nueva implementación de IEmailSender
        🟠 ProveedorEmailConfigurado { provider: "SendGrid" }
        → Aplicación inicia normalmente
```

---

## Integración con módulos consumidores

### Auth → Email

```
⚡ RestablecimientoSolicitado (evento de Auth)
    🟣 POLÍTICA: Enviar correo con token de reset
    🔵 EnviarCorreo {
        para: usuario.email,
        asunto: "Restablece tu contraseña",
        html: template con token y link
    }
    🟠 CorreoEnviado / CorreoFallido
    🟣 POLÍTICA: Si falla → log warning, NO revertir el token (ya está guardado en BD)
```

```
⚡ UsuarioBloqueado (evento de Auth)
    🟣 POLÍTICA: Notificar al usuario (opcional, configurable)
    🔵 EnviarCorreo { asunto: "Tu cuenta ha sido bloqueada temporalmente" }
```

### Nómina → Email

```
⚡ ComprobanteGenerado (evento de Nómina)
    🟣 POLÍTICA: Enviar comprobante al empleado
    🔵 EnviarCorreoConAdjunto {
        para: empleado.email,
        asunto: "Tu comprobante de pago - Enero 2025",
        adjuntos: [{ nombre: "comprobante.pdf", contenido: pdfBytes }]
    }
    🟠 CorreoConAdjuntoEnviado / CorreoFallido
```

### RRHH → Email

```
⚡ CandidatoSeleccionado (evento de RRHH)
    🔵 EnviarCorreo { asunto: "Felicitaciones, fuiste seleccionado" }

⚡ EntrevistaAgendada (evento de RRHH)
    🔵 EnviarCorreo { asunto: "Confirmación de entrevista" }
```

### Contabilidad → Email

```
⚡ FacturaEmitida (evento de Contabilidad)
    🔵 EnviarCorreoConAdjunto {
        asunto: "Factura #001-2025",
        adjuntos: [{ nombre: "factura-001.pdf" }]
    }
```

---

## Politicas Automaticas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Log de todo envío | `CorreoEnviado` o `CorreoFallido` | Registrar en log estructurado |
| P2 | No propagar excepciones | Cualquier error del proveedor | Encapsular en `EmailResult` |
| P3 | Fail-fast en configuración | Proveedor inválido al iniciar | No iniciar la aplicación |
| P4 | Correlación de logs | Cualquier envío | Incluir `X-Correlation-Id` en log |

---

## Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Qué pasa si el correo de reset falla? | El token ya está en BD — el usuario puede reintentar. Log warning. |
| H2 | ¿Se reintentan los envíos fallidos? | No en v1. Preparado para agregar Polly/queue en el futuro. |
| H3 | ¿Se guardan los correos enviados en BD? | No en v1. Solo log. Si se necesita historial → módulo Notificaciones futuro. |
| H4 | ¿Cómo manejar rate limits de Mailjet? | Retorna error en `EmailResult`. El consumidor decide reintentar. |
| H5 | ¿Templates HTML centralizados? | No en v1. Cada módulo construye su HTML. Templates en v2. |

---

**Estado:** Documentado
**Fecha:** 2026-04-22
