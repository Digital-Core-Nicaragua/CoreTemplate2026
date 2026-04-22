# Requerimientos Funcionales — Portal de Clientes

> **Fecha:** Pendiente de implementación
> **Estado:** Diseño completo — pendiente de desarrollo
> **Total:** 10 RF

---

## RF-PC-001: Habilitación del Portal por Tenant
**Prioridad:** Crítica
**Aggregate:** ConfiguracionTenant

### Descripción
El admin puede habilitar o deshabilitar el registro de clientes por tenant.
Cuando está deshabilitado, el endpoint de registro retorna 403.

### Criterios de Aceptación
- Flag `RegistroHabilitado` por tenant, gestionable desde admin
- Default: deshabilitado
- Si `EnableCustomerPortal = false` en config global, todo el portal está inactivo
- Si `EnableCustomerPortal = true` pero `RegistroHabilitado = false` en el tenant, solo el registro está bloqueado — los clientes existentes pueden seguir haciendo login

---

## RF-PC-002: Registro de Cliente
**Prioridad:** Crítica
**Aggregate:** UsuarioCliente

### Descripción
Un cliente externo puede registrarse en el portal con email y contraseña, o con número de teléfono (WhatsApp/SMS) cuando `RegistroPorTelefono.Enabled = true`.

### Criterios de Aceptación
- Solo disponible si `RegistroHabilitado = true` para el tenant
- Email único por tenant (cuando se registra con email)
- Teléfono único por tenant (cuando se registra con teléfono)
- Al menos uno de los dos (email o teléfono) debe estar presente
- Contraseña validada contra la misma política de `PasswordPolicy`
- Estado inicial: `Registered`
- Si `RequireEmailVerification = true`: genera token de verificación, estado queda en `Registered`
- Si `RequireEmailVerification = false`: estado pasa directamente a `Active`
- Si `RequirePhoneVerification = true`: teléfono requerido al registrarse
- Registro con OAuth (Google/Facebook) crea el cliente con email ya verificado → estado `Active`
- Registro por teléfono: genera OTP de 6 dígitos, expira en 10 minutos, envía por WhatsApp o SMS

---

## RF-PC-003: Verificación de Email
**Prioridad:** Alta
**Aggregate:** UsuarioCliente
**Requiere:** `RequireEmailVerification = true`

### Criterios de Aceptación
- Token de un solo uso con expiración de 24 horas
- Al verificar: estado pasa de `Registered` a `Verified`
- Si `RequirePhoneVerification = false`: estado pasa directamente a `Active`
- Admin puede reenviar el token de verificación
- Token expirado: cliente debe solicitar reenvío

---

## RF-PC-004: Verificación de Teléfono
**Prioridad:** Media
**Aggregate:** UsuarioCliente
**Requiere:** `RequirePhoneVerification = true`

### Criterios de Aceptación
- Código numérico de 6 dígitos con expiración de 10 minutos
- Solo disponible después de verificar email (si aplica)
- Al verificar: estado pasa a `Active`
- Admin puede reenviar el código

---

## RF-PC-005: Login de Cliente
**Prioridad:** Crítica
**Aggregate:** UsuarioCliente

### Descripción
El cliente se autentica en el portal con email y contraseña, o con proveedor OAuth.

### Criterios de Aceptación
- Endpoint separado `POST /api/portal/login` — independiente del login de usuarios internos
- Solo clientes con estado `Verified` o `Active` pueden hacer login
- `Registered` y `Blocked` reciben 401
- Genera JWT propio con claims: `sub`, `email`, `tipo = cliente`, `estado_cliente`, `tenant_id`
- Bloqueo automático por intentos fallidos (misma config `LockoutSettings`)
- Login con Google: `POST /api/portal/login/google` — valida `idToken` del frontend
- Login con Facebook: `POST /api/portal/login/facebook` — valida `accessToken` del frontend
- Si el email OAuth no existe → crea cliente automáticamente con estado `Active`
- Si el email OAuth ya existe con otro proveedor → vincula el nuevo proveedor al mismo cliente

---

## RF-PC-006: Refresh Token y Logout
**Prioridad:** Alta
**Aggregate:** UsuarioCliente

### Criterios de Aceptación
- Refresh token de un solo uso (rotación), igual que usuarios internos
- Logout revoca la sesión y agrega el AccessToken a la blacklist (si `EnableTokenBlacklist = true`)

---

## RF-PC-007: Cambio y Restablecimiento de Contraseña
**Prioridad:** Alta
**Aggregate:** UsuarioCliente

### Criterios de Aceptación
- Cambio de contraseña requiere contraseña actual
- Clientes OAuth sin contraseña local pueden establecer una desde el portal
- Restablecimiento por email: token de un solo uso, expiración configurable
- Al restablecer: revoca todas las sesiones activas del cliente
- Siempre retorna éxito (no revelar si el email existe)

---

## RF-PC-008: Gestión de Sesiones del Cliente
**Prioridad:** Media
**Aggregate:** UsuarioCliente
**Requiere:** `EnableSessionManagement = true`

### Criterios de Aceptación
- Cliente puede ver sus sesiones activas (canal, IP, última actividad)
- Cliente puede cerrar una sesión específica
- Cliente puede cerrar todas las sesiones excepto la actual
- Al cerrar sesión: AccessToken va a blacklist, RefreshToken se revoca

---

## RF-PC-009: Gestión de Clientes por Admin
**Prioridad:** Alta
**Aggregate:** UsuarioCliente

### Criterios de Aceptación
- Admin puede listar clientes del tenant (paginado, filtrable por estado)
- Admin puede ver detalle de un cliente
- Admin puede bloquear un cliente (estado → `Blocked`)
- Admin puede reactivar un cliente bloqueado (estado → `Active` o `Verified` según corresponda)
- Admin puede habilitar/deshabilitar el registro por tenant (`RegistroHabilitado`)
- Admin puede reenviar token de verificación de email o teléfono

---

## RF-PC-010: Perfil del Cliente Autenticado
**Prioridad:** Media
**Aggregate:** UsuarioCliente

### Criterios de Aceptación
- Cliente puede ver su perfil (email, nombre, estado, proveedores vinculados)
- Cliente puede cambiar su contraseña
- Cliente puede ver sus sesiones activas (si `EnableSessionManagement = true`)
- Cliente puede vincular/desvincular proveedores OAuth adicionales

---

## RF-PC-011: Registro por Teléfono (WhatsApp / SMS)
**Prioridad:** Media
**Aggregate:** UsuarioCliente
**Requiere:** `RegistroPorTelefono.Enabled = true`

### Descripción
Un cliente puede registrarse e identificarse usando su número de teléfono en lugar de email.

### Criterios de Aceptación
- Solo disponible si `RegistroPorTelefono.Enabled = true` en `CustomerPortalSettings`
- Teléfono en formato E.164 (`+521234567890`)
- Teléfono único por tenant
- Email no requerido cuando el registro es por teléfono
- Al registrarse: genera OTP de 6 dígitos, expira en `OtpExpirationMinutes` (default: 10 min)
- OTP se envía por WhatsApp o SMS según `Proveedor` configurado
- Al verificar OTP: estado pasa a `Active` directamente
- Login posterior: `POST /api/portal/login/telefono` con teléfono + OTP nuevo
- El sistema implementador provee `INotificacionClienteService` para el envío real

### Configuración
```json
{
  "CustomerPortalSettings": {
    "RegistroPorTelefono": {
      "Enabled": false,
      "Proveedor": "WhatsApp",
      "OtpExpirationMinutes": 10
    }
  }
}
```

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 3 |
| Alta | 4 |
| Media | 4 |
| **Total** | **11** |

---

## Configuración completa

```json
{
  "CustomerPortalSettings": {
    "EnableCustomerPortal": false,
    "RequireEmailVerification": true,
    "RequirePhoneVerification": false,
    "EnableSessionManagement": false,
    "RegistroPorTelefono": {
      "Enabled": false,
      "Proveedor": "WhatsApp",
      "OtpExpirationMinutes": 10
    },
    "OAuth": {
      "Google": {
        "Enabled": false,
        "ClientId": ""
      },
      "Facebook": {
        "Enabled": false,
        "AppId": "",
        "AppSecret": ""
      }
    }
  }
}
```

---

*Ver `docs/PLAN-PORTAL-CLIENTES.md` para el plan de diseño completo.*
