# Event Storming — Big Picture

> **Módulo:** Auth  
> **Nivel:** Big Picture  
> **Fecha:** 2026-04-15  
> **Eventos identificados:** 65+  
> **Comandos:** 40+  
> **Bounded Contexts:** 3  
> **Hotspots:** 8

---

## Actores del Sistema

| Actor | Tipo | Descripción |
|---|---|---|
| 👤 **Usuario Humano** | Humano | Persona que accede al sistema |
| 👤 **Administrador** | Humano | Gestiona usuarios, roles y configuración |
| 🤖 **Sistema** | Automático | Procesos internos (expiración, limpieza) |
| 🤖 **Scheduler** | Automático | Tareas programadas |

---

## BOUNDED CONTEXT 1: IAM (Identity & Access Management)

### Flujo: Registro y Activación

```
👤 Administrador → 🔵 RegistrarUsuario
    🟡 Usuario → Validar email único, política contraseña
    🟠 UsuarioRegistrado
    🟣 POLÍTICA: Asignar rol User por defecto
    🟠 RolAsignado

👤 Administrador → 🔵 ActivarUsuario
    🟡 Usuario → Estado: Pendiente → Activo
    🟠 UsuarioActivado
```

### Flujo: Login Normal

```
👤 Usuario → 🔵 IniciarSesion { email, password, canal }
    🟡 Usuario → Verificar estado (Activo?)
    🟡 Usuario → Verificar credenciales
    
    3a. SI credenciales incorrectas:
        🟠 CredencialesInvalidas
        🟡 Usuario → IncrementarIntentosFallidos
        🟣 POLÍTICA: Si IntentosFallidos >= MaxIntentos → BloquearCuenta
        🟠 UsuarioBloqueado (si aplica)
        → Fin
    
    3b. SI usuario bloqueado:
        🟠 LoginBloqueado
        → Fin
    
    3c. SI credenciales correctas:
        🟡 Usuario → ResetearIntentosFallidos
        🟡 Usuario → RegistrarAcceso
        🟠 CredencialesValidadas
        
        4a. SI TwoFactorActivo (solo Humano):
            🟠 DosFactoresRequerido
            🔵 GenerarTokenTemporal2FA
            🟠 TokenTemporal2FAGenerado
            → Esperar verificación TOTP
        
        4b. SI sin 2FA:
            🔵 VerificarLimiteSesiones
            🟣 POLÍTICA: Si sesiones >= límite → CerrarMasAntigua o BloquearNuevoLogin
            🔵 CrearSesion { canal, ip, userAgent, dispositivo }
            🟠 SesionCreada
            🔵 GenerarAccessToken
            🟠 AccessTokenGenerado
            → Retornar { accessToken, refreshToken, usuario }
```

### Flujo: Login con 2FA

```
👤 Usuario → 🔵 VerificarCodigo2FA { tokenTemporal, codigo }
    🟡 Usuario → ValidarTokenTemporal
    🟡 Usuario → ValidarCodigoTOTP
    
    2a. SI código inválido:
        🟠 CodigoTOTPInvalido
        → Fin
    
    2b. SI código válido:
        🟠 DosFactoresVerificado
        🔵 CrearSesion
        🟠 SesionCreada
        🔵 GenerarAccessToken
        🟠 AccessTokenGenerado
        → Retornar { accessToken, refreshToken, usuario }
```

### Flujo: Refresh Token

```
👤 Usuario → 🔵 RefrescarToken { refreshToken }
    🟡 Sesion → Buscar por hash del token
    🟡 Sesion → Verificar EsValida (activa + no expirada)
    🟡 Usuario → Verificar PuedeAutenticarse
    
    3a. SI inválido:
        🟠 RefreshTokenInvalido
        → Fin
    
    3b. SI válido:
        🟡 Sesion → Renovar (nuevo hash, nueva expiración)
        🟠 SesionRenovada
        🔵 GenerarAccessToken
        🟠 AccessTokenGenerado
        → Retornar { accessToken, refreshToken }
```

### Flujo: Logout

```
👤 Usuario → 🔵 CerrarSesion { refreshToken, accessToken }
    🟡 Sesion → Buscar por hash
    🟡 Sesion → Revocar
    🟠 SesionRevocada
    🟣 POLÍTICA: Si EnableTokenBlacklist → AgregarAccessTokenABlacklist
    🟠 TokenAgregadoABlacklist (si aplica)
    🟠 LogoutRegistrado
```

### Flujo: Cambio de Contraseña

```
👤 Usuario → 🔵 CambiarPassword { passwordActual, nuevoPassword }
    🟡 Usuario → Verificar password actual
    🟡 Usuario → Validar política nueva contraseña
    🟡 Usuario → CambiarPassword (nuevo hash)
    🟠 PasswordCambiado
    🟣 POLÍTICA: RevocarTodasLasSesiones
    🟡 Sesion (todas) → Revocar
    🟠 TodasSesionesRevocadas
    🟣 POLÍTICA: Si EnableTokenBlacklist → AgregarAccessTokenActualABlacklist
```

### Flujo: Restablecimiento de Contraseña

```
👤 Usuario → 🔵 SolicitarRestablecimiento { email }
    🟡 Usuario → Buscar por email
    🟣 POLÍTICA: Siempre retornar éxito (no revelar si email existe)
    🟡 Usuario → AgregarTokenRestablecimiento
    🟠 RestablecimientoSolicitado
    ⚡ → Servicio de Email (evento externo)

👤 Usuario → 🔵 RestablecerPassword { token, nuevoPassword }
    🟡 Usuario → Buscar por token válido
    🟡 Usuario → UsarTokenRestablecimiento
    🟡 Usuario → CambiarPassword
    🟠 PasswordRestablecido
    🟣 POLÍTICA: RevocarTodasLasSesiones
```

---

## BOUNDED CONTEXT 2: Authorization

### Flujo: Gestión de Roles

```
👤 Administrador → 🔵 CrearRol { nombre, descripcion, permisos }
    🟡 Rol → Verificar nombre único
    🟡 Rol → Crear con permisos
    🟠 RolCreado

👤 Administrador → 🔵 AsignarRolAUsuario { usuarioId, rolId }
    🟡 Usuario → AsignarRol
    🟣 POLÍTICA: Verificar que usuario no tenga ya el rol
    🟠 RolAsignado

👤 Administrador → 🔵 QuitarRolDeUsuario { usuarioId, rolId }
    🟡 Usuario → QuitarRol
    🟣 POLÍTICA: Usuario debe tener al menos un rol
    🟠 RolQuitado
```

### Flujo: Roles por Sucursal (EnableBranches = true)

```
👤 Administrador → 🔵 AsignarRolEnSucursal { usuarioId, sucursalId, rolId }
    🟡 AsignacionRol → Verificar unicidad (usuario+sucursal+rol)
    🟡 AsignacionRol → Verificar que usuario tiene la sucursal
    🟡 AsignacionRol → Crear
    🟠 RolAsignadoEnSucursal

👤 Administrador → 🔵 QuitarRolEnSucursal { usuarioId, sucursalId, rolId }
    🟡 AsignacionRol → Buscar y eliminar
    🟠 RolQuitadoEnSucursal
```

### Flujo: Catálogo de Acciones (UseActionCatalog = true)

```
👤 Administrador → 🔵 CrearAccion { codigo, nombre, modulo }
    🟡 Accion → Verificar código único y formato
    🟡 Accion → Crear activa
    🟠 AccionCreada

👤 Administrador → 🔵 DesactivarAccion { accionId }
    🟡 Accion → Desactivar
    🟠 AccionDesactivada
```

---

## BOUNDED CONTEXT 3: Organization + Configuration

### Flujo: Gestión de Sucursales (EnableBranches = true)

```
👤 Administrador → 🔵 CrearSucursal { codigo, nombre }
    🟡 Sucursal → Verificar código único
    🟡 Sucursal → Crear activa
    🟠 SucursalCreada

👤 Administrador → 🔵 AsignarSucursalAUsuario { usuarioId, sucursalId }
    🟡 Usuario → AsignarSucursal
    🟣 POLÍTICA: Primera sucursal → marcar como principal
    🟠 SucursalAsignada

👤 Usuario → 🔵 CambiarSucursalActiva { sucursalId }
    🟡 Usuario → CambiarSucursalPrincipal
    🟠 SucursalPrincipalCambiada
    🟣 POLÍTICA: Regenerar JWT con nuevo branch_id
```

### Flujo: Límites de Sesiones por Tenant

```
👤 Administrador → 🔵 ConfigurarLimiteSesionesTenant { tenantId, maxSesiones }
    🟡 ConfiguracionTenant → Upsert
    🟠 LimiteSesionesConfigurado

🤖 Sistema → 🔵 VerificarLimiteSesiones { usuarioId, tipoUsuario }
    🟢 ConsultarLimiteTenant → Tenant > Global > Default(5)
    🟢 ContarSesionesActivas
    
    3a. SI bajo límite:
        → Permitir nueva sesión
    
    3b. SI en límite + CerrarMasAntigua:
        🟡 Sesion (más antigua) → Revocar
        🟠 SesionMasAntiguaRevocada
        → Permitir nueva sesión
    
    3c. SI en límite + BloquearNuevoLogin:
        🟠 NuevoLoginBloqueado
        → Rechazar
```

---

## Políticas Automáticas Identificadas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Bloqueo por intentos | `IntentosFallidos >= MaxIntentos` | `BloquearCuenta` |
| P2 | Asignar rol por defecto | `UsuarioRegistrado` | `AsignarRolUser` |
| P3 | Revocar sesiones | `PasswordCambiado` | `RevocarTodasSesiones` |
| P4 | Blacklist al logout | `SesionRevocada` | `AgregarTokenABlacklist` |
| P5 | Cerrar sesión más antigua | `LimiteSesionesAlcanzado` | `RevocarSesionMasAntigua` |
| P6 | Primera sucursal = principal | `PrimeraSucursalAsignada` | `MarcarComoPrincipal` |
| P7 | Nueva principal al remover | `SucursalPrincipalRemovida` | `AsignarNuevaPrincipal` |
| P8 | Desbloqueo automático | `BloqueadoHasta < UtcNow` | `DesbloquearEnPuedeAutenticarse` |

---

## 🔴 Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Cómo invalidar tokens antes de expiración? | Token Blacklist (Redis/InMemory) |
| H2 | ¿Cómo gestionar límites de sesiones? | ISesionService con jerarquía |
| H3 | ¿Cómo almacenar refresh tokens de forma segura? | Hash SHA256, nunca texto plano |
| H4 | ¿Cómo manejar 2FA sin romper el flujo? | Token temporal de 5 min |
| H5 | ¿Cómo hacer multi-tenant configurable? | Flag IsMultiTenant + BaseDbContext |
| H6 | ¿Cómo hacer sucursales opcionales? | Flag EnableBranches + DI condicional |
| H7 | ¿Cómo hacer catálogo de acciones opcional? | Flag UseActionCatalog + DI condicional |
| H8 | ¿Cómo manejar límites por tenant? | ConfiguracionTenant + jerarquía |

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
