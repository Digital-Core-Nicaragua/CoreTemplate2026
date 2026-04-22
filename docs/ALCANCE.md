# CoreTemplate — Documento Maestro de Alcance

> **Versión**: 2.0
> **Fecha**: 2026-04-15
> **Estado**: Completo

---

## 1. Propósito

CoreTemplate es una plantilla de proyecto reutilizable para sistemas **ASP.NET Core 10** con arquitectura Clean Architecture + DDD + CQRS.

El objetivo es que al iniciar un nuevo sistema, el desarrollador clone esta plantilla y tenga listo desde el primer día un sistema de autenticación y autorización **enterprise-grade**, completamente configurable, con soporte multi-tenant, gestión de sesiones, tipos de usuario, canales de acceso y seguridad avanzada.

---

## 2. Qué incluye la plantilla

### 2.1 Módulo Auth — Funcionalidades base (siempre activas)

#### Autenticación
- Registro de usuarios con política de contraseña configurable
- Login con JWT (AccessToken + RefreshToken con rotación)
- 2FA TOTP configurable (Google Authenticator, Authy)
- Bloqueo de cuenta por intentos fallidos (configurable)
- Cambio de contraseña
- Restablecimiento de contraseña por email
- Auditoría de todos los eventos de seguridad

#### Sesiones
- Sesiones como entidad gestionable (no solo RefreshToken)
- Cada sesión registra: dispositivo, IP, user agent, canal, última actividad
- Ver sesiones activas propias
- Cerrar una sesión específica remotamente
- Cerrar todas las sesiones excepto la actual
- Límite de sesiones simultáneas configurable (global)
- Al superar el límite: cierra la sesión más antigua automáticamente (configurable)
- Admin puede ver y cerrar sesiones de cualquier usuario

#### Tipos de usuario
- Enum configurable: `Humano`, `Sistema`, `Integracion` (extensible)
- Permite distinguir usuarios de personas vs APIs vs servicios internos
- Comportamiento diferenciado por tipo (ej: sistemas no tienen 2FA)

#### Canales de acceso
- Enum configurable: `Web`, `Mobile`, `Api`, `Desktop` (extensible)
- Cada sesión registra el canal desde donde se originó
- Los permisos pueden restringirse por canal (opcional)
- Útil para auditoría y análisis de acceso

#### Roles y Permisos
- Roles con múltiples permisos
- Permisos en formato `Modulo.Recurso.Accion`
- Roles iniciales: `SuperAdmin`, `Admin`, `User`
- Verificación con `[RequirePermission("...")]`

#### Token Blacklist
- Invalidación inmediata de tokens antes de su expiración
- Backend configurable: **Redis** (producción) o **InMemory** (desarrollo/sin Redis)
- Se activa automáticamente al hacer logout o revocar sesión
- Configurable: activar/desactivar con `AuthSettings:EnableTokenBlacklist`

---

### 2.2 Módulo Auth — Funcionalidades configurables

Estas features se activan/desactivan con flags en `appsettings.json`. Cuando están desactivadas, no generan tablas, no aparecen en Swagger y no afectan el comportamiento base.

#### Sucursales por usuario (`OrganizationSettings:EnableBranches`)
Útil para sistemas con estructura organizacional (ERP, retail, franquicias).

```json
{
  "OrganizationSettings": {
    "EnableBranches": false
  }
}
```

Cuando está **activado**:
- Los usuarios se asignan a una o más sucursales
- Cada usuario tiene una sucursal principal
- El token JWT incluye el claim `branch_id` de la sucursal activa
- El usuario puede cambiar su sucursal activa en la sesión
- Admin puede gestionar asignaciones de usuarios a sucursales

Cuando está **desactivado** (default):
- No existe el concepto de sucursal
- Los usuarios son globales dentro del tenant

#### Roles por sucursal (requiere `EnableBranches: true`)
Cuando sucursales está habilitado, los roles se asignan por combinación `usuario + sucursal`:
- Un usuario puede tener rol `Admin` en Sucursal A y rol `Operativo` en Sucursal B
- Los permisos efectivos dependen de la sucursal activa en la sesión
- Aggregate `AsignacionRol` gestiona estas combinaciones

#### Catálogo de Acciones (`AuthSettings:UseActionCatalog`)
Modelo de permisos avanzado para sistemas con permisos muy granulares.

```json
{
  "AuthSettings": {
    "UseActionCatalog": false
  }
}
```

Cuando está **desactivado** (default):
- Permisos como strings `Modulo.Recurso.Accion` — simple y suficiente para el 80% de sistemas

Cuando está **activado**:
- `Accion` es un aggregate con código, nombre, módulo y descripción
- Las acciones pueden habilitarse/deshabilitarse por sucursal
- Los roles referencian acciones del catálogo en lugar de strings
- Permite gestión centralizada del catálogo de permisos

#### Límites de sesiones por tenant (`TenantSettings:EnableSessionLimitsPerTenant`)
Cuando `IsMultiTenant = true`, cada tenant puede tener su propio límite de sesiones:

```json
{
  "TenantSettings": {
    "IsMultiTenant": true,
    "EnableSessionLimitsPerTenant": false
  }
}
```

Jerarquía de límites (de mayor a menor prioridad):
1. Configuración por tenant (si `EnableSessionLimitsPerTenant = true`)
2. Configuración global en `appsettings.json`
3. Default del sistema (5 sesiones)

---

### 2.3 Módulo Catálogos
Catálogo de ejemplo completamente implementado como patrón reutilizable:
- CRUD completo con paginación, filtros y búsqueda
- Activar / Desactivar
- Seed data de ejemplo

### 2.4 BuildingBlocks

| Proyecto | Contenido |
|---|---|
| `CoreTemplate.SharedKernel` | Result, PagedResult, AggregateRoot, Entity, ValueObject, IDomainEvent |
| `CoreTemplate.Api.Common` | ApiResponse, BaseApiController, GlobalExceptionHandler, ValidationBehavior |
| `CoreTemplate.Infrastructure` | BaseDbContext multi-tenant, ICurrentUser, ICurrentTenant, TenantMiddleware |

---

## 3. Qué NO incluye la plantilla

- Lógica de negocio específica de ningún sistema
- Módulos funcionales (ventas, inventario, facturación, etc.)
- Integraciones con servicios externos (pagos, email, SMS) — solo contratos/interfaces
- Frontend
- Portal de clientes (UsuarioCliente con OAuth) — demasiado específico
- Implementación de proveedor de WhatsApp/SMS (Twilio, AWS SNS, etc.) — cada sistema elige su proveedor

---

## 4. Multi-tenant

```json
{
  "TenantSettings": {
    "IsMultiTenant": true,
    "TenantResolutionStrategy": "Header",
    "EnableSessionLimitsPerTenant": false
  }
}
```

| `IsMultiTenant` | Comportamiento |
|---|---|
| `false` | Single-tenant — TenantId ignorado en todo |
| `true` | Filtrado automático por TenantId, middleware activo |

---

## 5. Base de datos soportadas

| Motor | Soporte |
|---|---|
| SQL Server | ✅ Completo |
| PostgreSQL | ✅ Completo |

---

## 6. Configuración completa de Auth

```json
{
  "AuthSettings": {
    "JwtSecretKey": "CAMBIAR-EN-PRODUCCION",
    "JwtIssuer": "CoreTemplate",
    "JwtAudience": "CoreTemplate",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "TwoFactorEnabled": false,
    "TwoFactorRequired": false,
    "PasswordResetTokenExpirationHours": 1,
    "MaxSesionesSimultaneas": 5,
    "AccionAlLlegarLimiteSesiones": "CerrarMasAntigua",
    "EnableTokenBlacklist": true,
    "UseActionCatalog": false
  },
  "LockoutSettings": {
    "MaxFailedAttempts": 5,
    "LockoutDurationMinutes": 15,
    "AutoUnlock": true
  },
  "PasswordPolicy": {
    "MinLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigit": true,
    "RequireSpecialChar": false
  },
  "TokenBlacklistSettings": {
    "Provider": "InMemory",
    "RedisConnectionString": ""
  },
  "OrganizationSettings": {
    "EnableBranches": false
  },
  "TenantSettings": {
    "IsMultiTenant": false,
    "TenantResolutionStrategy": "Header",
    "EnableSessionLimitsPerTenant": false
  }
}
```

### AccionAlLlegarLimiteSesiones
| Valor | Comportamiento |
|---|---|
| `CerrarMasAntigua` | Cierra la sesión más antigua automáticamente |
| `BloquearNuevoLogin` | Rechaza el nuevo login con error descriptivo |

### TokenBlacklistSettings.Provider
| Valor | Cuándo usar |
|---|---|
| `InMemory` | Desarrollo o sistemas con un solo servidor |
| `Redis` | Producción con múltiples instancias |

---

## 7. Tipos de usuario

Enum base incluido en la plantilla:

```csharp
public enum TipoUsuario
{
    Humano = 1,      // Persona real
    Sistema = 2,     // Servicio interno o proceso automatizado
    Integracion = 3  // API externa o tercero
}
```

Comportamiento diferenciado:
- `Sistema` e `Integracion`: no requieren 2FA, no tienen límite de sesiones
- `Humano`: aplican todas las reglas de seguridad

---

## 8. Canales de acceso

Enum base incluido en la plantilla:

```csharp
public enum CanalAcceso
{
    Web = 1,
    Mobile = 2,
    Api = 3,
    Desktop = 4
}
```

Cada sesión registra el canal. Los permisos pueden restringirse por canal cuando `UseActionCatalog = true`.

---

## 9. Roles y Permisos

### Modo simple (default, `UseActionCatalog = false`)
- Permisos como strings `Modulo.Recurso.Accion`
- Verificación con `[RequirePermission("Usuarios.Roles.Crear")]`
- Suficiente para la mayoría de sistemas

### Modo catálogo (`UseActionCatalog = true`)
- `Accion` es un aggregate gestionable
- Habilitación por sucursal (requiere `EnableBranches = true`)
- Permisos por canal de acceso
- Gestión centralizada desde admin

---

## 10. Sesiones

### Información por sesión
| Campo | Descripción |
|---|---|
| Id | Identificador único |
| UsuarioId | Usuario propietario |
| TenantId | Tenant (si multi-tenant) |
| RefreshToken | Token de renovación (hash) |
| Canal | Web, Mobile, Api, Desktop |
| Dispositivo | Nombre del dispositivo |
| Ip | IP de origen |
| UserAgent | Navegador/cliente |
| UltimaActividad | Fecha de último uso |
| ExpiraEn | Fecha de expiración |
| EsActiva | Estado |

### Endpoints de sesiones
```
GET    /api/perfil/sesiones              → mis sesiones activas
DELETE /api/perfil/sesiones/{id}         → cerrar sesión específica
DELETE /api/perfil/sesiones/otras        → cerrar todas excepto la actual
GET    /api/usuarios/{id}/sesiones       → admin: sesiones de un usuario
DELETE /api/usuarios/{id}/sesiones       → admin: cerrar todas las sesiones
```

---

## 11. Convenciones de código

- File-scoped namespaces
- Sealed classes en handlers y repositorios
- Primary constructors para inyección de dependencias
- `CancellationToken` en todos los métodos async
- Nombres en español para dominio y negocio
- Nombres en inglés para infraestructura técnica
- XML docs en clases y métodos públicos de BuildingBlocks

---

## 12. Plan de implementación

Ver `docs/PLAN-IMPLEMENTACION.md` para el estado detallado de cada fase.

| Fase | Descripción | Estado |
|---|---|---|
| 0-13 | Implementación base (Auth + Catálogos) | ✅ Completo |
| 14 | Sesiones como aggregate + límites | ✅ Completo |
| 15 | Tipos de usuario + Canales de acceso | ✅ Completo |
| 16 | Token Blacklist (Redis + InMemory) | ✅ Completo |
| 17 | Sucursales por usuario (configurable) | ✅ Completo |
| 18 | Roles por sucursal | ✅ Completo |
| 19 | Catálogo de Acciones (configurable) | ✅ Completo |
| 20 | Límites de sesiones por tenant | ✅ Completo |
| 21 | Tests de nuevas features | ✅ Completo |
| 22 | Actualizar README | ✅ Completo |
