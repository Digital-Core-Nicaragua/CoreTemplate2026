# Auth — Requisitos

> **Versión**: 2.0 — Incluye Sesiones, Tipos de Usuario, Canales, Token Blacklist, Sucursales, Catálogo de Acciones

---

## 1. Requisitos Funcionales

### RF-AUTH-001: Registro de usuario ✅ Implementado
- Email único por tenant (o global si single-tenant)
- Contraseña con política configurable
- Estado inicial: `Pendiente` hasta activación
- Tipo de usuario asignable al crear (`Humano`, `Sistema`, `Integracion`)

### RF-AUTH-002: Login ✅ Implementado
- Autenticación con email y contraseña
- Genera AccessToken JWT + crea Sesión con RefreshToken
- La sesión registra: canal, dispositivo, IP, user agent
- Verifica límite de sesiones simultáneas al crear nueva sesión
- Bloqueo automático por intentos fallidos
- Auditoría de cada intento

### RF-AUTH-003: Refresh Token ✅ Implementado
- Renovación sin re-autenticarse
- Rotación: el token anterior se revoca al emitir uno nuevo
- Actualiza `UltimaActividad` de la sesión

### RF-AUTH-004: Logout ✅ Implementado
- Revoca el RefreshToken
- Marca la sesión como inactiva
- Agrega el AccessToken a la blacklist (si `EnableTokenBlacklist = true`)
- Auditoría

### RF-AUTH-005: Cambio de contraseña ✅ Implementado
- Requiere contraseña actual
- Revoca todas las sesiones activas
- Auditoría

### RF-AUTH-006: Restablecimiento de contraseña ✅ Implementado
- Token de un solo uso con expiración configurable
- Revoca todas las sesiones activas al completar
- Auditoría

### RF-AUTH-007: 2FA TOTP ✅ Implementado
- Configurable: habilitado/deshabilitado/obligatorio
- Compatible con Google Authenticator, Authy
- Códigos de recuperación de un solo uso
- Solo aplica a usuarios tipo `Humano`

### RF-AUTH-008: Gestión de sesiones ⏳ Pendiente
- El usuario puede ver sus sesiones activas con: canal, dispositivo, IP, última actividad
- El usuario puede cerrar una sesión específica remotamente
- El usuario puede cerrar todas las sesiones excepto la actual
- El admin puede ver las sesiones activas de cualquier usuario
- El admin puede cerrar todas las sesiones de un usuario
- Al cerrar sesión: AccessToken va a blacklist, RefreshToken se revoca

### RF-AUTH-009: Límite de sesiones simultáneas ⏳ Pendiente
- Configurable globalmente en `AuthSettings:MaxSesionesSimultaneas`
- Si `EnableSessionLimitsPerTenant = true`: cada tenant puede tener su propio límite
- Jerarquía: Tenant > Global > Default (5)
- Al superar el límite, comportamiento configurable:
  - `CerrarMasAntigua`: cierra la sesión más antigua automáticamente
  - `BloquearNuevoLogin`: rechaza el nuevo login con error descriptivo
- Usuarios tipo `Sistema` e `Integracion` no tienen límite de sesiones

### RF-AUTH-010: Tipos de usuario ⏳ Pendiente
- Enum: `Humano`, `Sistema`, `Integracion`
- Extensible por el sistema implementador
- Comportamiento diferenciado:
  - `Humano`: aplican todas las reglas (2FA, bloqueo, límite sesiones)
  - `Sistema`: sin 2FA, sin límite de sesiones, sin bloqueo por intentos
  - `Integracion`: sin 2FA, sin límite de sesiones, sin bloqueo por intentos

### RF-AUTH-011: Canales de acceso ⏳ Pendiente
- Enum: `Web`, `Mobile`, `Api`, `Desktop`
- Extensible por el sistema implementador
- Cada sesión registra el canal de origen
- Si `UseActionCatalog = true`: los permisos pueden restringirse por canal
- Útil para auditoría y análisis de patrones de acceso

### RF-AUTH-012: Token Blacklist ⏳ Pendiente
- Configurable: activar/desactivar con `EnableTokenBlacklist`
- Backend configurable: `InMemory` o `Redis`
- Se activa al: logout, revocar sesión, cambiar contraseña, restablecer contraseña
- El middleware JWT verifica la blacklist en cada request
- TTL de cada entrada = tiempo restante de expiración del token
- `InMemory`: válido para un solo servidor, se pierde al reiniciar
- `Redis`: válido para múltiples instancias, persistente

### RF-AUTH-013: Gestión de roles ✅ Implementado
- Roles predefinidos: `SuperAdmin`, `Admin`, `User`
- Roles personalizados por admin
- Permisos en formato `Modulo.Recurso.Accion`

### RF-AUTH-014: Gestión de usuarios (admin) ✅ Implementado
- Listar, ver, activar, desactivar, desbloquear
- Asignar/quitar roles

### RF-AUTH-015: Sucursales por usuario ⏳ Pendiente (configurable)
Requiere `OrganizationSettings:EnableBranches = true`.

- Un usuario puede pertenecer a múltiples sucursales
- Cada usuario tiene exactamente una sucursal principal
- El token JWT incluye el claim `branch_id` de la sucursal activa
- El usuario puede cambiar su sucursal activa (genera nuevo token)
- Admin puede asignar/remover sucursales a usuarios
- Invariante: un usuario debe tener al menos una sucursal asignada
- Invariante: solo una sucursal puede ser la principal

### RF-AUTH-016: Roles por sucursal ⏳ Pendiente (requiere EnableBranches)
- Los roles se asignan por combinación `usuario + sucursal`
- Un usuario puede tener rol `Admin` en Sucursal A y `Operativo` en Sucursal B
- Los permisos efectivos se calculan según la sucursal activa en la sesión
- Aggregate `AsignacionRol` gestiona estas combinaciones
- Invariante: no puede haber el mismo rol asignado dos veces al mismo usuario en la misma sucursal

### RF-AUTH-017: Catálogo de Acciones ⏳ Pendiente (configurable)
Requiere `AuthSettings:UseActionCatalog = true`.

- `Accion` es un aggregate con: código, nombre, módulo, descripción, estado
- Las acciones pueden habilitarse/deshabilitarse por sucursal (si `EnableBranches = true`)
- Los permisos de los roles referencian acciones del catálogo
- Los permisos pueden restringirse por canal de acceso
- Admin puede gestionar el catálogo de acciones
- Seed inicial con acciones del sistema

---

## 2. Requisitos No Funcionales

### RNF-AUTH-001: Seguridad de contraseñas ✅
- BCrypt work factor 12
- Nunca texto plano, nunca en respuestas

### RNF-AUTH-002: Tokens JWT ✅
- Firmado con clave secreta (mínimo 256 bits)
- Claims: `sub`, `email`, `name`, `roles`, `tenantId`, `tipo_usuario`, `canal`
- Con sucursales: agrega `branch_id`

### RNF-AUTH-003: Política de contraseñas ✅ Configurable

### RNF-AUTH-004: Bloqueo de cuenta ✅ Configurable

### RNF-AUTH-005: Auditoría ✅
- Todos los eventos registrados con: fecha UTC, IP, user agent, canal, resultado
- Inmutables

### RNF-AUTH-006: Token Blacklist ⏳
- Verificación en O(1) con Redis
- TTL automático basado en expiración del token
- No afecta rendimiento si está desactivado

### RNF-AUTH-007: Rendimiento
- Login < 500ms bajo carga normal
- Verificación de blacklist < 5ms con Redis

---

## 3. Reglas de Negocio

| ID | Regla | Estado |
|---|---|---|
| RN-001 | Email único por tenant | ✅ |
| RN-002 | Usuario bloqueado no puede autenticarse | ✅ |
| RN-003 | Usuario inactivo no puede autenticarse | ✅ |
| RN-004 | RefreshToken de un solo uso (rotación) | ✅ |
| RN-005 | Cambio de contraseña revoca todas las sesiones | ✅ |
| RN-006 | Códigos de recuperación 2FA de un solo uso | ✅ |
| RN-007 | SuperAdmin no puede ser desactivado | ✅ |
| RN-008 | Usuario debe tener al menos un rol | ✅ |
| RN-009 | Permisos del SuperAdmin no modificables | ✅ |
| RN-010 | Token de restablecimiento expira en 1h (configurable) | ✅ |
| RN-011 | Al superar límite de sesiones: cerrar más antigua o bloquear | ⏳ |
| RN-012 | Usuarios Sistema/Integracion no tienen límite de sesiones | ⏳ |
| RN-013 | Usuarios Sistema/Integracion no requieren 2FA | ⏳ |
| RN-014 | Token en blacklist es rechazado aunque no haya expirado | ⏳ |
| RN-015 | Usuario debe tener al menos una sucursal (si EnableBranches) | ⏳ |
| RN-016 | Solo una sucursal puede ser la principal | ⏳ |
| RN-017 | No duplicar mismo rol en misma sucursal para mismo usuario | ⏳ |
| RN-018 | Permisos efectivos dependen de sucursal activa en sesión | ⏳ |
| RN-019 | Límite de sesiones por tenant tiene prioridad sobre global | ⏳ |
