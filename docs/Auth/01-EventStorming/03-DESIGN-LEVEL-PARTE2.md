# Event Storming — Design Level Parte 2

> **Aggregates:** Rol, AsignacionRol, Accion  
> **Bounded Context:** Authorization  
> **Fecha:** 2026-04-15

---

## 🟡 AGGREGATE: Rol

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 ROL (Aggregate Root)                                     │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│   TenantId: Guid? (null = rol global del sistema)           │
│                                                              │
│ Propiedades:                                                 │
│   Nombre: string (max 100, único por tenant)                │
│   Descripcion: string                                        │
│   EsSistema: bool (SuperAdmin, Admin, User)                 │
│   CreadoEn: DateTime                                         │
│                                                              │
│ Colecciones:                                                 │
│   _permisos: List<RolPermiso>                                │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(nombre, descripcion, esSistema, tenantId?)          │
│   Actualizar(nombre, descripcion)                            │
│   AgregarPermiso(permisoId)                                  │
│   QuitarPermiso(permisoId)                                   │
│   PuedeEliminarse() → bool                                   │
├──────────────────────────────────────────────────────────────┤
│ Eventos que emite:                                           │
│   🟠 RolCreadoEvent                                          │
│   🟠 RolActualizadoEvent                                     │
│   🟠 PermisoAgregadoARolEvent                                │
│   🟠 PermisoQuitadoDeRolEvent                                │
└──────────────────────────────────────────────────────────────┘
```

### Entidad Hija: RolPermiso

```
┌─────────────────────────────────────┐
│ RolPermiso (Entity)                 │
├─────────────────────────────────────┤
│ Id: Guid                            │
│ RolId: Guid                         │
│ PermisoId: Guid                     │
│                                     │
│ Crear(rolId, permisoId) → static   │
└─────────────────────────────────────┘
```

### Aggregate: Permiso (catálogo estático)

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 PERMISO (Aggregate Root — catálogo del sistema)          │
├──────────────────────────────────────────────────────────────┤
│ Id: Guid                                                     │
│ Codigo: string (formato Modulo.Recurso.Accion)              │
│ Nombre: string                                               │
│ Descripcion: string                                          │
│ Modulo: string                                               │
│ CreadoEn: DateTime                                           │
│                                                              │
│ Crear(codigo, nombre, descripcion, modulo) → static         │
└──────────────────────────────────────────────────────────────┘
```

### Permisos del Sistema (Seed)

| Código | Nombre | Módulo |
|---|---|---|
| `Usuarios.Ver` | Ver usuarios | Usuarios |
| `Usuarios.Crear` | Crear usuarios | Usuarios |
| `Usuarios.Gestionar` | Gestionar usuarios | Usuarios |
| `Usuarios.Roles.Gestionar` | Gestionar roles de usuarios | Usuarios |
| `Roles.Ver` | Ver roles | Roles |
| `Roles.Crear` | Crear roles | Roles |
| `Roles.Editar` | Editar roles | Roles |
| `Roles.Eliminar` | Eliminar roles | Roles |
| `Catalogos.Ver` | Ver catálogos | Catalogos |
| `Catalogos.Crear` | Crear ítems | Catalogos |
| `Catalogos.Gestionar` | Gestionar catálogos | Catalogos |

### Invariantes del Aggregate Rol

| # | Invariante | Implementación |
|---|---|---|
| 1 | Nombre único por tenant | Validado en handler antes de crear |
| 2 | Roles de sistema no pueden eliminarse | `PuedeEliminarse()` |
| 3 | Roles con usuarios no pueden eliminarse | `IRolRepository.TieneUsuarios()` |
| 4 | No duplicar mismo permiso en el rol | `AgregarPermiso()` |
| 5 | Nombre máximo 100 caracteres | `Crear()` |
| 6 | Roles iniciales: SuperAdmin, Admin, User | `AuthDataSeeder` |

---

## 🟡 AGGREGATE: AsignacionRol

> Solo existe cuando `OrganizationSettings:EnableBranches = true`

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 ASIGNACION ROL (Aggregate Root)                          │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│                                                              │
│ Propiedades:                                                 │
│   UsuarioId: Guid                                            │
│   SucursalId: Guid                                           │
│   RolId: Guid                                                │
│   AsignadoEn: DateTime                                       │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(usuarioId, sucursalId, rolId) → static              │
└──────────────────────────────────────────────────────────────┘
```

### Invariantes del Aggregate AsignacionRol

| # | Invariante | Implementación |
|---|---|---|
| 1 | Combinación UsuarioId+SucursalId+RolId única | Índice único en BD + validación en handler |
| 2 | Usuario debe tener la sucursal asignada | Handler `AsignarRolSucursalCommandHandler` |
| 3 | Sucursal debe estar activa | Handler `AsignarRolSucursalCommandHandler` |
| 4 | Rol debe existir | Handler `AsignarRolSucursalCommandHandler` |
| 5 | La validación de unicidad es en el handler | `IAsignacionRolRepository.ExisteAsync()` |

### Repositorio

```csharp
IAsignacionRolRepository:
  GetByIdAsync(Guid id)
  GetByUsuarioSucursalAsync(Guid usuarioId, Guid sucursalId)
  GetByUsuarioAsync(Guid usuarioId)
  ExisteAsync(Guid usuarioId, Guid sucursalId, Guid rolId)
  AddAsync(AsignacionRol asignacion)
  DeleteAsync(AsignacionRol asignacion)
```

---

## 🟡 AGGREGATE: Accion

> Solo existe cuando `AuthSettings:UseActionCatalog = true`

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 ACCION (Aggregate Root)                                  │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│                                                              │
│ Propiedades:                                                 │
│   Codigo: string (formato Modulo.Recurso.Accion, único)     │
│   Nombre: string (max 100)                                   │
│   Modulo: string (max 50)                                    │
│   Descripcion: string (max 500)                              │
│   EsActiva: bool                                             │
│   CreadoEn: DateTime                                         │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(codigo, nombre, modulo, descripcion?) → static      │
│   Activar()                                                  │
│   Desactivar()                                               │
└──────────────────────────────────────────────────────────────┘
```

### Invariantes del Aggregate Accion

| # | Invariante | Implementación |
|---|---|---|
| 1 | Código único | `IAccionRepository.ExistsByCodigo()` |
| 2 | Código debe contener punto | `Accion.Crear()` |
| 3 | Nombre requerido, max 100 | `Accion.Crear()` |
| 4 | Módulo requerido, max 50 | `Accion.Crear()` |

### Repositorio

```csharp
IAccionRepository:
  GetByIdAsync(Guid id)
  GetByCodigoAsync(string codigo)
  GetAllAsync(string? modulo)
  ExistsByCodigoAsync(string codigo)
  AddAsync(Accion accion)
  UpdateAsync(Accion accion)
```

---

## Relación entre Aggregates de Authorization

```
┌─────────────────────────────────────────────────────────────┐
│                    AUTHORIZATION BC                         │
│                                                             │
│  Permiso ←──── RolPermiso ────→ Rol                        │
│  (catálogo)    (entity hija)   (aggregate)                  │
│                                                             │
│  Usuario ←── AsignacionRol ──→ Rol                         │
│  (IAM BC)    (aggregate)       (aggregate)                  │
│              ↑                                              │
│           Sucursal                                          │
│           (Organization BC)                                 │
│                                                             │
│  Accion (aggregate independiente, opcional)                 │
└─────────────────────────────────────────────────────────────┘
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
