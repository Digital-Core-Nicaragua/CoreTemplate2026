# Event Storming — Process Level Parte 3

> **Procesos:** Sucursales, Cambio de Contexto, Límites de Sesiones por Tenant  
> **Requiere:** `OrganizationSettings:EnableBranches = true`  
> **Fecha:** 2026-04-15

---

## PROCESO 14: Gestión de Sucursales

```
👤 Administrador → 🔵 CrearSucursal { codigo, nombre, tenantId? }
  🟣 POLÍTICA: Verificar EnableBranches == true
  SI false: → Error "Sucursales no habilitadas"

  🟢 ISucursalRepository → ExistsByCodigo(codigo, tenantId)
  SI existe: → Error "Código ya existe"

  🟡 Sucursal → Crear { codigo, nombre, tenantId }
  🟣 POLÍTICA: Código se convierte a MAYÚSCULAS
  🟠 SucursalCreada

👤 Administrador → 🔵 DesactivarSucursal { sucursalId }
  🟢 ISucursalRepository → GetById(sucursalId)
  🟡 Sucursal → Desactivar
  🟠 SucursalDesactivada

👤 Administrador → 🔵 ListarSucursales
  🟢 ICurrentTenant → TenantId
  🟢 ISucursalRepository → GetAll(tenantId)
  → Retornar lista de SucursalDto
```

---

## PROCESO 15: Asignación de Sucursales a Usuarios

```
👤 Administrador → 🔵 AsignarSucursalAUsuario { usuarioId, sucursalId }
  🟢 IUsuarioRepository → GetById(usuarioId)
  SI no existe: → Error

  🟢 ISucursalRepository → GetById(sucursalId)
  SI no activa: → Error "Sucursal no existe o está inactiva"

  🟡 Usuario → AsignarSucursal(sucursalId)
  🟣 POLÍTICA: Si es la primera sucursal → marcar como principal
  🟣 POLÍTICA: No duplicar sucursal ya asignada
  🟠 SucursalAsignada

👤 Administrador → 🔵 RemoverSucursalDeUsuario { usuarioId, sucursalId }
  🟢 IUsuarioRepository → GetById(usuarioId)
  🟡 Usuario → RemoverSucursal(sucursalId)
  🟣 POLÍTICA: Usuario debe tener al menos una sucursal
  🟣 POLÍTICA: Si era principal → asignar siguiente como principal
  🟠 SucursalRemovida

👤 Administrador → 🔵 VerSucursalesDeUsuario { usuarioId }
  🟢 IUsuarioRepository → GetById(usuarioId)
  → Para cada UsuarioSucursal:
      🟢 ISucursalRepository → GetById(sucursalId)
  → Retornar lista de UsuarioSucursalDto { sucursalId, codigo, nombre, esPrincipal }
```

---

## PROCESO 16: Cambio de Sucursal Activa

```
👤 Usuario → 🔵 CambiarSucursalActiva { sucursalId }
  🟢 ICurrentUser → Id
  🟢 IUsuarioRepository → GetById(currentUser.Id)

  🟡 Usuario → CambiarSucursalPrincipal(sucursalId)
  🟣 POLÍTICA: Verificar que usuario tiene asignada esa sucursal
  SI no tiene: → Error "No tiene asignada esta sucursal"

  🟠 SucursalPrincipalCambiada

  🟢 ISucursalRepository → GetById(sucursalId)
  → Retornar SucursalDto { id, codigo, nombre, esActiva }

  🔴 HOTSPOT RESUELTO: El JWT no se regenera automáticamente.
  El cliente debe hacer un nuevo login o refresh para obtener
  el JWT con el nuevo branch_id. El endpoint retorna la nueva
  sucursal activa para que el cliente sepa que debe renovar el token.
```

---

## PROCESO 17: JWT con branch_id (EnableBranches = true)

```
🤖 JwtService → GenerarAccessToken(usuario)

  → Construir claims base:
      sub, email, name, jti, tipo_usuario, tenant_id, roles[]

  SI OrganizationSettings.EnableBranches == true:
    🟢 ICurrentBranch → BranchId (del request actual)
    SI BranchId != null:
      → Usar BranchId del request (cambio de sucursal activa)
    SI BranchId == null:
      → Usar usuario.Sucursales.FirstOrDefault(s => s.EsPrincipal)?.SucursalId
    SI branchId encontrado:
      → Agregar claim "branch_id": branchId.ToString()

  → Firmar y retornar JWT
```

---

## PROCESO 18: Límites de Sesiones por Tenant

```
👤 Administrador → 🔵 ConfigurarLimiteSesionesTenant { tenantId, maxSesiones? }
  🟣 POLÍTICA: Verificar IsMultiTenant == true Y EnableSessionLimitsPerTenant == true
  SI false: → Error "Límites por tenant no habilitados"

  SI maxSesiones != null Y maxSesiones < 1:
    → Error "El límite debe ser mayor a 0"

  🟢 IConfiguracionTenantRepository → GetByTenantId(tenantId)

  SI no existe:
    🟡 ConfiguracionTenant → Crear { tenantId, maxSesiones }
    🟠 ConfiguracionTenantCreada
  SI existe:
    🟡 ConfiguracionTenant → ActualizarLimiteSesiones(maxSesiones)
    🟠 ConfiguracionTenantActualizada

👤 Administrador → 🔵 VerConfiguracionTenant { tenantId }
  🟢 IConfiguracionTenantRepository → GetByTenantId(tenantId)
  → Retornar ConfiguracionTenantDto { tenantId, maxSesiones, modificadoEn }
  SI no existe: → Retornar null (usa límite global)
```

---

## PROCESO 19: Resolución de Límite de Sesiones (Jerarquía)

```
🤖 SesionService → ObtenerLimite(ct)

  Jerarquía (mayor prioridad primero):

  NIVEL 1: Configuración por Tenant
    SI TenantSettings.IsMultiTenant == true
    Y TenantSettings.EnableSessionLimitsPerTenant == true
    Y ICurrentTenant.TenantId != null:
      🟢 IConfiguracionTenantRepository → GetByTenantId(tenantId)
      SI config != null Y config.MaxSesionesSimultaneas != null:
        → Retornar config.MaxSesionesSimultaneas

  NIVEL 2: Configuración Global
    → Retornar AuthSettings.MaxSesionesSimultaneas

  NIVEL 3: Default del Sistema
    (AuthSettings.MaxSesionesSimultaneas default = 5)
```

---

## PROCESO 20: 2FA — Activación Completa

```
👤 Usuario → 🔵 ActivarDosFactores
  🟣 POLÍTICA: Verificar AuthSettings.TwoFactorEnabled == true
  SI false: → Error "2FA no habilitado en este sistema"

  🟢 ICurrentUser → Id
  🟢 IUsuarioRepository → GetById(id)
  SI TwoFactorActivo: → Error "2FA ya está activo"

  🔵 ITotpService → GenerarSecretKey
  🔵 ITotpService → GenerarQrCodeUri(email, secretKey, issuer)
  🔵 ITotpService → GenerarCodigosRecuperacion (8 códigos)
  → Hashear cada código con SHA256

  🟡 Usuario → GuardarSecretKeyTemporal(secretKey)
  🟠 SecretKeyTemporalGuardada

  → Retornar { qrCodeUri, secretKey, codigosRecuperacion }
  (El usuario escanea el QR y confirma con el primer código)

👤 Usuario → 🔵 Confirmar2FA { codigo }
  🟢 IUsuarioRepository → GetById(currentUser.Id)
  SI TwoFactorSecretKey == null: → Error

  🔵 ITotpService → ValidarCodigo(secretKey, codigo)
  SI inválido: → Error "Código inválido"

  🔵 ITotpService → GenerarCodigosRecuperacion (8 nuevos)
  → Hashear cada código
  🟡 Usuario → ActivarDosFactores(secretKey, codigosHash)
  🟠 DosFactoresActivado

👤 Usuario → 🔵 DesactivarDosFactores { codigo }
  🟢 IUsuarioRepository → GetById(currentUser.Id)
  SI !TwoFactorActivo: → Error

  🔵 ITotpService → ValidarCodigo(secretKey, codigo)
  SI inválido: → Error

  🟡 Usuario → DesactivarDosFactores
  🟠 DosFactoresDesactivado
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
