# Event Storming — Big Picture
# Building Block: CoreTemplate.Notifications + Módulo Notificaciones

> **Nivel:** Big Picture + Process Level
> **Fecha:** 2026-04-22

---

## Leyenda

| Símbolo | Color | Elemento |
|---|---|---|
| 🟠 | Naranja | Evento de dominio |
| 🔵 | Azul | Comando |
| 🟡 | Amarillo | Aggregate |
| 🟣 | Morado | Política |
| 🟢 | Verde | Read Model |
| 🔴 | Rojo | Hotspot |
| ⚡ | — | Evento externo |
| 👤 | — | Actor humano |
| 🤖 | — | Sistema automático |

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| 👤 **Usuario** | Humano | Recibe y gestiona sus notificaciones |
| 🤖 **Módulo Auth** | Sistema | Publica eventos de seguridad |
| 🤖 **Módulo Nómina** | Sistema | Publica eventos de comprobantes |
| 🤖 **Módulo RRHH** | Sistema | Publica eventos de solicitudes |
| 🌐 **SignalR Hub** | Infraestructura | Canal WebSocket de entrega en tiempo real |

---

## Flujo: Usuario se conecta al Hub

```
👤 Usuario → Conectar a /hubs/notificaciones?access_token={jwt}
    SignalR Hub → Validar JWT
    
    2a. JWT inválido o expirado:
        → Rechazar conexión (401)
    
    2b. JWT válido:
        → Agregar usuario al grupo "user-{usuarioId}"
        → Agregar usuario al grupo "tenant-{tenantId}"
        🟠 UsuarioConectado { usuarioId, connectionId }
        
        🟣 POLÍTICA: Entregar notificaciones no leídas pendientes
        🟢 ConsultarNotificacionesPendientes { usuarioId }
        → Enviar notificaciones acumuladas mientras estaba desconectado
```

---

## Flujo: Enviar notificación a un usuario

```
🤖 Módulo Consumidor → 🔵 EnviarNotificacion {
    usuarioId, titulo, mensaje, tipo, url?
}

INotificationSender:
    1. Guardar en BD (siempre)
    🟡 Notificacion → Crear { usuarioId, titulo, mensaje, tipo, url, EsLeida: false }
    🟠 NotificacionCreada { notificacionId, usuarioId }

    2. ¿Usuario conectado al Hub?
    3a. SÍ → Enviar via SignalR al grupo "user-{usuarioId}"
        🟠 NotificacionEntregadaEnTiempoReal { notificacionId }
        → Retornar NotificationResult { Exitoso: true, EntregadaEnTiempoReal: true }

    3b. NO → Queda en BD para entregar al reconectarse
        → Retornar NotificationResult { Exitoso: true, EntregadaEnTiempoReal: false }
```

---

## Flujo: Enviar notificación a todo un tenant

```
🤖 Sistema → 🔵 EnviarNotificacionATenant {
    tenantId, titulo, mensaje, tipo
}

INotificationSender:
    → Enviar via SignalR al grupo "tenant-{tenantId}"
    → Guardar en BD para cada usuario del tenant (o solo los conectados)
    🟠 NotificacionTenantEnviada { tenantId, titulo }
```

---

## Flujo: Usuario marca notificación como leída

```
👤 Usuario → 🔵 MarcarComoLeida { notificacionId }
    🟡 Notificacion → Verificar que pertenece al usuario
    🟡 Notificacion → Marcar { EsLeida: true, LeidaEn: utcNow }
    🟠 NotificacionLeida { notificacionId, usuarioId }
    → Retornar 200
```

---

## Flujo: Usuario marca todas como leídas

```
👤 Usuario → 🔵 MarcarTodasComoLeidas
    🟡 Notificacion (todas del usuario) → Marcar como leídas
    🟠 TodasNotificacionesLeidas { usuarioId, cantidad }
    → Retornar 200
```

---

## Integración con eventos de Auth

```
⚡ SesionCreadaEvent { usuarioId, dispositivo, ip, canal }
    🟣 POLÍTICA: Si es sesión desde nuevo dispositivo → notificar
    🔵 EnviarNotificacion {
        usuarioId,
        titulo: "Nueva sesión iniciada",
        mensaje: "Sesión iniciada desde {dispositivo} ({ip})",
        tipo: Seguridad,
        url: "/perfil/sesiones"
    }
    🟠 NotificacionCreada

⚡ UsuarioBloqueadoEvent { usuarioId, bloqueadoHasta }
    🔵 EnviarNotificacion {
        titulo: "Cuenta bloqueada temporalmente",
        tipo: Advertencia
    }

⚡ PasswordCambiadoEvent { usuarioId }
    🔵 EnviarNotificacion {
        titulo: "Tu contraseña fue cambiada",
        tipo: Seguridad,
        url: "/perfil/sesiones"
    }
```

---

## Integración con módulos de negocio (futuros)

```
⚡ ComprobanteGeneradoEvent (Nómina)
    🔵 EnviarNotificacion {
        titulo: "Tu comprobante está listo",
        tipo: Exito,
        url: "/nomina/comprobantes/{id}"
    }

⚡ SolicitudAprobadaEvent (RRHH)
    🔵 EnviarNotificacion {
        titulo: "Tu solicitud fue aprobada",
        tipo: Exito
    }

⚡ ProcesoNominaFinalizadoEvent (Nómina)
    🔵 EnviarNotificacionATenant {
        titulo: "Proceso de nómina finalizado",
        tipo: Info
    }
```

---

## Políticas Automáticas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Siempre guardar en BD | Cualquier notificación | Persistir antes de intentar SignalR |
| P2 | Entregar pendientes al conectar | UsuarioConectado | Enviar notificaciones no leídas acumuladas |
| P3 | Aislamiento por tenant | Cualquier consulta | IHasTenant + QueryFilter automático |
| P4 | Autenticación del Hub | Conexión WebSocket | JWT obligatorio como query string |

---

## Eventos de Dominio del Módulo

| Evento | Trigger | Datos |
|---|---|---|
| `NotificacionCreada` | Nueva notificación | notificacionId, usuarioId, tipo |
| `NotificacionLeida` | Marcar como leída | notificacionId, usuarioId, leidaEn |
| `TodasNotificacionesLeidas` | Marcar todas | usuarioId, cantidad |
| `NotificacionEntregadaEnTiempoReal` | Entrega via SignalR | notificacionId |

---

## Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Múltiples instancias del servidor? | Redis Backplane para SignalR. Configurable: `UseRedisBackplane: true`. Reutiliza la conexión Redis del TokenBlacklist. |
| H2 | ¿Cuánto tiempo guardar notificaciones? | Política de retención configurable. Default: 90 días. Limpiar con job programado. |
| H3 | ¿Notificaciones para usuarios desconectados? | Se guardan en BD. Se entregan al reconectarse (P2). |
| H4 | ¿El token JWT expira durante la conexión WebSocket? | SignalR reconecta automáticamente con `withAutomaticReconnect()`. El cliente debe renovar el token. |
| H5 | ¿Límite de notificaciones no leídas? | No en v1. En v2: límite configurable, eliminar las más antiguas. |

---

**Estado:** Documentado
**Fecha:** 2026-04-22
