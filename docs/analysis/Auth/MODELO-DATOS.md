# Auth — Modelo de Datos

---

## Entidades

### Usuario
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| TenantId | Guid? | FK — null si single-tenant |
| Nombre | string(100) | Nombre completo |
| Email | string(200) | Email único (por tenant) |
| PasswordHash | string(500) | Hash BCrypt |
| Estado | EstadoUsuario | Activo, Inactivo, Pendiente, Bloqueado |
| IntentosFallidos | int | Contador de intentos fallidos |
| BloqueadoHasta | DateTime? | Fecha de desbloqueo automático |
| TwoFactorActivo | bool | Si tiene 2FA configurado |
| TwoFactorSecretKey | string? | Clave secreta TOTP (encriptada) |
| UltimoAcceso | DateTime? | Fecha del último login exitoso |
| CreadoEn | DateTime | Fecha de creación |
| ModificadoEn | DateTime? | Fecha de última modificación |

### Rol
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| TenantId | Guid? | FK — null si single-tenant |
| Nombre | string(100) | Nombre único del rol |
| Descripcion | string(500) | Descripción del rol |
| EsSistema | bool | true = no puede eliminarse (SuperAdmin, Admin, User) |
| CreadoEn | DateTime | Fecha de creación |

### Permiso
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| Codigo | string(200) | Formato: `Modulo.Recurso.Accion` |
| Nombre | string(200) | Nombre legible |
| Descripcion | string(500) | Descripción del permiso |
| Modulo | string(100) | Módulo al que pertenece |

### RolPermiso (tabla de unión)
| Campo | Tipo | Descripción |
|---|---|---|
| RolId | Guid | FK → Rol |
| PermisoId | Guid | FK → Permiso |

### UsuarioRol (tabla de unión)
| Campo | Tipo | Descripción |
|---|---|---|
| UsuarioId | Guid | FK → Usuario |
| RolId | Guid | FK → Rol |
| AsignadoEn | DateTime | Fecha de asignación |

### RefreshToken
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| UsuarioId | Guid | FK → Usuario |
| Token | string(500) | Token único (hash) |
| ExpiraEn | DateTime | Fecha de expiración |
| RevocarEn | DateTime? | Fecha de revocación |
| EsRevocado | bool | Si fue revocado |
| CreadoEn | DateTime | Fecha de creación |
| CreadoDesdeIp | string(50) | IP de origen |

### TokenRestablecimiento
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| UsuarioId | Guid | FK → Usuario |
| Token | string(500) | Token único (hash) |
| ExpiraEn | DateTime | Fecha de expiración |
| UsadoEn | DateTime? | Fecha de uso |
| EsUsado | bool | Si ya fue usado |
| CreadoEn | DateTime | Fecha de creación |

### CodigoRecuperacion2FA
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| UsuarioId | Guid | FK → Usuario |
| CodigoHash | string(500) | Hash del código |
| UsadoEn | DateTime? | Fecha de uso |
| EsUsado | bool | Si ya fue usado |
| CreadoEn | DateTime | Fecha de creación |

### RegistroAuditoria
| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | PK |
| TenantId | Guid? | FK — null si single-tenant |
| UsuarioId | Guid? | FK → Usuario (null si login fallido de email inexistente) |
| Email | string(200) | Email usado en el intento |
| Evento | EventoAuditoria | Login, LoginFallido, Logout, CambioPassword, etc. |
| Ip | string(50) | IP de origen |
| UserAgent | string(500) | User agent del cliente |
| Detalle | string? | Información adicional |
| CreadoEn | DateTime | Fecha del evento |

---

## Enums

### EstadoUsuario
```
Pendiente  = 1  → Registrado, pendiente de activación
Activo     = 2  → Puede autenticarse
Inactivo   = 3  → Desactivado por administrador
Bloqueado  = 4  → Bloqueado por intentos fallidos
```

### EventoAuditoria
```
Login               = 1
LoginFallido        = 2
Logout              = 3
CambioPassword      = 4
RestablecimientoSolicitado = 5
RestablecimientoCompletado = 6
CuentaBloqueada     = 7
CuentaDesbloqueada  = 8
2FAActivado         = 9
2FADesactivado      = 10
TokenRefrescado     = 11
```

---

## Relaciones

```
Usuario ──< UsuarioRol >── Rol ──< RolPermiso >── Permiso
Usuario ──< RefreshToken
Usuario ──< TokenRestablecimiento
Usuario ──< CodigoRecuperacion2FA
Usuario ──< RegistroAuditoria
```

---

## Índices importantes

| Tabla | Columnas | Tipo |
|---|---|---|
| Usuario | (TenantId, Email) | Unique |
| Rol | (TenantId, Nombre) | Unique |
| Permiso | Codigo | Unique |
| RefreshToken | Token | Unique |
| TokenRestablecimiento | Token | Unique |

---

## Diagrama de estados — Usuario

```
[Pendiente] ──activar──→ [Activo]
[Activo]    ──desactivar──→ [Inactivo]
[Inactivo]  ──activar──→ [Activo]
[Activo]    ──bloquear (intentos)──→ [Bloqueado]
[Bloqueado] ──desbloquear (admin/tiempo)──→ [Activo]
```
