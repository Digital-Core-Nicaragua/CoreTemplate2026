# Event Storming — Bounded Context Canvas

> **Fecha:** 2026-04-15

---

## BC 1: IAM (Identity & Access Management)

```
┌─────────────────────────────────────────────────────────────┐
│ BOUNDED CONTEXT: IAM                                        │
├─────────────────────────────────────────────────────────────┤
│ Propósito:                                                  │
│ Autenticar usuarios y gestionar su identidad y sesiones.   │
│                                                             │
│ Aggregates:                                                 │
│ - Usuario (root principal)                                  │
│ - Sesion                                                    │
│                                                             │
│ Comandos entrantes:                                         │
│ - RegistrarUsuario, ActivarUsuario, DesactivarUsuario       │
│ - IniciarSesion, CerrarSesion, RefrescarToken              │
│ - CambiarPassword, SolicitarRestablecimiento               │
│ - ActivarDosFactores, VerificarCodigo2FA                   │
│                                                             │
│ Eventos publicados:                                         │
│ - UsuarioRegistrado, UsuarioActivado, UsuarioBloqueado      │
│ - SesionCreada, SesionRevocada, PasswordCambiado           │
│ - DosFactoresActivado                                       │
│                                                             │
│ Integraciones:                                              │
│ → Authorization BC (para validar permisos)                  │
│ → Configuration BC (para límites de sesiones)              │
│ ← Token Blacklist Service (verifica tokens revocados)      │
│                                                             │
│ Lenguaje ubicuo clave:                                      │
│ Usuario, Sesion, RefreshToken, AccessToken, 2FA, TOTP      │
└─────────────────────────────────────────────────────────────┘
```

---

## BC 2: Authorization

```
┌─────────────────────────────────────────────────────────────┐
│ BOUNDED CONTEXT: Authorization                              │
├─────────────────────────────────────────────────────────────┤
│ Propósito:                                                  │
│ Controlar qué puede hacer cada usuario en el sistema.      │
│                                                             │
│ Aggregates:                                                 │
│ - Rol (con RolPermiso)                                      │
│ - Permiso (catálogo estático)                               │
│ - AsignacionRol [opcional, EnableBranches]                  │
│ - Accion [opcional, UseActionCatalog]                       │
│ - Sucursal [opcional, EnableBranches]                       │
│                                                             │
│ Comandos entrantes:                                         │
│ - CrearRol, ActualizarRol, EliminarRol                     │
│ - AsignarRolGlobal, QuitarRolGlobal                        │
│ - AsignarRolEnSucursal, QuitarRolEnSucursal                │
│ - CrearSucursal, AsignarSucursalAUsuario                   │
│ - CrearAccion, ActivarAccion, DesactivarAccion             │
│                                                             │
│ Eventos publicados:                                         │
│ - RolCreado, RolActualizado                                 │
│ - PermisoAgregadoARol, PermisoQuitadoDeRol                 │
│ - SucursalAsignada, SucursalRemovida                        │
│                                                             │
│ Integraciones:                                              │
│ ← IAM BC (Usuario necesita roles y sucursales)             │
│ → JWT (claims de roles y branch_id)                        │
│                                                             │
│ Lenguaje ubicuo clave:                                      │
│ Rol, Permiso, Sucursal, AsignacionRol, PermisosEfectivos   │
└─────────────────────────────────────────────────────────────┘
```

---

## BC 3: Configuration

```
┌─────────────────────────────────────────────────────────────┐
│ BOUNDED CONTEXT: Configuration                              │
├─────────────────────────────────────────────────────────────┤
│ Propósito:                                                  │
│ Gestionar configuración específica por tenant.             │
│                                                             │
│ Aggregates:                                                 │
│ - ConfiguracionTenant                                       │
│                                                             │
│ Comandos entrantes:                                         │
│ - ConfigurarLimiteSesionesTenant                           │
│                                                             │
│ Integraciones:                                              │
│ ← IAM BC (SesionService consulta límites)                  │
│                                                             │
│ Lenguaje ubicuo clave:                                      │
│ ConfiguracionTenant, MaxSesionesSimultaneas                │
└─────────────────────────────────────────────────────────────┘
```
