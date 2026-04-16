# Event Storming — Design Level Parte 3

> **Aggregates:** Sucursal, ConfiguracionTenant  
> **Bounded Contexts:** Organization, Configuration  
> **Fecha:** 2026-04-15

---

## 🟡 AGGREGATE: Sucursal

> Solo existe cuando `OrganizationSettings:EnableBranches = true`

```
┌──────────────────────────────────────────────────────────────┐
│ 🟡 SUCURSAL (Aggregate Root)                                │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│   TenantId: Guid?                                            │
│                                                              │
│ Propiedades:                                                 │
│   Codigo: string (max 20, MAYÚSCULAS, único por tenant)     │
│   Nombre: string (max 100)                                   │
│   EsActiva: bool                                             │
│   CreadoEn: DateTime                                         │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(codigo, nombre, tenantId?) → static                 │
│   Activar()                                                  │
│   Desactivar()                                               │
└──────────────────────────────────────────────────────────────┘
```

### Invariantes del Aggregate Sucursal

| # | Invariante | Implementación |
|---|---|---|
| 1 | Código único por tenant | `ISucursalRepository.ExistsByCodigo()` |
| 2 | Código convertido a MAYÚSCULAS | `Sucursal.Crear()` |
| 3 | Código máximo 20 caracteres | `Sucursal.Crear()` |
| 4 | Nombre requerido, máximo 100 caracteres | `Sucursal.Crear()` |
| 5 | Sucursal inactiva no puede asignarse a usuarios | Handler `AsignarSucursalUsuarioCommandHandler` |

### Repositorio

```csharp
ISucursalRepository:
  GetByIdAsync(Guid id)
  GetByCodigoAsync(string codigo, Guid? tenantId)
  GetAllAsync(Guid? tenantId)
  ExistsByCodigoAsync(string codigo, Guid? tenantId)
  AddAsync(Sucursal sucursal)
  UpdateAsync(Sucursal sucursal)
```

---

## 🟡 ENTITY: ConfiguracionTenant

> Solo relevante cuando `TenantSettings:EnableSessionLimitsPerTenant = true`

```
┌──────────────────────────────────────────────────────────────┐
│ ConfiguracionTenant (Entity — no AggregateRoot)             │
├──────────────────────────────────────────────────────────────┤
│ Identidad:                                                   │
│   Id: Guid                                                   │
│   TenantId: Guid (único, índice)                            │
│                                                              │
│ Propiedades:                                                 │
│   MaxSesionesSimultaneas: int? (null = usar global)         │
│   ModificadoEn: DateTime                                     │
├──────────────────────────────────────────────────────────────┤
│ Comandos que procesa:                                        │
│   Crear(tenantId, maxSesiones?) → static                    │
│   ActualizarLimiteSesiones(maxSesiones?)                    │
└──────────────────────────────────────────────────────────────┘
```

### Invariantes de ConfiguracionTenant

| # | Invariante | Implementación |
|---|---|---|
| 1 | Un solo registro por TenantId | Índice único en BD |
| 2 | MaxSesionesSimultaneas > 0 si se especifica | Handler `ConfigurarLimiteSesionesTenantCommandHandler` |
| 3 | null = usar límite global | `SesionService.ObtenerLimiteAsync()` |

### Repositorio

```csharp
IConfiguracionTenantRepository:
  GetByTenantIdAsync(Guid tenantId)
  AddAsync(ConfiguracionTenant config)
  UpdateAsync(ConfiguracionTenant config)
```

---

## Jerarquía de Límites de Sesiones

```
┌─────────────────────────────────────────────────────────────┐
│              JERARQUÍA DE LÍMITES DE SESIONES               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  NIVEL 1 (mayor prioridad):                                 │
│  ConfiguracionTenant.MaxSesionesSimultaneas                 │
│  Condición: IsMultiTenant=true                              │
│             EnableSessionLimitsPerTenant=true               │
│             TenantId != null                                │
│             config.MaxSesionesSimultaneas != null           │
│                                                             │
│  NIVEL 2:                                                   │
│  AuthSettings.MaxSesionesSimultaneas                        │
│  (default: 5)                                               │
│                                                             │
│  NIVEL 3 (menor prioridad):                                 │
│  Default del sistema: 5                                     │
│  (cuando AuthSettings no está configurado)                  │
│                                                             │
│  EXENCIÓN:                                                  │
│  TipoUsuario.Sistema | TipoUsuario.Integracion              │
│  → Sin límite, siempre permitido                            │
└─────────────────────────────────────────────────────────────┘
```

---

## Mapa Completo de Aggregates del Módulo Auth

```
┌─────────────────────────────────────────────────────────────────────┐
│                         MÓDULO AUTH                                 │
├──────────────────┬──────────────────────┬───────────────────────────┤
│       IAM        │    AUTHORIZATION      │     CONFIGURATION         │
│                  │                       │                           │
│  ┌──────────┐   │  ┌──────────────┐    │  ┌──────────────────────┐ │
│  │ Usuario  │   │  │     Rol      │    │  │ ConfiguracionTenant  │ │
│  │ (root)   │   │  │   (root)     │    │  │    (entity)          │ │
│  │          │   │  │              │    │  └──────────────────────┘ │
│  │ UsuarioRol│  │  │  RolPermiso  │    │                           │
│  │ UsuarioSuc│  │  │  (entity)    │    │                           │
│  │ TokenRest │  │  └──────────────┘    │                           │
│  │ Cod2FA   │  │                       │                           │
│  └──────────┘   │  ┌──────────────┐    │                           │
│                  │  │ AsignacionRol│    │                           │
│  ┌──────────┐   │  │   (root)     │    │                           │
│  │  Sesion  │   │  │  [opcional]  │    │                           │
│  │  (root)  │   │  └──────────────┘    │                           │
│  └──────────┘   │                       │                           │
│                  │  ┌──────────────┐    │                           │
│                  │  │    Accion    │    │                           │
│                  │  │   (root)     │    │                           │
│                  │  │  [opcional]  │    │                           │
│                  │  └──────────────┘    │                           │
│                  │                       │                           │
│                  │  ┌──────────────┐    │                           │
│                  │  │   Sucursal   │    │                           │
│                  │  │   (root)     │    │                           │
│                  │  │  [opcional]  │    │                           │
│                  │  └──────────────┘    │                           │
│                  │                       │                           │
│                  │  ┌──────────────┐    │                           │
│                  │  │   Permiso    │    │                           │
│                  │  │  (catálogo)  │    │                           │
│                  │  └──────────────┘    │                           │
└──────────────────┴──────────────────────┴───────────────────────────┘

[opcional] = Solo existe cuando el flag correspondiente está habilitado
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
