# Requerimientos Funcionales — Módulo Auth

> **Fecha:** 2026-04-15  
> **Total:** 22 RF

---

## Por Prioridad

- **Crítica:** 8 RF
- **Alta:** 10 RF
- **Media:** 4 RF

---

## RF-AUTH-001: Registro de Usuario ✅
**Prioridad:** Crítica  
**Aggregate:** Usuario

### Descripción
El sistema permite registrar nuevos usuarios con email, nombre y contraseña.

### Criterios de Aceptación
- Email único por tenant (o global si single-tenant)
- Contraseña validada contra política configurable
- Estado inicial: `Pendiente` hasta activación manual
- Tipo de usuario asignable al crear (`Humano`, `Sistema`, `Integracion`)
- Rol `User` asignado automáticamente al registrar

---

## RF-AUTH-002: Login con Credenciales ✅
**Prioridad:** Crítica  
**Aggregates:** Usuario, Sesion

### Descripción
El sistema autentica usuarios con email y contraseña, generando AccessToken + Sesion con RefreshToken.

### Criterios de Aceptación
- Genera AccessToken JWT (15 min) + RefreshToken (7 días)
- La sesión registra: canal, dispositivo, IP, user agent
- Verifica límite de sesiones simultáneas al crear nueva sesión
- Bloqueo automático por intentos fallidos (configurable)
- Mensaje genérico si credenciales inválidas (no revelar si email existe)
- Si 2FA activo → retorna `tokenTemporal` en lugar de tokens definitivos
- `Sistema` e `Integracion` no requieren 2FA
- `Sistema` e `Integracion` no tienen límite de sesiones

---

## RF-AUTH-003: Renovar Access Token ✅
**Prioridad:** Crítica  
**Aggregate:** Sesion

### Criterios de Aceptación
- Refresh token de un solo uso (rotación)
- Genera nuevo AccessToken + nuevo RefreshToken
- Actualiza `UltimaActividad` de la sesión
- Retorna error si token inválido, expirado o revocado

---

## RF-AUTH-004: Logout ✅
**Prioridad:** Crítica  
**Aggregate:** Sesion

### Criterios de Aceptación
- Revoca la sesión (marca como inactiva)
- Si `EnableTokenBlacklist = true`: agrega AccessToken a blacklist
- Auditoría del evento

---

## RF-AUTH-005: Cambio de Contraseña ✅
**Prioridad:** Alta  
**Aggregate:** Usuario, Sesion

### Criterios de Aceptación
- Requiere contraseña actual
- Nueva contraseña validada contra política
- Revoca todas las sesiones activas
- Si `EnableTokenBlacklist = true`: agrega AccessToken actual a blacklist
- Auditoría del evento

---

## RF-AUTH-006: Restablecimiento de Contraseña ✅
**Prioridad:** Alta  
**Aggregate:** Usuario

### Criterios de Aceptación
- Token de un solo uso con expiración configurable (default: 1 hora)
- Siempre retorna éxito (no revelar si email existe)
- Al completar: revoca todas las sesiones activas
- Auditoría del evento

---

## RF-AUTH-007: 2FA TOTP ✅
**Prioridad:** Media  
**Aggregate:** Usuario

### Criterios de Aceptación
- Compatible con Google Authenticator, Authy
- Configurable: habilitado/deshabilitado/obligatorio
- Solo aplica a usuarios `TipoUsuario.Humano`
- 8 códigos de recuperación de un solo uso
- Flujo: Login → tokenTemporal → verificar código → tokens definitivos

---

## RF-AUTH-008: Gestión de Sesiones ✅
**Prioridad:** Alta  
**Aggregate:** Sesion

### Criterios de Aceptación
- Usuario puede ver sus sesiones activas (canal, dispositivo, IP, última actividad)
- Usuario puede cerrar una sesión específica remotamente
- Usuario puede cerrar todas las sesiones excepto la actual
- Admin puede ver sesiones activas de cualquier usuario
- Admin puede cerrar todas las sesiones de un usuario
- Al cerrar sesión: AccessToken va a blacklist, RefreshToken se revoca

---

## RF-AUTH-009: Límite de Sesiones Simultáneas ✅
**Prioridad:** Alta  
**Aggregates:** Sesion, ConfiguracionTenant

### Criterios de Aceptación
- Configurable globalmente en `AuthSettings:MaxSesionesSimultaneas`
- Si `EnableSessionLimitsPerTenant = true`: cada tenant puede tener su propio límite
- Jerarquía: Tenant > Global > Default (5)
- Al superar el límite: `CerrarMasAntigua` o `BloquearNuevoLogin` (configurable)
- `Sistema` e `Integracion` no tienen límite

---

## RF-AUTH-010: Token Blacklist ✅
**Prioridad:** Crítica  
**Servicio:** ITokenBlacklistService

### Criterios de Aceptación
- Configurable: activar/desactivar con `EnableTokenBlacklist`
- Backend configurable: `InMemory` o `Redis`
- Se activa al: logout, revocar sesión, cambiar contraseña, restablecer contraseña
- El middleware JWT verifica la blacklist en cada request
- TTL de cada entrada = tiempo restante de expiración del token

---

## RF-AUTH-011: Tipos de Usuario ✅
**Prioridad:** Alta  
**Aggregate:** Usuario

### Criterios de Aceptación
- Enum: `Humano`, `Sistema`, `Integracion`
- `Humano`: aplican todas las reglas (2FA, bloqueo, límite sesiones)
- `Sistema` e `Integracion`: sin 2FA, sin bloqueo, sin límite de sesiones
- Claim `tipo_usuario` incluido en el JWT

---

## RF-AUTH-012: Canales de Acceso ✅
**Prioridad:** Alta  
**Aggregate:** Sesion

### Criterios de Aceptación
- Enum: `Web`, `Mobile`, `Api`, `Desktop`
- Cada sesión registra el canal de origen
- Claim `canal` incluido en el JWT (en desarrollo, pendiente de agregar)

---

## RF-AUTH-013: Gestión de Roles ✅
**Prioridad:** Alta  
**Aggregate:** Rol

### Criterios de Aceptación
- Roles predefinidos: `SuperAdmin`, `Admin`, `User`
- Roles personalizados por admin
- Permisos en formato `Modulo.Recurso.Accion`
- Roles de sistema no pueden eliminarse
- Roles con usuarios asignados no pueden eliminarse

---

## RF-AUTH-014: Gestión de Usuarios (Admin) ✅
**Prioridad:** Alta  
**Aggregate:** Usuario

### Criterios de Aceptación
- Listar usuarios con paginación y filtro por estado
- Activar, desactivar, desbloquear usuarios
- Asignar/quitar roles globales
- Ver sesiones activas de un usuario
- Cerrar todas las sesiones de un usuario

---

## RF-AUTH-015: Sucursales por Usuario ✅
**Prioridad:** Alta  
**Aggregates:** Sucursal, Usuario  
**Requiere:** `EnableBranches = true`

### Criterios de Aceptación
- Usuarios asignados a una o más sucursales
- Primera sucursal asignada es automáticamente principal
- JWT incluye claim `branch_id` de la sucursal activa
- Usuario puede cambiar su sucursal activa
- Admin puede gestionar asignaciones de usuarios a sucursales

---

## RF-AUTH-016: Roles por Sucursal ✅
**Prioridad:** Alta  
**Aggregate:** AsignacionRol  
**Requiere:** `EnableBranches = true`

### Criterios de Aceptación
- Roles asignados por combinación `usuario + sucursal`
- Un usuario puede tener rol `Admin` en Sucursal A y `Operativo` en Sucursal B
- No duplicar mismo rol en misma sucursal para mismo usuario
- Permisos efectivos calculados según sucursal activa en el JWT

---

## RF-AUTH-017: Catálogo de Acciones ✅
**Prioridad:** Media  
**Aggregate:** Accion  
**Requiere:** `UseActionCatalog = true`

### Criterios de Aceptación
- `Accion` es un aggregate con código, nombre, módulo, descripción, estado
- Acciones pueden activarse/desactivarse
- Admin puede gestionar el catálogo desde `/api/acciones`
- Seed inicial con acciones del sistema al arrancar

---

## RF-AUTH-018: Multi-Tenant Configurable ✅
**Prioridad:** Crítica  
**Infraestructura:** BaseDbContext, TenantMiddleware

### Criterios de Aceptación
- `IsMultiTenant = false`: sistema single-tenant, TenantId ignorado
- `IsMultiTenant = true`: filtrado automático por TenantId en todas las entidades
- Header `X-Tenant-Id` requerido cuando multi-tenant
- Estrategia de resolución configurable: `Header`, `Claim`, `Subdomain`

---

## RF-AUTH-019: Auditoría de Eventos ✅
**Prioridad:** Crítica  
**Entidad:** RegistroAuditoria

### Criterios de Aceptación
- Eventos auditados: login, logout, cambio contraseña, bloqueo, asignación roles
- Registro incluye: fecha UTC, IP, user agent, canal, resultado
- Registros inmutables (solo se agregan, nunca se modifican)

---

## RF-AUTH-020: Configuración de Límites por Tenant ✅
**Prioridad:** Media  
**Aggregate:** ConfiguracionTenant  
**Requiere:** `IsMultiTenant = true` y `EnableSessionLimitsPerTenant = true`

### Criterios de Aceptación
- Admin puede configurar límite de sesiones por tenant
- Jerarquía: Tenant > Global > Default (5)
- `null` = usar límite global

---

## RF-AUTH-021: Perfil del Usuario Autenticado ✅
**Prioridad:** Alta

### Criterios de Aceptación
- Ver perfil propio (datos, roles, estado)
- Cambiar contraseña
- Ver sesiones activas propias
- Cerrar sesiones propias
- Cambiar sucursal activa (si `EnableBranches = true`)

---

## RF-AUTH-022: Permisos Efectivos ✅
**Prioridad:** Media  
**Servicio:** GetPermisosEfectivosQuery

### Criterios de Aceptación
- Si `EnableBranches = false`: permisos de roles globales del usuario
- Si `EnableBranches = true`: permisos de roles en la sucursal activa del JWT
- Retorna lista de códigos de permisos

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 8 |
| Alta | 10 |
| Media | 4 |
| **Total** | **22** |

---

**Fecha:** 2026-04-15
