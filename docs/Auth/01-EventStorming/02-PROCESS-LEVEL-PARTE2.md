# Event Storming — Process Level Parte 2

> **Procesos:** Autorización, Roles, Permisos, Catálogo de Acciones  
> **Fecha:** 2026-04-15

---

## PROCESO 7: Validación de Permisos en cada Request

```
🤖 [RequirePermission("Modulo.Recurso.Accion")] Attribute

  → Obtener usuario autenticado (ICurrentUser)
  → Obtener roles del usuario (claims del JWT)
  → Para cada rol:
      🟢 IRolRepository → GetById(rolId)
      → Verificar si rol tiene permiso "Modulo.Recurso.Accion"

  SI ningún rol tiene el permiso:
    🟠 AccesoDenegado
    → Retornar HTTP 403

  SI algún rol tiene el permiso:
    → Continuar con el handler
```

---

## PROCESO 8: Permisos Efectivos (con y sin Sucursales)

```
👤 Usuario → 🔵 ObtenerPermisosEfectivos

  🟢 ICurrentBranch → BranchId (del JWT claim branch_id)

  SI EnableBranches == false O BranchId == null:
    ┌─────────────────────────────────────────┐
    │ MODO SIN SUCURSALES (roles globales)   │
    └─────────────────────────────────────────┘
    🟢 IUsuarioRepository → GetById(currentUser.Id)
    → Para cada UsuarioRol:
        🟢 IRolRepository → GetById(rolId)
        → Agregar permisos del rol al conjunto
    → Retornar lista de códigos de permisos

  SI EnableBranches == true Y BranchId != null:
    ┌─────────────────────────────────────────┐
    │ MODO CON SUCURSALES (roles por sucursal)│
    └─────────────────────────────────────────┘
    🟢 IAsignacionRolRepository → GetByUsuarioSucursal(userId, branchId)
    → Para cada AsignacionRol:
        🟢 IRolRepository → GetById(rolId)
        → Agregar permisos del rol al conjunto
    → Retornar lista de códigos de permisos
```

---

## PROCESO 9: Gestión de Roles

```
👤 Administrador → 🔵 CrearRol { nombre, descripcion, permisoIds }
  🟢 IRolRepository → ExistsByNombre(nombre, tenantId)
  SI existe:
    🟠 RolNombreYaExiste → Error

  🟡 Rol → Crear { nombre, descripcion, esSistema: false, tenantId }
  → Para cada permisoId:
      🟢 IPermisoRepository → GetById(permisoId)
      🟡 Rol → AgregarPermiso(permisoId)
  🟠 RolCreado
  → Retornar rolId

👤 Administrador → 🔵 ActualizarRol { rolId, nombre, descripcion, permisoIds }
  🟢 IRolRepository → GetById(rolId)
  SI no existe: → Error

  🟡 Rol → Actualizar { nombre, descripcion }
  → Sincronizar permisos (quitar los que no están, agregar los nuevos)
  🟠 RolActualizado

👤 Administrador → 🔵 EliminarRol { rolId }
  🟢 IRolRepository → GetById(rolId)
  SI EsSistema: → Error "Rol del sistema no puede eliminarse"
  🟢 IRolRepository → TieneUsuarios(rolId)
  SI tiene usuarios: → Error "Rol con usuarios no puede eliminarse"
  🟠 RolEliminado
```

---

## PROCESO 10: Asignación de Roles a Usuarios

```
👤 Administrador → 🔵 AsignarRolGlobal { usuarioId, rolId }
  🟢 IUsuarioRepository → GetById(usuarioId)
  🟢 IRolRepository → GetById(rolId)
  🟡 Usuario → AsignarRol(rolId)
  🟣 POLÍTICA: Verificar que no tenga ya el rol
  🟠 RolAsignado

👤 Administrador → 🔵 QuitarRolGlobal { usuarioId, rolId }
  🟢 IUsuarioRepository → GetById(usuarioId)
  🟡 Usuario → QuitarRol(rolId)
  🟣 POLÍTICA: Usuario debe tener al menos un rol
  🟠 RolQuitado
```

---

## PROCESO 11: Roles por Sucursal (EnableBranches = true)

```
👤 Administrador → 🔵 AsignarRolEnSucursal { usuarioId, sucursalId, rolId }
  🟢 IUsuarioRepository → GetById(usuarioId)
  → Verificar usuario.Sucursales.Any(s => s.SucursalId == sucursalId)
  SI no tiene la sucursal: → Error

  🟢 ISucursalRepository → GetById(sucursalId)
  SI no activa: → Error

  🟢 IRolRepository → GetById(rolId)
  SI no existe: → Error

  🟢 IAsignacionRolRepository → Existe(usuarioId, sucursalId, rolId)
  SI ya existe: → Error "Ya tiene este rol en esta sucursal"

  🟡 AsignacionRol → Crear { usuarioId, sucursalId, rolId }
  🟠 RolAsignadoEnSucursal

👤 Administrador → 🔵 QuitarRolEnSucursal { usuarioId, sucursalId, rolId }
  🟢 IAsignacionRolRepository → GetByUsuarioSucursal(usuarioId, sucursalId)
  → Buscar asignación con rolId
  SI no existe: → Error

  🟡 IAsignacionRolRepository → Delete(asignacion)
  🟠 RolQuitadoEnSucursal
```

---

## PROCESO 12: Catálogo de Acciones (UseActionCatalog = true)

```
👤 Administrador → 🔵 CrearAccion { codigo, nombre, modulo, descripcion }
  🟣 POLÍTICA: Verificar UseActionCatalog == true
  SI false: → Error "Catálogo de acciones no habilitado"

  🟢 IAccionRepository → ExistsByCodigo(codigo)
  SI existe: → Error "Código ya existe"

  🟡 Accion → Crear { codigo, nombre, modulo, descripcion }
  🟣 POLÍTICA: Código debe contener punto (formato Modulo.Recurso.Accion)
  🟠 AccionCreada

👤 Administrador → 🔵 ActivarAccion { accionId }
  🟢 IAccionRepository → GetById(accionId)
  🟡 Accion → Activar
  🟠 AccionActivada

👤 Administrador → 🔵 DesactivarAccion { accionId }
  🟢 IAccionRepository → GetById(accionId)
  🟡 Accion → Desactivar
  🟠 AccionDesactivada

👤 Administrador → 🔵 ListarAcciones { modulo? }
  🟢 IAccionRepository → GetAll(modulo)
  → Retornar lista de AccionDto
```

---

## PROCESO 13: Seed Inicial de Permisos y Roles

```
🤖 AuthDataSeeder → SeedAsync (al arrancar en Development)

  SI Permisos.Any(): → Saltar

  → Crear 11 permisos del sistema:
    Usuarios.Ver, Usuarios.Crear, Usuarios.Gestionar, Usuarios.Roles.Gestionar
    Roles.Ver, Roles.Crear, Roles.Editar, Roles.Eliminar
    Catalogos.Ver, Catalogos.Crear, Catalogos.Gestionar

  SI Roles.Any(): → Saltar

  → Crear rol SuperAdmin (todos los permisos, EsSistema: true)
  → Crear rol Admin (todos excepto Roles.Eliminar, EsSistema: true)
  → Crear rol User (solo *.Ver + Catalogos.Crear, EsSistema: true)

  SI Usuarios.Any(): → Saltar

  → Crear usuario admin@coretemplate.com
      Password: Admin@1234!
      Estado: Activo
      Rol: SuperAdmin
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
