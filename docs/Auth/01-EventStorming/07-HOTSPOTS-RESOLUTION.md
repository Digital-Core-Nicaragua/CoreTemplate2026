# Event Storming — Hotspots y Resolución

> **Total:** 8 hotspots identificados y resueltos  
> **Fecha:** 2026-04-15

---

## H1: ¿Cómo invalidar tokens antes de su expiración?

**Problema:** JWT es stateless. Una vez emitido, no se puede invalidar sin infraestructura adicional.

**Solución:** Token Blacklist con backend configurable.
- `InMemory`: `ConcurrentDictionary` con limpieza por TTL. Para desarrollo o un solo servidor.
- `Redis`: `SET jti EX ttl`. Para producción con múltiples instancias.
- TTL = tiempo restante del token (cuando expira naturalmente, la entrada también expira).
- Middleware `TokenBlacklistMiddleware` verifica el JTI en cada request.

**Implementación:** `ITokenBlacklistService`, `InMemoryTokenBlacklistService`, `RedisTokenBlacklistService`, `TokenBlacklistMiddleware`

---

## H2: ¿Cómo gestionar límites de sesiones simultáneas?

**Problema:** El límite puede variar por tenant, por configuración global o usar un default.

**Solución:** Jerarquía de límites con `ISesionService`.
1. `ConfiguracionTenant.MaxSesionesSimultaneas` (si `EnableSessionLimitsPerTenant = true`)
2. `AuthSettings.MaxSesionesSimultaneas` (global)
3. Default: 5

Acción configurable: `CerrarMasAntigua` o `BloquearNuevoLogin`.

**Implementación:** `SesionService`, `ConfiguracionTenant`, `IConfiguracionTenantRepository`

---

## H3: ¿Cómo almacenar refresh tokens de forma segura?

**Problema:** Almacenar el refresh token en texto plano es un riesgo de seguridad.

**Solución:** Almacenar solo el hash SHA256 del token.
- El token se genera como 64 bytes aleatorios → Base64
- Se almacena `SHA256(token)` en la BD
- Al verificar: `SHA256(tokenRecibido)` y comparar con el hash almacenado
- Si la BD es comprometida, los tokens no son utilizables

**Implementación:** `ComputarHash()` en `LoginCommandHandler`, `RefreshTokenCommandHandler`, `LogoutCommandHandler`

---

## H4: ¿Cómo manejar 2FA sin romper el flujo de login?

**Problema:** El login con 2FA requiere dos pasos. ¿Cómo mantener el estado entre pasos?

**Solución:** Token temporal JWT de corta duración (5 minutos).
- Paso 1: Login exitoso con 2FA activo → retorna `{ requires2FA: true, tokenTemporal }`
- El `tokenTemporal` contiene solo el `usuarioId` y el claim `tipo: "2fa_temp"`
- Paso 2: `POST /api/auth/2fa/verificar { tokenTemporal, codigo }` → retorna tokens definitivos
- Si el token temporal expira, el usuario debe hacer login nuevamente

**Implementación:** `JwtService.GenerarTokenTemporal2FA()`, `JwtService.ValidarTokenTemporal2FA()`, `Verificar2FACommandHandler`

---

## H5: ¿Cómo hacer multi-tenant configurable (on/off)?

**Problema:** Algunos sistemas son single-tenant, otros multi-tenant. No queremos dos versiones del código.

**Solución:** Flag `IsMultiTenant` en `TenantSettings`.
- `false`: `TenantId` ignorado en todo el sistema, `BaseDbContext` no aplica QueryFilters
- `true`: `BaseDbContext` aplica `QueryFilter` automático por `TenantId` en todas las entidades que implementan `IHasTenant`
- `TenantMiddleware` solo se registra cuando `IsMultiTenant = true`

**Implementación:** `TenantSettings`, `BaseDbContext`, `TenantMiddleware`, `ICurrentTenant`

---

## H6: ¿Cómo hacer sucursales opcionales?

**Problema:** No todos los sistemas necesitan sucursales. Incluirlas siempre agrega complejidad innecesaria.

**Solución:** Flag `EnableBranches` en `OrganizationSettings`.
- `false` (default): No existen tablas de sucursales, no hay endpoints, roles son globales
- `true`: Se registran `ISucursalRepository`, `IAsignacionRolRepository` en DI; JWT incluye `branch_id`
- Los handlers verifican el flag antes de operar

**Implementación:** `OrganizationSettings`, DI condicional en `DependencyInjection.cs`, `JwtService` condicional

---

## H7: ¿Cómo hacer el catálogo de acciones opcional?

**Problema:** El modelo de permisos como strings es suficiente para el 80% de sistemas. El catálogo de acciones agrega complejidad.

**Solución:** Flag `UseActionCatalog` en `AuthSettings`.
- `false` (default): Permisos como strings `Modulo.Recurso.Accion`, sin tabla `Acciones`
- `true`: `Accion` es un aggregate gestionable, `IAccionRepository` registrado en DI
- Los handlers verifican el flag antes de operar

**Implementación:** `AuthSettings.UseActionCatalog`, DI condicional, `CrearAccionCommandHandler`

---

## H8: ¿Cómo manejar límites de sesiones por tenant en multi-tenant?

**Problema:** En multi-tenant, cada cliente puede necesitar un límite diferente de sesiones.

**Solución:** `ConfiguracionTenant` con upsert y jerarquía.
- Solo activo cuando `IsMultiTenant = true` Y `EnableSessionLimitsPerTenant = true`
- Upsert: si no existe la configuración para el tenant, se crea; si existe, se actualiza
- `null` en `MaxSesionesSimultaneas` = usar límite global
- `SesionService` consulta la jerarquía en cada verificación

**Implementación:** `ConfiguracionTenant`, `IConfiguracionTenantRepository`, `SesionService.ObtenerLimiteAsync()`, `TenantsController`

---

## Resumen

| Hotspot | Estado | Solución |
|---|---|---|
| H1: Invalidar tokens | ✅ Resuelto | Token Blacklist (Redis/InMemory) |
| H2: Límites de sesiones | ✅ Resuelto | ISesionService con jerarquía |
| H3: Seguridad refresh token | ✅ Resuelto | Hash SHA256 |
| H4: Flujo 2FA | ✅ Resuelto | Token temporal JWT 5 min |
| H5: Multi-tenant configurable | ✅ Resuelto | Flag IsMultiTenant |
| H6: Sucursales opcionales | ✅ Resuelto | Flag EnableBranches |
| H7: Catálogo acciones opcional | ✅ Resuelto | Flag UseActionCatalog |
| H8: Límites por tenant | ✅ Resuelto | ConfiguracionTenant + jerarquía |

**Todos los hotspots resueltos ✅**

---

**Fecha:** 2026-04-15
