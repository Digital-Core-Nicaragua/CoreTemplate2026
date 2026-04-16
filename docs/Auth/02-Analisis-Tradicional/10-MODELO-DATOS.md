# Modelo de Datos — Módulo Auth

> **Schema:** `Auth`  
> **Total tablas:** 14  
> **Fecha:** 2026-04-15

---

## Tablas Principales

| Tabla | Aggregate | Opcional |
|---|---|---|
| `Usuarios` | Usuario | No |
| `UsuarioRoles` | Usuario (entity hija) | No |
| `UsuarioSucursales` | Usuario (entity hija) | EnableBranches |
| `TokensRestablecimiento` | Usuario (entity hija) | No |
| `CodigosRecuperacion2FA` | Usuario (entity hija) | No |
| `Sesiones` | Sesion | No |
| `Roles` | Rol | No |
| `RolPermisos` | Rol (entity hija) | No |
| `Permisos` | Permiso | No |
| `Sucursales` | Sucursal | EnableBranches |
| `AsignacionesRol` | AsignacionRol | EnableBranches |
| `Acciones` | Accion | UseActionCatalog |
| `ConfiguracionesTenant` | ConfiguracionTenant | No |
| `RegistrosAuditoria` | RegistroAuditoria | No |

---

## Tabla: Usuarios

```sql
CREATE TABLE Auth.Usuarios (
    Id              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    TenantId        UNIQUEIDENTIFIER    NULL,
    Email           NVARCHAR(200)       NOT NULL,
    Nombre          NVARCHAR(100)       NOT NULL,
    PasswordHash    NVARCHAR(500)       NOT NULL,
    TipoUsuario     NVARCHAR(MAX)       NOT NULL,  -- Humano | Sistema | Integracion
    Estado          NVARCHAR(MAX)       NOT NULL,  -- Pendiente | Activo | Inactivo | Bloqueado
    IntentosFallidos INT               NOT NULL DEFAULT 0,
    BloqueadoHasta  DATETIME2           NULL,
    TwoFactorActivo BIT                 NOT NULL DEFAULT 0,
    TwoFactorSecretKey NVARCHAR(500)    NULL,
    UltimoAcceso    DATETIME2           NULL,
    CreadoEn        DATETIME2           NOT NULL,
    ModificadoEn    DATETIME2           NULL
);

CREATE UNIQUE INDEX IX_Usuarios_Email ON Auth.Usuarios (Email);
CREATE INDEX IX_Usuarios_TenantId ON Auth.Usuarios (TenantId);
```

## Tabla: Sesiones

```sql
CREATE TABLE Auth.Sesiones (
    Id                  UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    UsuarioId           UNIQUEIDENTIFIER    NOT NULL,
    TenantId            UNIQUEIDENTIFIER    NULL,
    RefreshTokenHash    NVARCHAR(64)        NOT NULL,  -- SHA256 hex
    Canal               NVARCHAR(MAX)       NOT NULL,  -- Web | Mobile | Api | Desktop
    Dispositivo         NVARCHAR(200)       NOT NULL DEFAULT '',
    Ip                  NVARCHAR(50)        NOT NULL,
    UserAgent           NVARCHAR(500)       NOT NULL DEFAULT '',
    UltimaActividad     DATETIME2           NOT NULL,
    ExpiraEn            DATETIME2           NOT NULL,
    CreadoEn            DATETIME2           NOT NULL,
    EsActiva            BIT                 NOT NULL DEFAULT 1
);

CREATE UNIQUE INDEX IX_Sesiones_RefreshTokenHash ON Auth.Sesiones (RefreshTokenHash);
CREATE INDEX IX_Sesiones_UsuarioId_EsActiva ON Auth.Sesiones (UsuarioId, EsActiva);
```

## Tabla: Roles

```sql
CREATE TABLE Auth.Roles (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    TenantId    UNIQUEIDENTIFIER    NULL,
    Nombre      NVARCHAR(100)       NOT NULL,
    Descripcion NVARCHAR(MAX)       NOT NULL DEFAULT '',
    EsSistema   BIT                 NOT NULL DEFAULT 0,
    CreadoEn    DATETIME2           NOT NULL
);
```

## Tabla: Permisos

```sql
CREATE TABLE Auth.Permisos (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    Codigo      NVARCHAR(200)       NOT NULL,  -- Modulo.Recurso.Accion
    Nombre      NVARCHAR(200)       NOT NULL,
    Descripcion NVARCHAR(500)       NOT NULL DEFAULT '',
    Modulo      NVARCHAR(100)       NOT NULL,
    CreadoEn    DATETIME2           NOT NULL
);
```

## Tabla: RolPermisos

```sql
CREATE TABLE Auth.RolPermisos (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    RolId       UNIQUEIDENTIFIER    NOT NULL REFERENCES Auth.Roles(Id),
    PermisoId   UNIQUEIDENTIFIER    NOT NULL REFERENCES Auth.Permisos(Id)
);

CREATE UNIQUE INDEX IX_RolPermisos_RolId_PermisoId ON Auth.RolPermisos (RolId, PermisoId);
```

## Tabla: UsuarioRoles

```sql
CREATE TABLE Auth.UsuarioRoles (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    UsuarioId   UNIQUEIDENTIFIER    NOT NULL REFERENCES Auth.Usuarios(Id) ON DELETE CASCADE,
    RolId       UNIQUEIDENTIFIER    NOT NULL,
    AsignadoEn  DATETIME2           NOT NULL
);

CREATE UNIQUE INDEX IX_UsuarioRoles_UsuarioId_RolId ON Auth.UsuarioRoles (UsuarioId, RolId);
```

## Tabla: Sucursales (EnableBranches)

```sql
CREATE TABLE Auth.Sucursales (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    TenantId    UNIQUEIDENTIFIER    NULL,
    Codigo      NVARCHAR(20)        NOT NULL,
    Nombre      NVARCHAR(100)       NOT NULL,
    EsActiva    BIT                 NOT NULL DEFAULT 1,
    CreadoEn    DATETIME2           NOT NULL
);

CREATE UNIQUE INDEX IX_Sucursales_TenantId_Codigo ON Auth.Sucursales (TenantId, Codigo);
```

## Tabla: UsuarioSucursales (EnableBranches)

```sql
CREATE TABLE Auth.UsuarioSucursales (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    UsuarioId   UNIQUEIDENTIFIER    NOT NULL REFERENCES Auth.Usuarios(Id) ON DELETE CASCADE,
    SucursalId  UNIQUEIDENTIFIER    NOT NULL,
    EsPrincipal BIT                 NOT NULL DEFAULT 0,
    AsignadoEn  DATETIME2           NOT NULL
);

CREATE UNIQUE INDEX IX_UsuarioSucursales_UsuarioId_SucursalId
    ON Auth.UsuarioSucursales (UsuarioId, SucursalId);
```

## Tabla: AsignacionesRol (EnableBranches)

```sql
CREATE TABLE Auth.AsignacionesRol (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    UsuarioId   UNIQUEIDENTIFIER    NOT NULL,
    SucursalId  UNIQUEIDENTIFIER    NOT NULL,
    RolId       UNIQUEIDENTIFIER    NOT NULL,
    AsignadoEn  DATETIME2           NOT NULL
);

CREATE UNIQUE INDEX IX_AsignacionesRol_UsuarioId_SucursalId_RolId
    ON Auth.AsignacionesRol (UsuarioId, SucursalId, RolId);
CREATE INDEX IX_AsignacionesRol_UsuarioId_SucursalId
    ON Auth.AsignacionesRol (UsuarioId, SucursalId);
```

## Tabla: Acciones (UseActionCatalog)

```sql
CREATE TABLE Auth.Acciones (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    Codigo      NVARCHAR(100)       NOT NULL,
    Nombre      NVARCHAR(100)       NOT NULL,
    Modulo      NVARCHAR(50)        NOT NULL,
    Descripcion NVARCHAR(500)       NOT NULL DEFAULT '',
    EsActiva    BIT                 NOT NULL DEFAULT 1,
    CreadoEn    DATETIME2           NOT NULL
);

CREATE UNIQUE INDEX IX_Acciones_Codigo ON Auth.Acciones (Codigo);
CREATE INDEX IX_Acciones_Modulo ON Auth.Acciones (Modulo);
```

## Tabla: ConfiguracionesTenant

```sql
CREATE TABLE Auth.ConfiguracionesTenant (
    Id                      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    TenantId                UNIQUEIDENTIFIER    NOT NULL,
    MaxSesionesSimultaneas  INT                 NULL,
    ModificadoEn            DATETIME2           NOT NULL
);

CREATE UNIQUE INDEX IX_ConfiguracionesTenant_TenantId
    ON Auth.ConfiguracionesTenant (TenantId);
```

## Tabla: RegistrosAuditoria

```sql
CREATE TABLE Auth.RegistrosAuditoria (
    Id          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    TenantId    UNIQUEIDENTIFIER    NULL,
    UsuarioId   UNIQUEIDENTIFIER    NULL,
    Email       NVARCHAR(200)       NOT NULL DEFAULT '',
    Evento      NVARCHAR(MAX)       NOT NULL,
    Ip          NVARCHAR(50)        NOT NULL DEFAULT '',
    UserAgent   NVARCHAR(500)       NOT NULL DEFAULT '',
    Detalle     NVARCHAR(MAX)       NULL,
    CreadoEn    DATETIME2           NOT NULL
);

CREATE INDEX IX_RegistrosAuditoria_UsuarioId ON Auth.RegistrosAuditoria (UsuarioId);
CREATE INDEX IX_RegistrosAuditoria_CreadoEn ON Auth.RegistrosAuditoria (CreadoEn);
```

---

## Migraciones EF Core

| Migración | Descripción |
|---|---|
| `InitialCreate_Auth` | Tablas base: Usuarios, Roles, Permisos, UsuarioRoles, RolPermisos, TokensRestablecimiento, CodigosRecuperacion2FA, RegistrosAuditoria |
| `Add_Sesiones_TipoUsuario` | Tabla Sesiones (reemplaza RefreshTokens), columna TipoUsuario en Usuarios |
| `Add_Sucursales` | Tablas Sucursales y UsuarioSucursales |
| `Add_AsignacionesRol` | Tabla AsignacionesRol |
| `Add_CatalogoAcciones` | Tabla Acciones |
| `Add_ConfiguracionTenant` | Tabla ConfiguracionesTenant |

---

**Fecha:** 2026-04-15
