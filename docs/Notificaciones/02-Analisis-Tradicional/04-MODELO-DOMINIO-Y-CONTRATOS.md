# Notificaciones — Modelo de Dominio y Contratos

> **Fecha:** 2026-04-22

---

## Aggregate: Notificacion

```
Notificacion (AggregateRoot, IHasTenant)
  + Id                      : Guid
  + TenantId                : Guid?
  + UsuarioId               : Guid
  + Titulo                  : string          "Tu comprobante está listo"
  + Mensaje                 : string          "El comprobante de enero 2025..."
  + Tipo                    : TipoNotificacion Info | Exito | Advertencia | Error | Seguridad
  + Url                     : string?         "/nomina/comprobantes/123" (link opcional)
  + EsLeida                 : bool            default: false
  + EntregadaEnTiempoReal   : bool            si llegó via SignalR
  + CreadaEn                : DateTime
  + LeidaEn                 : DateTime?

Métodos:
  + Crear(...)              : Result<Notificacion>
  + MarcarComoLeida()       : Result
```

---

## Contratos del Building Block

```csharp
public enum TipoNotificacion { Info, Exito, Advertencia, Error, Seguridad }

public record NotificationMessage(
    Guid UsuarioId,
    string Titulo,
    string Mensaje,
    TipoNotificacion Tipo = TipoNotificacion.Info,
    string? Url = null,
    Guid? TenantId = null);

public record NotificationResult(bool Exitoso, bool EntregadaEnTiempoReal);

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

## Modelo de datos

### Tabla: Notificaciones.Notificaciones

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | Multi-tenant |
| UsuarioId | uniqueidentifier | Dueño de la notificación |
| Titulo | nvarchar(200) | Título corto |
| Mensaje | nvarchar(1000) | Descripción completa |
| Tipo | nvarchar(20) | Info, Exito, Advertencia, Error, Seguridad |
| Url | nvarchar(500)? | Link de acción opcional |
| EsLeida | bit | Default: 0 |
| EntregadaEnTiempoReal | bit | Si llegó via SignalR |
| CreadaEn | datetime2 | |
| LeidaEn | datetime2? | |

**Índices:**
- `IX_Notificaciones_UsuarioId_EsLeida` → para conteo rápido de no leídas
- `IX_Notificaciones_TenantId_UsuarioId` → para aislamiento multi-tenant

---

## Estructura de proyectos

```
src/BuildingBlocks/CoreTemplate.Notifications/
  Abstractions/
    INotificationSender.cs
    NotificationMessage.cs
    NotificationResult.cs
    TipoNotificacion.cs
  Hubs/
    NotificationHub.cs          → SignalR Hub con autenticación JWT
  Services/
    SignalRNotificationSender.cs → implementa INotificationSender
  DependencyInjection.cs
  CoreTemplate.Notifications.csproj

src/Modules/Notificaciones/
  CoreTemplate.Modules.Notificaciones.Domain/
    Aggregates/
      Notificacion.cs
    Events/
      NotificacionEvents.cs
    Repositories/
      INotificacionRepository.cs

  CoreTemplate.Modules.Notificaciones.Application/
    Commands/
      MarcarComoLeidaCommand.cs
      MarcarTodasComoLeidasCommand.cs
    Queries/
      GetMisNotificacionesQuery.cs
      GetConteoNoLeidasQuery.cs
    EventHandlers/
      SesionCreadaNotificationHandler.cs
      UsuarioBloqueadoNotificationHandler.cs
      PasswordCambiadoNotificationHandler.cs
    DTOs/
      NotificacionDto.cs

  CoreTemplate.Modules.Notificaciones.Infrastructure/
    Persistence/
      NotificacionesDbContext.cs    schema: Notificaciones
      Configurations/
        NotificacionConfiguration.cs
    Repositories/
      NotificacionRepository.cs
    Services/
      NotificacionService.cs        implementa INotificationSender usando Hub + repo
    DependencyInjection.cs

  CoreTemplate.Modules.Notificaciones.Api/
    Controllers/
      NotificacionesController.cs
```

---

## Configuración en appsettings

```json
{
  "NotificationSettings": {
    "UseRedisBackplane": false,
    "RetenciónDías": 90,
    "Handlers": {
      "SesionCreada": true,
      "UsuarioBloqueado": true,
      "PasswordCambiado": true
    }
  }
}
```

---

## Endpoints

| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/api/notificaciones` | Listar mis notificaciones (paginado) | Sí |
| GET | `/api/notificaciones/no-leidas/count` | Conteo de no leídas | Sí |
| PUT | `/api/notificaciones/{id}/leer` | Marcar una como leída | Sí |
| PUT | `/api/notificaciones/leer-todas` | Marcar todas como leídas | Sí |
| WS | `/hubs/notificaciones` | WebSocket SignalR | JWT query string |

---

**Fecha:** 2026-04-22
