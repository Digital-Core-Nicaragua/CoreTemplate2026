# Eventos de Dominio — Módulo Auth

> **Total publicados:** 20  
> **Total consumidos:** 0 (CoreTemplate es plantilla base, sin integraciones entre módulos)  
> **Fecha:** 2026-04-15

---

## Eventos Publicados

### Aggregate: Usuario (11 eventos)

| Evento | Datos | Cuándo se dispara |
|---|---|---|
| `UsuarioRegistradoEvent` | usuarioId, email, nombre, tenantId | Al crear un nuevo usuario |
| `UsuarioActivadoEvent` | usuarioId, email | Al activar un usuario |
| `UsuarioDesactivadoEvent` | usuarioId, email | Al desactivar un usuario |
| `UsuarioBloqueadoEvent` | usuarioId, email, bloqueadoHasta | Al bloquear (manual o automático) |
| `UsuarioDesbloqueadoEvent` | usuarioId, email | Al desbloquear un usuario |
| `PasswordCambiadoEvent` | usuarioId, email | Al cambiar contraseña |
| `RestablecimientoSolicitadoEvent` | usuarioId, email, token, expiraEn | Al solicitar restablecimiento |
| `DosFactoresActivadoEvent` | usuarioId, email | Al activar 2FA definitivamente |
| `DosFactoresDesactivadoEvent` | usuarioId, email | Al desactivar 2FA |
| `SucursalAsignadaEvent` | usuarioId, sucursalId | Al asignar sucursal a usuario |
| `SucursalRemovidaEvent` | usuarioId, sucursalId | Al remover sucursal de usuario |

### Aggregate: Sesion (2 eventos)

| Evento | Datos | Cuándo se dispara |
|---|---|---|
| `SesionRevocadaEvent` | sesionId, usuarioId | Al revocar una sesión específica |
| `TodasSesionesRevocadasEvent` | usuarioId | Al revocar todas las sesiones |

### Aggregate: Rol (4 eventos)

| Evento | Datos | Cuándo se dispara |
|---|---|---|
| `RolCreadoEvent` | rolId, nombre, tenantId | Al crear un rol |
| `RolActualizadoEvent` | rolId, nombre | Al actualizar un rol |
| `PermisoAgregadoARolEvent` | rolId, permisoId | Al agregar permiso a rol |
| `PermisoQuitadoDeRolEvent` | rolId, permisoId | Al quitar permiso de rol |

### Aggregates: Sucursal y AsignacionRol (3 eventos)

| Evento | Datos | Cuándo se dispara |
|---|---|---|
| `SucursalAsignadaEvent` | usuarioId, sucursalId | Al asignar sucursal |
| `SucursalRemovidaEvent` | usuarioId, sucursalId | Al remover sucursal |
| `TodasSesionesRevocadasEvent` | usuarioId | Al revocar todas las sesiones |

---

## Eventos Consumidos

CoreTemplate es una plantilla base sin integraciones entre módulos. Los eventos de dominio se disparan pero no tienen handlers registrados por defecto.

**El sistema implementador debe registrar handlers para:**

| Evento | Acción sugerida |
|---|---|
| `RestablecimientoSolicitadoEvent` | Enviar email con el token de restablecimiento |
| `UsuarioBloqueadoEvent` | Notificar al usuario por email |
| `PasswordCambiadoEvent` | Notificar al usuario por email |
| `DosFactoresActivadoEvent` | Notificar al usuario por email |

---

## Implementación

Los eventos se implementan como `record` que implementan `IDomainEvent`:

```csharp
// SharedKernel
public interface IDomainEvent { }

// Ejemplo de evento
public record UsuarioRegistradoEvent(
    Guid UsuarioId,
    string Email,
    string Nombre,
    Guid? TenantId) : IDomainEvent;
```

Los aggregates acumulan eventos en `_domainEvents` y los exponen via `DomainEvents`. El dispatch se hace en los handlers después de persistir.

---

**Fecha:** 2026-04-15
