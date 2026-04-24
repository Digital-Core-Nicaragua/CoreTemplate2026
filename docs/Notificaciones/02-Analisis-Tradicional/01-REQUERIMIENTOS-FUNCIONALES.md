# Notificaciones — Requerimientos Funcionales

> **Fecha:** 2026-04-22
> **Total:** 10 RF

---

## Contexto

`CoreTemplate.Notifications` es un building block transversal.
`CoreTemplate.Modules.Notificaciones` gestiona el historial en BD y expone la API.

Cualquier módulo inyecta `INotificationSender` para enviar notificaciones
sin conocer el mecanismo de entrega (SignalR + BD).

---

## RF-NOT-001: Enviar notificación a usuario específico
**Prioridad:** Crítica

### Criterios de Aceptación
- Contrato: `INotificationSender.EnviarAsync(NotificationMessage, ct)`
- Siempre guarda en BD antes de intentar entrega en tiempo real
- Si el usuario está conectado → entrega via SignalR inmediatamente
- Si no está conectado → queda en BD, se entrega al reconectarse
- Retorna `NotificationResult { Exitoso, EntregadaEnTiempoReal }`
- No lanza excepciones al consumidor — encapsula errores en el resultado

---

## RF-NOT-002: Enviar notificación a todo un tenant
**Prioridad:** Alta

### Criterios de Aceptación
- Contrato: `INotificationSender.EnviarATenantAsync(tenantId, titulo, mensaje, tipo, ct)`
- Envía via SignalR al grupo del tenant
- Útil para: mantenimiento, actualizaciones, avisos generales

---

## RF-NOT-003: Historial de notificaciones del usuario
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/notificaciones` — lista las notificaciones del usuario autenticado
- Paginado (default: 20 por página)
- Filtro por `soloNoLeidas` (bool)
- Ordenado por `CreadaEn` descendente (más recientes primero)
- Solo retorna notificaciones del usuario autenticado (nunca de otros)

---

## RF-NOT-004: Conteo de notificaciones no leídas
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/notificaciones/no-leidas/count`
- Retorna `{ count: 5 }` — para el badge 🔔 en la UI
- Respuesta rápida — usa índice `(UsuarioId, EsLeida)`

---

## RF-NOT-005: Marcar notificación como leída
**Prioridad:** Alta

### Criterios de Aceptación
- `PUT /api/notificaciones/{id}/leer`
- Solo el dueño puede marcar su propia notificación
- Si no existe o no pertenece al usuario → 404
- Registra `LeidaEn = utcNow`

---

## RF-NOT-006: Marcar todas las notificaciones como leídas
**Prioridad:** Alta

### Criterios de Aceptación
- `PUT /api/notificaciones/leer-todas`
- Marca todas las notificaciones no leídas del usuario autenticado
- Retorna la cantidad de notificaciones marcadas

---

## RF-NOT-007: Tipos de notificación
**Prioridad:** Media

### Tipos disponibles:

| Tipo | Uso | Ícono sugerido |
|---|---|---|
| `Info` | Información general | ℹ️ |
| `Exito` | Operación completada | ✅ |
| `Advertencia` | Atención requerida | ⚠️ |
| `Error` | Algo falló | ❌ |
| `Seguridad` | Actividad de seguridad | 🔒 |

---

## RF-NOT-008: Conexión WebSocket autenticada
**Prioridad:** Crítica

### Criterios de Aceptación
- Endpoint WebSocket: `/hubs/notificaciones`
- Requiere JWT válido como query string: `?access_token={jwt}`
- Si el JWT es inválido o expirado → rechazar conexión (401)
- Al conectar → entregar notificaciones no leídas acumuladas
- Al desconectar → el historial en BD se mantiene intacto

---

## RF-NOT-009: Multi-tenant
**Prioridad:** Crítica

### Criterios de Aceptación
- `Notificacion` implementa `IHasTenant`
- El QueryFilter de `BaseDbContext` aplica automáticamente
- Un usuario solo ve sus propias notificaciones
- Las notificaciones de Tenant A no son visibles para Tenant B
- El Hub agrupa conexiones por tenant: `"tenant-{tenantId}"`

---

## RF-NOT-010: Integración con eventos de Auth
**Prioridad:** Alta

### Eventos de Auth que generan notificaciones automáticas:

| Evento | Notificación | Tipo |
|---|---|---|
| `SesionCreadaEvent` (nuevo dispositivo) | "Nueva sesión iniciada desde {dispositivo}" | Seguridad |
| `UsuarioBloqueadoEvent` | "Tu cuenta fue bloqueada temporalmente" | Advertencia |
| `PasswordCambiadoEvent` | "Tu contraseña fue cambiada" | Seguridad |

### Criterios de Aceptación
- Los handlers son configurables: `"NotificationSettings:Handlers:SesionCreada": true`
- Si el envío falla → log warning, NO revertir la operación de Auth

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 3 |
| Alta | 6 |
| Media | 1 |
| **Total** | **10** |

---

**Fecha:** 2026-04-22
