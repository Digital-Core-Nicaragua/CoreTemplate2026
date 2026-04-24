# Notificaciones en Tiempo Real — Documentación

> **Tipo:** Building Block (`CoreTemplate.Notifications`) + Módulo (`CoreTemplate.Modules.Notificaciones`)
> **Tecnología:** SignalR (incluido en ASP.NET Core, sin paquetes extra)
> **Fecha:** 2026-04-22
> **Estado:** Pendiente de implementación

---

## ¿Por qué notificaciones en tiempo real?

Hoy el sistema solo notifica por correo. Pero en un ERP los usuarios necesitan
saber qué pasa **mientras están usando el sistema**, sin refrescar la página:

```
🔔 "Tu comprobante de pago de enero está listo para descargar"
🔔 "Tu solicitud de vacaciones fue aprobada"
🔔 "Nuevo documento pendiente de tu firma"
🔔 "El proceso de nómina de enero finalizó"
🔔 "Tu sesión fue iniciada desde otro dispositivo"  ← seguridad
```

---

## Arquitectura propuesta

```
CoreTemplate.Notifications (Building Block)
  Abstractions/
    INotificationSender.cs      → contrato para enviar notificaciones
    NotificationMessage.cs      → modelo de notificación
    NotificationResult.cs       → resultado del envío
  Hubs/
    NotificationHub.cs          → SignalR Hub — punto de conexión WebSocket
  Services/
    SignalRNotificationSender.cs → implementación de INotificationSender

Módulo Notificaciones
  Domain/
    Notificacion (aggregate)    → historial de notificaciones en BD
  Application/
    Commands/                   → MarcarComoLeida, MarcarTodasComoLeidas
    Queries/                    → GetMisNotificaciones (paginado)
  Infrastructure/
    NotificacionesDbContext     → schema: Notificaciones
  Api/
    NotificacionesController    → GET /api/notificaciones
    NotificacionesHub           → /hubs/notificaciones (WebSocket)
```

---

## Requerimientos Funcionales

### RF-NOT-001: Enviar notificación a un usuario específico
**Prioridad:** Crítica

- Contrato: `INotificationSender.EnviarAsync(usuarioId, NotificationMessage)`
- Si el usuario está conectado → recibe la notificación en tiempo real (SignalR)
- Si no está conectado → se guarda en BD para mostrar al reconectarse
- Retorna `NotificationResult { Exitoso, EntregadaEnTiempoReal }`

### RF-NOT-002: Enviar notificación a todos los usuarios de un tenant
**Prioridad:** Alta

- Contrato: `INotificationSender.EnviarATenantAsync(tenantId, NotificationMessage)`
- Útil para: mantenimiento programado, actualizaciones del sistema, avisos generales

### RF-NOT-003: Historial de notificaciones
**Prioridad:** Alta

- `GET /api/notificaciones` → lista las notificaciones del usuario autenticado (paginado)
- Filtro por leídas/no leídas
- Retorna conteo de no leídas para el badge del ícono 🔔

### RF-NOT-004: Marcar como leída
**Prioridad:** Alta

- `PUT /api/notificaciones/{id}/leer` → marca una notificación como leída
- `PUT /api/notificaciones/leer-todas` → marca todas como leídas

### RF-NOT-005: Tipos de notificación
**Prioridad:** Media

Cada notificación tiene un tipo que el frontend usa para mostrar el ícono correcto:

| Tipo | Ícono | Ejemplo |
|---|---|---|
| `Info` | ℹ️ | "El proceso finalizó correctamente" |
| `Exito` | ✅ | "Tu comprobante está listo" |
| `Advertencia` | ⚠️ | "Tu sesión expira en 5 minutos" |
| `Error` | ❌ | "El proceso de nómina falló" |
| `Seguridad` | 🔒 | "Nueva sesión iniciada desde otro dispositivo" |

### RF-NOT-006: Multi-tenant
**Prioridad:** Alta

- `Notificacion` implementa `IHasTenant`
- Un usuario solo ve sus propias notificaciones
- Las notificaciones de tenant A no son visibles para tenant B

### RF-NOT-007: Integración con eventos de dominio
**Prioridad:** Alta

Los módulos publican eventos → los handlers de Notificaciones los convierten en notificaciones:

| Evento | Notificación generada |
|---|---|
| `SesionCreadaEvent` (nuevo dispositivo) | 🔒 "Nueva sesión iniciada desde {dispositivo}" |
| `UsuarioBloqueadoEvent` | ⚠️ "Tu cuenta fue bloqueada temporalmente" |
| `PasswordCambiadoEvent` | 🔒 "Tu contraseña fue cambiada" |
| Eventos de Nómina (futuro) | ✅ "Tu comprobante de enero está listo" |
| Eventos de RRHH (futuro) | ✅ "Tu solicitud fue aprobada" |

---

## Requerimientos No Funcionales

### RNF-NOT-001: Sin paquetes extra
SignalR está incluido en ASP.NET Core. No se requieren paquetes NuGet adicionales.

### RNF-NOT-002: Fallback a BD
Si el usuario no está conectado, la notificación se guarda en BD y se entrega al reconectarse.
No se pierden notificaciones.

### RNF-NOT-003: Escalabilidad horizontal
Para múltiples instancias del servidor → usar SignalR con Redis Backplane.
Configurable: `"NotificationSettings": { "UseRedisBackplane": false }`.
En desarrollo y servidor único → sin Redis.

### RNF-NOT-004: Autenticación del Hub
La conexión al Hub de SignalR requiere JWT válido.
El token se pasa como query string: `?access_token={jwt}` (requerido por SignalR).

---

## Modelo de datos

### Aggregate: Notificacion

```
Notificacion (AggregateRoot, IHasTenant)
  + Id              : Guid
  + TenantId        : Guid?
  + UsuarioId       : Guid
  + Titulo          : string        "Tu comprobante está listo"
  + Mensaje         : string        "El comprobante de enero 2025 está disponible"
  + Tipo            : TipoNotificacion  (Info, Exito, Advertencia, Error, Seguridad)
  + Url             : string?       "/nomina/comprobantes/2025/01"  (link opcional)
  + EsLeida         : bool
  + EntregadaEnTiempoReal : bool
  + CreadaEn        : DateTime
  + LeidaEn         : DateTime?
```

### Tabla: Notificaciones.Notificaciones

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | Multi-tenant |
| UsuarioId | uniqueidentifier | FK lógica al usuario |
| Titulo | nvarchar(200) | Título corto |
| Mensaje | nvarchar(1000) | Descripción completa |
| Tipo | nvarchar(20) | Info, Exito, Advertencia, Error, Seguridad |
| Url | nvarchar(500)? | Link de acción opcional |
| EsLeida | bit | Default: false |
| EntregadaEnTiempoReal | bit | Si llegó via SignalR |
| CreadaEn | datetime2 | |
| LeidaEn | datetime2? | |

**Índices:**
- `(UsuarioId, EsLeida)` — para consultar no leídas rápido
- `(TenantId, UsuarioId)` — para aislamiento multi-tenant

---

## Contratos del Building Block

```csharp
public record NotificationMessage(
    Guid UsuarioId,
    string Titulo,
    string Mensaje,
    TipoNotificacion Tipo = TipoNotificacion.Info,
    string? Url = null,
    Guid? TenantId = null);

public record NotificationResult(
    bool Exitoso,
    bool EntregadaEnTiempoReal);

public interface INotificationSender
{
    Task<NotificationResult> EnviarAsync(
        NotificationMessage mensaje, CancellationToken ct = default);

    Task EnviarATenantAsync(
        Guid tenantId, string titulo, string mensaje,
        TipoNotificacion tipo = TipoNotificacion.Info,
        CancellationToken ct = default);
}
```

---

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/notificaciones` | Listar mis notificaciones (paginado, filtro leídas) |
| GET | `/api/notificaciones/no-leidas/count` | Conteo de no leídas (para el badge 🔔) |
| PUT | `/api/notificaciones/{id}/leer` | Marcar una como leída |
| PUT | `/api/notificaciones/leer-todas` | Marcar todas como leídas |
| WS | `/hubs/notificaciones` | WebSocket SignalR (requiere JWT) |

---

## Cómo lo usa un módulo

```csharp
// En cualquier handler — inyectar INotificationSender
public class AprobacionSolicitudHandler(INotificationSender notifications)
{
    public async Task Handle(AprobarSolicitudCommand cmd, CancellationToken ct)
    {
        // ... lógica de negocio ...

        await notifications.EnviarAsync(new NotificationMessage(
            UsuarioId: solicitud.UsuarioId,
            Titulo: "Solicitud aprobada",
            Mensaje: $"Tu solicitud de {solicitud.Tipo} fue aprobada",
            Tipo: TipoNotificacion.Exito,
            Url: $"/solicitudes/{solicitud.Id}"
        ), ct);
    }
}
```

---

## Cómo se conecta el frontend

```javascript
// JavaScript / TypeScript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notificaciones", {
        accessTokenFactory: () => localStorage.getItem("accessToken")
    })
    .withAutomaticReconnect()
    .build();

// Escuchar notificaciones
connection.on("RecibirNotificacion", (notificacion) => {
    mostrarBadge(notificacion);
    mostrarToast(notificacion.titulo, notificacion.mensaje);
});

await connection.start();
```

---

## Plan de implementación

### Fase 1 — Building Block (Día 1)
```
□ Crear CoreTemplate.Notifications/
□ Contratos: INotificationSender, NotificationMessage, NotificationResult
□ NotificationHub.cs (SignalR Hub con autenticación JWT)
□ SignalRNotificationSender.cs
□ DependencyInjection.cs
□ Registrar en Program.cs: app.MapHub<NotificationHub>("/hubs/notificaciones")
```

### Fase 2 — Módulo Notificaciones (Día 1-2)
```
□ Aggregate Notificacion con IHasTenant
□ NotificacionesDbContext (schema: Notificaciones)
□ Migración
□ Commands: MarcarComoLeida, MarcarTodasComoLeidas
□ Queries: GetMisNotificaciones, GetConteoNoLeidas
□ NotificacionesController
□ Seeder de permisos: Notificaciones.Ver
```

### Fase 3 — Handlers de eventos de Auth (Día 2)
```
□ SesionCreadaHandler → notificación de seguridad
□ UsuarioBloqueadoHandler → notificación de advertencia
□ PasswordCambiadoHandler → notificación de seguridad
```

### Fase 4 — Escalabilidad con Redis (Opcional, Día 3)
```
□ Agregar SignalR Redis Backplane (solo si hay múltiples instancias)
□ Configurar: "NotificationSettings": { "UseRedisBackplane": true }
□ Reutilizar la conexión Redis existente del TokenBlacklist
```

---

## Consideraciones de seguridad

| Riesgo | Mitigación |
|---|---|
| Usuario recibe notificaciones de otro usuario | El Hub verifica el JWT y solo envía al grupo del usuario autenticado |
| Tenant A ve notificaciones de Tenant B | IHasTenant + QueryFilter automático |
| Token expirado en conexión WebSocket | SignalR reconecta automáticamente con nuevo token |

---

**Estado:** Documentado — pendiente de implementación
**Fecha:** 2026-04-22
