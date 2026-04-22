# Reglas de Negocio — Módulo Auth

> **Fecha:** 2026-04-15  
> **Total:** 30 reglas de negocio

---

## IAM — Identity & Access Management (12 reglas)

### RN-AUTH-001: Credenciales Inválidas — Mensaje Genérico
El sistema nunca revela si el email existe o no. Siempre retorna "Las credenciales son inválidas."

### RN-AUTH-002: Bloqueo Automático por Intentos Fallidos
Después de `LockoutSettings:MaxFailedAttempts` (default: 5) intentos fallidos consecutivos, la cuenta se bloquea por `LockoutDurationMinutes` (default: 15 minutos).
- Solo aplica a usuarios `TipoUsuario.Humano`
- `Sistema` e `Integracion` nunca se bloquean

### RN-AUTH-003: Desbloqueo Automático
Si `LockoutSettings:AutoUnlock = true`, el usuario se desbloquea automáticamente cuando `BloqueadoHasta` es pasado. Se verifica en `PuedeAutenticarse()`.

### RN-AUTH-004: Rotación de Refresh Token
El refresh token se rota en cada uso. El token anterior se invalida al emitir uno nuevo. El token se almacena como hash SHA256.

### RN-AUTH-005: Revocación de Sesiones al Cambiar Contraseña
Al cambiar o restablecer la contraseña, todas las sesiones activas del usuario se revocan. Si `EnableTokenBlacklist = true`, el AccessToken actual también va a la blacklist.

### RN-AUTH-006: Token de Restablecimiento de Un Solo Uso
El token de restablecimiento de contraseña expira en `AuthSettings:PasswordResetTokenExpirationHours` (default: 1 hora) y solo puede usarse una vez.

### RN-AUTH-007: 2FA Solo para Usuarios Humano
El 2FA solo aplica a usuarios `TipoUsuario.Humano`. `Sistema` e `Integracion` nunca requieren 2FA, independientemente de la configuración.

### RN-AUTH-008: Códigos de Recuperación 2FA de Un Solo Uso
Se generan 8 códigos al activar el 2FA. Cada código solo puede usarse una vez. Se almacenan como hash SHA256.

### RN-AUTH-009: SuperAdmin No Puede Desactivarse
El usuario con rol `SuperAdmin` no puede ser desactivado. El sistema retorna error si se intenta.

### RN-AUTH-010: Permisos del SuperAdmin No Modificables
Los permisos del rol `SuperAdmin` no pueden modificarse. El rol `SuperAdmin` tiene acceso total al sistema.

### RN-AUTH-011: Usuario Debe Tener Al Menos Un Rol
No se puede quitar el último rol de un usuario. El sistema retorna error si se intenta.

### RN-AUTH-012: Política de Contraseñas Configurable
La política se configura en `PasswordPolicy`: longitud mínima, mayúsculas, minúsculas, dígitos, caracteres especiales.

---

## Sesiones (8 reglas)

### RN-AUTH-013: Límite de Sesiones Simultáneas
Jerarquía de límites (mayor prioridad primero):
1. Configuración por tenant (`ConfiguracionTenant.MaxSesionesSimultaneas`)
2. Configuración global (`AuthSettings:MaxSesionesSimultaneas`)
3. Default del sistema: 5

### RN-AUTH-014: Acción al Llegar al Límite
Configurable con `AuthSettings:AccionAlLlegarLimiteSesiones`:
- `CerrarMasAntigua`: cierra la sesión con menor `UltimaActividad`
- `BloquearNuevoLogin`: rechaza el nuevo login con error descriptivo

### RN-AUTH-015: Exención del Límite por Tipo de Usuario
`Sistema` e `Integracion` no tienen límite de sesiones simultáneas. La verificación se omite completamente.

### RN-AUTH-016: Token en Blacklist Rechazado
Si `EnableTokenBlacklist = true`, un AccessToken en la blacklist es rechazado con 401 aunque no haya expirado. Se verifica en el middleware `TokenBlacklistMiddleware`.

### RN-AUTH-017: TTL de Blacklist = Tiempo Restante del Token
Al agregar un token a la blacklist, el TTL es `expiracion - ahora`. Cuando el token expira naturalmente, la entrada en la blacklist también expira.

### RN-AUTH-018: Sesión Expirada No Es Válida
`Sesion.EsValida = EsActiva && DateTime.UtcNow < ExpiraEn`. Una sesión puede estar activa pero expirada.

### RN-AUTH-019: Límite por Tenant Requiere Multi-Tenant
`EnableSessionLimitsPerTenant` solo tiene efecto cuando `IsMultiTenant = true`. Si el sistema es single-tenant, se usa siempre el límite global.

### RN-AUTH-020: Límite por Tenant Debe Ser Positivo
`ConfiguracionTenant.MaxSesionesSimultaneas` debe ser > 0 si se especifica. `null` significa "usar el límite global".

---

## Authorization (6 reglas)

### RN-AUTH-021: Permisos en Formato Modulo.Recurso.Accion
Los permisos se definen como strings en formato `Modulo.Recurso.Accion`. Ejemplo: `Usuarios.Roles.Crear`, `Catalogos.Items.Gestionar`.

### RN-AUTH-022: Roles por Sucursal Requieren EnableBranches
`AsignacionRol` solo existe cuando `EnableBranches = true`. Sin sucursales, los roles son globales por usuario (`UsuarioRol`).

### RN-AUTH-023: No Duplicar Rol en Misma Sucursal
La combinación `UsuarioId + SucursalId + RolId` debe ser única. Se valida en el handler antes de crear y se refuerza con índice único en BD.

### RN-AUTH-024: Catálogo de Acciones Requiere UseActionCatalog
`Accion` como aggregate solo existe cuando `UseActionCatalog = true`. Sin catálogo, los permisos son strings estáticos.

### RN-AUTH-025: Código de Acción Único y con Formato
El código de una `Accion` debe ser único y contener al menos un punto (formato `Modulo.Recurso.Accion`).

### RN-AUTH-026: Permisos Efectivos Según Sucursal Activa
Cuando `EnableBranches = true`, los permisos efectivos se calculan según los roles asignados en la sucursal activa del JWT (`branch_id`). Sin sucursales, se usan los roles globales.

---

## Sucursales (4 reglas)

### RN-AUTH-027: Primera Sucursal Es Principal
Al asignar la primera sucursal a un usuario, se marca automáticamente como principal.

### RN-AUTH-028: Usuario Debe Tener Al Menos Una Sucursal
No se puede remover la única sucursal de un usuario. El sistema retorna error si se intenta.

### RN-AUTH-029: Al Remover Sucursal Principal → Nueva Principal
Si se remueve la sucursal principal, la siguiente sucursal en la lista se convierte automáticamente en principal.

### RN-AUTH-030: Claim branch_id en JWT
Cuando `EnableBranches = true`, el JWT incluye el claim `branch_id` con la sucursal principal del usuario. El usuario puede cambiar su sucursal activa con `PUT /api/perfil/sucursal-activa`.

---

## Portal de Clientes (4 reglas)

### RN-AUTH-031: Al Menos Email o Teléfono Requerido
Un `UsuarioCliente` debe tener al menos email o teléfono. No pueden estar ambos ausentes. Se valida en los factory methods del aggregate.

### RN-AUTH-032: Unicidad de Email y Teléfono por Tenant
Email y teléfono son únicos por tenant de forma independiente. Se implementan como índices filtrados (`WHERE Email IS NOT NULL`, `WHERE Telefono IS NOT NULL`) para permitir nulls.

### RN-AUTH-033: OTP de Teléfono de Un Solo Uso
El código OTP generado para registro o login por teléfono expira en `OtpExpirationMinutes` (default: 10 min) y se invalida al usarse. No puede reutilizarse.

### RN-AUTH-034: Registro por Teléfono Requiere Flag Habilitado
El endpoint `POST /api/portal/registro/telefono` solo está disponible cuando `CustomerPortalSettings:RegistroPorTelefono:Enabled = true`. Si está deshabilitado, retorna 404.

---

## Resumen

| Bounded Context | Reglas |
|---|---|
| IAM | 12 |
| Sesiones | 8 |
| Authorization | 6 |
| Sucursales | 4 |
| Portal de Clientes | 4 |
| **Total** | **34** |

---

**Fecha:** 2026-04-15
