# Modelo de Dominio — Diagramas

> Complementa: `docs/Auth/02-Analisis-Tradicional/02-MODELO-DOMINIO.md` y `10-MODELO-DATOS.md`  
> Fecha: 2026-04-15

---

## Diagramas de Clases

### Diagrama 1: Bounded Contexts

```mermaid
graph TD
    subgraph IAM["IAM — Identity & Access Management"]
        Usuario
        Sesion
    end

    subgraph Authorization
        Rol
        AsignacionRol
        Accion
    end

    subgraph Organization
        Sucursal
    end

    subgraph Configuration
        ConfiguracionTenant
    end

    Usuario -->|tiene muchas| Sesion
    Usuario -->|tiene muchos| Rol
    Usuario -->|tiene muchas| Sucursal
    Rol -->|tiene muchos| Accion
    AsignacionRol -->|une| Usuario
    AsignacionRol -->|une| Sucursal
    AsignacionRol -->|une| Rol
```

---

### Diagrama 2: Aggregate Usuario

```mermaid
classDiagram
    class Usuario {
        +Guid Id
        +Guid? TenantId
        +Email Email
        +string Nombre
        +PasswordHash PasswordHash
        +TipoUsuario TipoUsuario
        +EstadoUsuario Estado
        +int IntentosFallidos
        +DateTime? BloqueadoHasta
        +bool TwoFactorActivo
        +DateTime? UltimoAcceso
        +Crear() Result~Usuario~
        +Activar() Result
        +Desactivar() Result
        +Bloquear(hasta) Result
        +Desbloquear() Result
        +CambiarPassword(hash) Result
        +IncrementarIntentosFallidos()
        +ResetearIntentosFallidos()
        +AsignarRol(rolId) Result
        +QuitarRol(rolId) Result
        +ActivarDosFactores() Result
        +DesactivarDosFactores() Result
        +PuedeAutenticarse() bool
    }

    class UsuarioRol {
        +Guid Id
        +Guid UsuarioId
        +Guid RolId
        +DateTime AsignadoEn
    }

    class UsuarioSucursal {
        +Guid Id
        +Guid UsuarioId
        +Guid SucursalId
        +bool EsPrincipal
        +DateTime AsignadoEn
    }

    class TokenRestablecimiento {
        +Guid Id
        +string Token
        +DateTime ExpiraEn
        +bool Usado
    }

    class CodigoRecuperacion2FA {
        +Guid Id
        +string CodigoHash
        +bool Usado
    }

    class Email {
        +string Value
        +Crear(email) Result~Email~
    }

    class PasswordHash {
        +string Value
        +Crear(plain) PasswordHash
        +Verificar(plain) bool
    }

    Usuario "1" --> "*" UsuarioRol
    Usuario "1" --> "*" UsuarioSucursal
    Usuario "1" --> "*" TokenRestablecimiento
    Usuario "1" --> "*" CodigoRecuperacion2FA
    Usuario --> Email
    Usuario --> PasswordHash
```

---

### Diagrama 3: Aggregate Sesion

```mermaid
classDiagram
    class Sesion {
        +Guid Id
        +Guid UsuarioId
        +Guid? TenantId
        +string RefreshTokenHash
        +CanalAcceso Canal
        +string Dispositivo
        +string Ip
        +string UserAgent
        +DateTime UltimaActividad
        +DateTime ExpiraEn
        +bool EsActiva
        +bool EsValida
        +Crear() Sesion
        +Renovar(nuevoHash, nuevaExpiracion)
        +Revocar()
    }

    class CanalAcceso {
        <<enumeration>>
        Web
        Mobile
        Api
        Desktop
    }

    Sesion --> CanalAcceso
```

---

### Diagrama 4: Aggregate Rol

```mermaid
classDiagram
    class Rol {
        +Guid Id
        +Guid? TenantId
        +string Nombre
        +string Descripcion
        +bool EsSistema
        +DateTime CreadoEn
        +Crear() Result~Rol~
        +Actualizar(nombre, desc) Result
        +AgregarPermiso(permisoId) Result
        +QuitarPermiso(permisoId) Result
        +PuedeEliminarse() bool
    }

    class RolPermiso {
        +Guid Id
        +Guid RolId
        +Guid PermisoId
    }

    class Permiso {
        +Guid Id
        +string Codigo
        +string Nombre
        +string Modulo
        +string Descripcion
    }

    Rol "1" --> "*" RolPermiso
    RolPermiso --> Permiso
```

---

### Diagrama 5: Enumeraciones del Dominio

```mermaid
classDiagram
    class TipoUsuario {
        <<enumeration>>
        Humano = 1
        Sistema = 2
        Integracion = 3
    }

    class EstadoUsuario {
        <<enumeration>>
        Pendiente
        Activo
        Inactivo
        Bloqueado
    }

    class CanalAcceso {
        <<enumeration>>
        Web = 1
        Mobile = 2
        Api = 3
        Desktop = 4
    }

    class AccionAlLlegarLimiteSesiones {
        <<enumeration>>
        CerrarMasAntigua
        BloquearNuevoLogin
    }
```

---

### Diagrama 6: SharedKernel — Clases Base

```mermaid
classDiagram
    class AggregateRoot~TId~ {
        +TId Id
        #AddDomainEvent(event)
        +GetDomainEvents() IReadOnlyList
        +ClearDomainEvents()
    }

    class Entity~TId~ {
        +TId Id
    }

    class ValueObject {
        #GetEqualityComponents() IEnumerable
        +Equals(obj) bool
    }

    class Result~T~ {
        +bool IsSuccess
        +T Value
        +string Error
        +Success(value) Result~T~
        +Failure(error) Result~T~
    }

    class PagedResult~T~ {
        +IReadOnlyList~T~ Items
        +int TotalCount
        +int Pagina
        +int TamanoPagina
        +int TotalPaginas
    }

    AggregateRoot~TId~ --|> Entity~TId~
    Usuario --|> AggregateRoot~Guid~
    Sesion --|> AggregateRoot~Guid~
    Rol --|> AggregateRoot~Guid~
    Email --|> ValueObject
    PasswordHash --|> ValueObject
```

---

## Diagrama de Objetos

### Diagrama 7: Instancia de ejemplo — Usuario con Roles y Sucursales

```mermaid
classDiagram
    class usuario1 {
        Id = "a1b2c3..."
        Email = "juan@empresa.com"
        Nombre = "Juan Pérez"
        TipoUsuario = Humano
        Estado = Activo
        TwoFactorActivo = false
    }

    class rol_admin {
        Id = "r1..."
        Nombre = "Admin"
        EsSistema = true
    }

    class sucursal_central {
        Id = "s1..."
        Codigo = "CENTRAL"
        Nombre = "Sucursal Central"
        EsActiva = true
    }

    class usuarioRol1 {
        UsuarioId = "a1b2c3..."
        RolId = "r1..."
        AsignadoEn = 2026-01-01
    }

    class usuarioSucursal1 {
        UsuarioId = "a1b2c3..."
        SucursalId = "s1..."
        EsPrincipal = true
    }

    usuario1 --> usuarioRol1
    usuario1 --> usuarioSucursal1
    usuarioRol1 --> rol_admin
    usuarioSucursal1 --> sucursal_central
```

---

## Diagrama ER

### Diagrama 8: Modelo de Datos — Schema Auth

```mermaid
erDiagram
    Usuarios {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        nvarchar Email
        nvarchar Nombre
        nvarchar PasswordHash
        nvarchar TipoUsuario
        nvarchar Estado
        int IntentosFallidos
        datetime2 BloqueadoHasta
        bit TwoFactorActivo
        datetime2 CreadoEn
    }

    Sesiones {
        uniqueidentifier Id PK
        uniqueidentifier UsuarioId FK
        uniqueidentifier TenantId
        nvarchar RefreshTokenHash
        nvarchar Canal
        nvarchar Ip
        datetime2 ExpiraEn
        bit EsActiva
    }

    Roles {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        nvarchar Nombre
        bit EsSistema
    }

    Permisos {
        uniqueidentifier Id PK
        nvarchar Codigo
        nvarchar Nombre
        nvarchar Modulo
    }

    UsuarioRoles {
        uniqueidentifier Id PK
        uniqueidentifier UsuarioId FK
        uniqueidentifier RolId FK
    }

    RolPermisos {
        uniqueidentifier Id PK
        uniqueidentifier RolId FK
        uniqueidentifier PermisoId FK
    }

    Sucursales {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        nvarchar Codigo
        nvarchar Nombre
        bit EsActiva
    }

    UsuarioSucursales {
        uniqueidentifier Id PK
        uniqueidentifier UsuarioId FK
        uniqueidentifier SucursalId FK
        bit EsPrincipal
    }

    AsignacionesRol {
        uniqueidentifier Id PK
        uniqueidentifier UsuarioId FK
        uniqueidentifier SucursalId FK
        uniqueidentifier RolId FK
    }

    ConfiguracionesTenant {
        uniqueidentifier Id PK
        uniqueidentifier TenantId
        int MaxSesionesSimultaneas
    }

    Usuarios ||--o{ Sesiones : "tiene"
    Usuarios ||--o{ UsuarioRoles : "tiene"
    Usuarios ||--o{ UsuarioSucursales : "tiene"
    Roles ||--o{ UsuarioRoles : "asignado en"
    Roles ||--o{ RolPermisos : "tiene"
    Permisos ||--o{ RolPermisos : "asignado en"
    Sucursales ||--o{ UsuarioSucursales : "asignada en"
    Usuarios ||--o{ AsignacionesRol : "tiene"
    Sucursales ||--o{ AsignacionesRol : "tiene"
    Roles ||--o{ AsignacionesRol : "asignado en"
```
