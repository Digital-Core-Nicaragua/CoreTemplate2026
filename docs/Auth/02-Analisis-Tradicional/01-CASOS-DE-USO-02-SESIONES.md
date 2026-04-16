# Casos de Uso — Sesiones

> **Grupo:** Sesiones  
> **Códigos:** CU-AUTH-013 a CU-AUTH-020  
> **Fecha:** 2026-04-15

---

## CU-AUTH-013: Ver Mis Sesiones Activas

**Actor:** Usuario autenticado  
**Endpoint:** `GET /api/perfil/sesiones`

**Flujo:** Sistema retorna sesiones activas y no expiradas del usuario autenticado con: canal, dispositivo, IP, user agent, última actividad, expiración.

---

## CU-AUTH-014: Cerrar Sesión Específica

**Actor:** Usuario autenticado  
**Endpoint:** `DELETE /api/perfil/sesiones/{id}`

**Flujo:** Sistema verifica que la sesión pertenece al usuario autenticado, la revoca.

**Alternativo:** Sesión no encontrada o no pertenece al usuario → 404.

---

## CU-AUTH-015: Cerrar Todas las Sesiones Excepto la Actual

**Actor:** Usuario autenticado  
**Endpoint:** `DELETE /api/perfil/sesiones/otras?sesionActualId={id}`

**Flujo:** Sistema obtiene todas las sesiones activas del usuario, revoca todas excepto la indicada como actual.

---

## CU-AUTH-016: Ver Sesiones de un Usuario (Admin)

**Actor:** Administrador  
**Endpoint:** `GET /api/usuarios/{id}/sesiones`

**Flujo:** Sistema retorna sesiones activas del usuario indicado.

**Alternativo:** Usuario no encontrado → 404.

---

## CU-AUTH-017: Cerrar Todas las Sesiones de un Usuario (Admin)

**Actor:** Administrador  
**Endpoint:** `DELETE /api/usuarios/{id}/sesiones`

**Flujo:** Sistema verifica que el usuario existe, revoca todas sus sesiones activas.

---

## CU-AUTH-018: Verificar Token en Blacklist

**Actor:** Sistema (middleware)  
**Trigger:** Cada request HTTP con Bearer token

**Flujo:**
1. Middleware extrae Bearer token del header
2. Extrae JTI del token (sin validar firma)
3. Consulta `ITokenBlacklistService.EstaEnBlacklistAsync(jti)`
4. Si está en blacklist → HTTP 401 `{ message: "El token ha sido revocado." }`
5. Si no está → continuar al siguiente middleware

---

## CU-AUTH-019: Configurar Límite de Sesiones por Tenant

**Actor:** Administrador  
**Endpoint:** `PUT /api/tenants/{tenantId}/limite-sesiones`  
**Requiere:** `IsMultiTenant = true` y `EnableSessionLimitsPerTenant = true`

**Flujo:** Sistema hace upsert de `ConfiguracionTenant` con el nuevo límite. `null` = usar límite global.

---

## CU-AUTH-020: Ver Configuración de Tenant

**Actor:** Administrador  
**Endpoint:** `GET /api/tenants/{tenantId}/configuracion`

**Flujo:** Sistema retorna `ConfiguracionTenantDto`. Si no existe configuración → retorna `null`.

---

**Fecha:** 2026-04-15
