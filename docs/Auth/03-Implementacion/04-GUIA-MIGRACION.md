# Guía de Migraciones — Módulo Auth

> **Fecha:** 2026-04-15

---

## Migraciones Existentes

| # | Nombre | Qué hace |
|---|---|---|
| 1 | `InitialCreate_Auth` | Tablas base: Usuarios, Roles, Permisos, UsuarioRoles, RolPermisos, TokensRestablecimiento, CodigosRecuperacion2FA, RegistrosAuditoria |
| 2 | `Add_Sesiones_TipoUsuario` | Tabla Sesiones (reemplaza RefreshTokens), columna TipoUsuario en Usuarios |
| 3 | `Add_Sucursales` | Tablas Sucursales y UsuarioSucursales |
| 4 | `Add_AsignacionesRol` | Tabla AsignacionesRol |
| 5 | `Add_CatalogoAcciones` | Tabla Acciones |
| 6 | `Add_ConfiguracionTenant` | Tabla ConfiguracionesTenant |

---

## Comandos de Migración

### Aplicar todas las migraciones

```bash
dotnet ef database update \
  --project src/Modules/Auth/MiSistema.Modules.Auth.Infrastructure \
  --startup-project src/Host/MiSistema.Api \
  --context AuthDbContext
```

### Crear nueva migración

```bash
dotnet ef migrations add NombreMigracion \
  --project src/Modules/Auth/MiSistema.Modules.Auth.Infrastructure \
  --startup-project src/Host/MiSistema.Api \
  --context AuthDbContext
```

### Revertir última migración

```bash
dotnet ef migrations remove \
  --project src/Modules/Auth/MiSistema.Modules.Auth.Infrastructure \
  --startup-project src/Host/MiSistema.Api \
  --context AuthDbContext
```

### Ver estado de migraciones

```bash
dotnet ef migrations list \
  --project src/Modules/Auth/MiSistema.Modules.Auth.Infrastructure \
  --startup-project src/Host/MiSistema.Api \
  --context AuthDbContext
```

---

## Cuándo Crear una Nueva Migración

Crear migración cuando se modifica:
- Un aggregate (agregar/quitar propiedades)
- Una entidad hija
- Una configuración EF (`IEntityTypeConfiguration`)
- Se agrega un nuevo `DbSet<T>` al `AuthDbContext`

**No crear migración cuando se modifica:**
- Lógica de negocio en aggregates (métodos, invariantes)
- Handlers, commands, queries
- DTOs, contratos API

---

## Activar Features Opcionales

### Activar Sucursales (EnableBranches = true)

1. Cambiar `appsettings.json`:
```json
{ "OrganizationSettings": { "EnableBranches": true } }
```

2. Las migraciones `Add_Sucursales` y `Add_AsignacionesRol` ya existen — solo aplicar:
```bash
dotnet ef database update ...
```

### Activar Catálogo de Acciones (UseActionCatalog = true)

1. Cambiar `appsettings.json`:
```json
{ "AuthSettings": { "UseActionCatalog": true } }
```

2. La migración `Add_CatalogoAcciones` ya existe — solo aplicar:
```bash
dotnet ef database update ...
```

---

## Seed de Datos

El `AuthDataSeeder` se ejecuta automáticamente al arrancar en Development:

```csharp
// Extensions/ApplicationSeederExtension.cs
await AuthDataSeeder.SeedAsync(services);
```

**Qué crea el seeder:**

| Dato | Valor |
|---|---|
| Permisos | 11 permisos del sistema |
| Rol SuperAdmin | Todos los permisos, EsSistema: true |
| Rol Admin | Todos excepto Roles.Eliminar, EsSistema: true |
| Rol User | Solo *.Ver + Catalogos.Crear, EsSistema: true |
| Usuario admin | admin@coretemplate.com / Admin@1234! / Rol: SuperAdmin |

**El seeder es idempotente** — verifica si ya existen datos antes de insertar.

---

## PostgreSQL

Para usar PostgreSQL en lugar de SQL Server:

```json
{
  "DatabaseSettings": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=MiSistemaDb;Username=postgres;Password=TuPassword;"
  }
}
```

Las migraciones son compatibles con ambos motores. EF Core genera SQL específico para cada uno.

---

**Fecha:** 2026-04-15
