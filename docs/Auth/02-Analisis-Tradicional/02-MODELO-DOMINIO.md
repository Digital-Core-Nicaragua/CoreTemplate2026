# Modelo de Dominio — Módulo Auth

> **Basado en:** Design Level Event Storming — 7 Aggregates identificados  
> **Fecha:** 2026-04-15

---

## Bounded Contexts

### 1. Identity & Access Management (IAM)
**Responsabilidad:** Autenticación, gestión de identidad y sesiones  
**Aggregates:** Usuario, Sesion

### 2. Authorization
**Responsabilidad:** Control de acceso, roles, permisos y acciones  
**Aggregates:** Rol, AsignacionRol, Accion, Sucursal

### 3. Configuration
**Responsabilidad:** Configuración por tenant  
**Aggregates:** ConfiguracionTenant

---

## AGGREGATE 1: Usuario

**Bounded Context:** IAM  
**Aggregate Root:** Usuario  
**Invariantes:** 18

### Entidades

#### Usuario (Root)
```csharp
public sealed class Usuario : AggregateRoot<Guid>
{
    public Guid? TenantId { get; private set; }
    public Email Email { get; private set; }
    public string Nombre { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public TipoUsuario TipoUsuario { get; private set; }
    public EstadoUsuario Estado { get; private set; }
    public int IntentosFallidos { get; private set; }
    public DateTime? BloqueadoHasta { get; private set; }
    public bool TwoFactorActivo { get; private set; }
    public string? TwoFactorSecretKey { get; private set; }
    public DateTime? UltimoAcceso { get; private set; }
    public DateTime CreadoEn { get; private set; }

    // Colecciones
    public IReadOnlyList<UsuarioRol> Roles { get; }
    public IReadOnlyList<UsuarioSucursal> Sucursales { get; }
    public IReadOnlyList<TokenRestablecimiento> TokensRestablecimiento { get; }
    public IReadOnlyList<CodigoRecuperacion2FA> CodigosRecuperacion { get; }

    // Métodos
    public static Result<Usuario> Crear(Email, string, PasswordHash, Guid?, TipoUsuario);
    public Result Activar();
    public Result Desactivar();
    public Result Bloquear(DateTime hasta);
    public Result Desbloquear();
    public void IncrementarIntentosFallidos(int max, int minutos);
    public void ResetearIntentosFallidos();
    public Result CambiarPassword(PasswordHash nuevoHash);
    public void RegistrarAcceso();
    public Result AsignarRol(Guid rolId);
    public Result QuitarRol(Guid rolId);
    public Result AsignarSucursal(Guid sucursalId);
    public Result RemoverSucursal(Guid sucursalId);
    public Result CambiarSucursalPrincipal(Guid sucursalId);
    public Result ActivarDosFactores(string secretKey, IEnumerable<string> codigosHash);
    public void GuardarSecretKeyTemporal(string secretKey);
    public Result DesactivarDosFactores();
    public bool UsarCodigoRecuperacion(string codigoHash);
    public bool PuedeAutenticarse();
}
```

### Value Objects
- **Email** — Validación de formato, normalización a minúsculas
- **PasswordHash** — Hash BCrypt, nunca texto plano

### Enumeraciones
```csharp
public enum TipoUsuario { Humano = 1, Sistema = 2, Integracion = 3 }
public enum EstadoUsuario { Pendiente, Activo, Inactivo, Bloqueado }
```

### Invariantes Clave
1. Email único por tenant
2. Nombre requerido, máximo 100 caracteres
3. Usuario bloqueado no puede autenticarse
4. Usuario inactivo/pendiente no puede autenticarse
5. Después de N intentos fallidos → Bloqueo automático
6. Solo una sucursal puede ser principal
7. Usuario debe tener al menos una sucursal (si EnableBranches)
8. Usuario debe tener al menos un rol
9. SuperAdmin no puede ser desactivado
10. Permisos del SuperAdmin no modificables
11. `Sistema` e `Integracion` no tienen límite de sesiones
12. `Sistema` e `Integracion` no requieren 2FA
13. Bloqueo con fecha pasada → desbloqueo automático en `PuedeAutenticarse()`
14. Token de restablecimiento de un solo uso
15. Códigos de recuperación 2FA de un solo uso
16. No duplicar mismo rol asignado
17. No duplicar misma sucursal asignada
18. Al remover sucursal principal → asignar siguiente como principal

### Eventos de Dominio
- `UsuarioRegistradoEvent`
- `UsuarioActivadoEvent` / `UsuarioDesactivadoEvent`
- `UsuarioBloqueadoEvent` / `UsuarioDesbloqueadoEvent`
- `PasswordCambiadoEvent`
- `RestablecimientoSolicitadoEvent`
- `DosFactoresActivadoEvent` / `DosFactoresDesactivadoEvent`
- `SucursalAsignadaEvent` / `SucursalRemovidaEvent`

---

## AGGREGATE 2: Sesion

**Bounded Context:** IAM  
**Aggregate Root:** Sesion  
**Invariantes:** 10

### Entidades

#### Sesion (Root)
```csharp
public sealed class Sesion : AggregateRoot<Guid>
{
    public Guid UsuarioId { get; private set; }
    public Guid? TenantId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public CanalAcceso Canal { get; private set; }
    public string Dispositivo { get; private set; }
    public string Ip { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime UltimaActividad { get; private set; }
    public DateTime ExpiraEn { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public bool EsActiva { get; private set; }
    public bool EsValida => EsActiva && DateTime.UtcNow < ExpiraEn;

    public static Sesion Crear(Guid, Guid?, string, DateTime, CanalAcceso, string, string, string);
    public void Renovar(string nuevoHash, DateTime nuevaExpiracion);
    public void Revocar();
}
```

### Enumeraciones
```csharp
public enum CanalAcceso { Web = 1, Mobile = 2, Api = 3, Desktop = 4 }
```

### Invariantes Clave
1. RefreshToken almacenado como hash SHA256 (nunca texto plano)
2. Sesión expirada no es válida aunque esté activa
3. Sesión revocada no puede reactivarse
4. RefreshToken de un solo uso (rotación en cada renovación)
5. Límite de sesiones simultáneas configurable (Global → Tenant → Default 5)
6. `Sistema` e `Integracion` no tienen límite de sesiones
7. Al revocar: AccessToken va a blacklist si `EnableTokenBlacklist = true`
8. Al cambiar contraseña: revocar todas las sesiones
9. Al restablecer contraseña: revocar todas las sesiones
10. Sesión más antigua se cierra automáticamente si `CerrarMasAntigua`

### Eventos de Dominio
- `SesionRevocadaEvent`
- `TodasSesionesRevocadasEvent`

---

## AGGREGATE 3: Rol

**Bounded Context:** Authorization  
**Aggregate Root:** Rol  
**Invariantes:** 6

### Entidades

#### Rol (Root)
```csharp
public sealed class Rol : AggregateRoot<Guid>
{
    public Guid? TenantId { get; private set; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public bool EsSistema { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public IReadOnlyList<RolPermiso> Permisos { get; }

    public static Result<Rol> Crear(string, string, bool, Guid?);
    public Result Actualizar(string, string);
    public Result AgregarPermiso(Guid permisoId);
    public Result QuitarPermiso(Guid permisoId);
    public bool PuedeEliminarse();
}
```

#### RolPermiso (Entity)
```csharp
public sealed class RolPermiso : Entity<Guid>
{
    public Guid RolId { get; private set; }
    public Guid PermisoId { get; private set; }
}
```

### Invariantes Clave
1. Nombre único por tenant
2. Roles de sistema (`EsSistema = true`) no pueden eliminarse
3. No duplicar mismo permiso en el mismo rol
4. Rol con usuarios asignados no puede eliminarse
5. Nombre máximo 100 caracteres
6. Roles iniciales: `SuperAdmin`, `Admin`, `User`

### Eventos de Dominio
- `RolCreadoEvent`
- `RolActualizadoEvent`
- `PermisoAgregadoARolEvent`
- `PermisoQuitadoDeRolEvent`

---

## AGGREGATE 4: AsignacionRol

**Bounded Context:** Authorization  
**Aggregate Root:** AsignacionRol  
**Invariantes:** 5  
**Requiere:** `OrganizationSettings:EnableBranches = true`

### Entidades

#### AsignacionRol (Root)
```csharp
public sealed class AsignacionRol : AggregateRoot<Guid>
{
    public Guid UsuarioId { get; private set; }
    public Guid SucursalId { get; private set; }
    public Guid RolId { get; private set; }
    public DateTime AsignadoEn { get; private set; }

    public static Result<AsignacionRol> Crear(Guid, Guid, Guid);
}
```

### Invariantes Clave
1. Combinación `UsuarioId + SucursalId + RolId` debe ser única (índice BD)
2. Usuario debe tener la sucursal asignada antes de asignar rol
3. Sucursal debe estar activa
4. Rol debe existir
5. La validación de unicidad se aplica en el handler antes de crear

---

## AGGREGATE 5: Accion

**Bounded Context:** Authorization  
**Aggregate Root:** Accion  
**Invariantes:** 4  
**Requiere:** `AuthSettings:UseActionCatalog = true`

### Entidades

#### Accion (Root)
```csharp
public sealed class Accion : AggregateRoot<Guid>
{
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public string Modulo { get; private set; }
    public string Descripcion { get; private set; }
    public bool EsActiva { get; private set; }
    public DateTime CreadoEn { get; private set; }

    public static Result<Accion> Crear(string, string, string, string);
    public Result Activar();
    public Result Desactivar();
}
```

### Invariantes Clave
1. Código único en formato `Modulo.Recurso.Accion`
2. Código debe contener al menos un punto
3. Nombre requerido, máximo 100 caracteres
4. Módulo requerido, máximo 50 caracteres

### Eventos de Dominio
- (Acciones no generan eventos de dominio en la implementación actual)

---

## AGGREGATE 6: Sucursal

**Bounded Context:** Organization (relacionado con Authorization)  
**Aggregate Root:** Sucursal  
**Invariantes:** 5  
**Requiere:** `OrganizationSettings:EnableBranches = true`

### Entidades

#### Sucursal (Root)
```csharp
public sealed class Sucursal : AggregateRoot<Guid>
{
    public Guid? TenantId { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public bool EsActiva { get; private set; }
    public DateTime CreadoEn { get; private set; }

    public static Result<Sucursal> Crear(string, string, Guid?);
    public Result Activar();
    public Result Desactivar();
}
```

#### UsuarioSucursal (Entity — hija de Usuario)
```csharp
public sealed class UsuarioSucursal : Entity<Guid>
{
    public Guid UsuarioId { get; private set; }
    public Guid SucursalId { get; private set; }
    public bool EsPrincipal { get; private set; }
    public DateTime AsignadoEn { get; private set; }
}
```

### Invariantes Clave
1. Código único por tenant, convertido a mayúsculas
2. Código máximo 20 caracteres
3. Nombre máximo 100 caracteres
4. Primera sucursal asignada a usuario es automáticamente principal
5. No se puede remover la única sucursal de un usuario

---

## AGGREGATE 7: ConfiguracionTenant

**Bounded Context:** Configuration  
**Aggregate Root:** ConfiguracionTenant (Entity, no AggregateRoot)  
**Invariantes:** 3  
**Requiere:** `TenantSettings:EnableSessionLimitsPerTenant = true`

### Entidades

#### ConfiguracionTenant (Entity)
```csharp
public sealed class ConfiguracionTenant : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public int? MaxSesionesSimultaneas { get; private set; }
    public DateTime ModificadoEn { get; private set; }

    public static ConfiguracionTenant Crear(Guid, int?);
    public void ActualizarLimiteSesiones(int?);
}
```

### Invariantes Clave
1. Un solo registro por TenantId (índice único)
2. `MaxSesionesSimultaneas` debe ser > 0 si se especifica
3. Jerarquía: Tenant > Global (`AuthSettings:MaxSesionesSimultaneas`) > Default (5)

---

## Domain Services

### ISesionService
```csharp
public interface ISesionService
{
    Task<bool> VerificarYAplicarLimiteAsync(
        Guid usuarioId, TipoUsuario tipoUsuario, CancellationToken ct);
}
```
Implementa la jerarquía de límites: Tenant → Global → Default(5).

### ITokenBlacklistService
```csharp
public interface ITokenBlacklistService
{
    Task AgregarAsync(string jti, TimeSpan ttl, CancellationToken ct);
    Task<bool> EstaEnBlacklistAsync(string jti, CancellationToken ct);
}
```
Implementaciones: `InMemoryTokenBlacklistService`, `RedisTokenBlacklistService`.

---

## Repositorios

```csharp
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Usuario?> GetByEmailAsync(string email, Guid? tenantId, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string email, Guid? tenantId, CancellationToken ct);
    Task<Usuario?> GetByTokenRestablecimientoAsync(string token, CancellationToken ct);
    Task<PagedResult<Usuario>> GetPagedAsync(int pagina, int tamanoPagina, EstadoUsuario? estado, CancellationToken ct);
    Task AddAsync(Usuario usuario, CancellationToken ct);
    Task UpdateAsync(Usuario usuario, CancellationToken ct);
}

public interface ISesionRepository
{
    Task<Sesion?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Sesion?> GetActivaByRefreshTokenHashAsync(string hash, CancellationToken ct);
    Task<List<Sesion>> GetActivasByUsuarioAsync(Guid usuarioId, CancellationToken ct);
    Task<int> ContarActivasAsync(Guid usuarioId, CancellationToken ct);
    Task<Sesion?> GetMasAntiguaActivaAsync(Guid usuarioId, CancellationToken ct);
    Task AddAsync(Sesion sesion, CancellationToken ct);
    Task UpdateAsync(Sesion sesion, CancellationToken ct);
    Task RevocarTodasAsync(Guid usuarioId, CancellationToken ct);
    Task LimpiarExpiradosAsync(int diasAntiguedad, CancellationToken ct);
}

public interface IRolRepository { ... }
public interface IPermisoRepository { ... }
public interface ISucursalRepository { ... }       // Solo si EnableBranches
public interface IAsignacionRolRepository { ... }  // Solo si EnableBranches
public interface IAccionRepository { ... }         // Solo si UseActionCatalog
public interface IConfiguracionTenantRepository { ... }
public interface IRegistroAuditoriaRepository { ... }
```

---

## Resumen

| Métrica | Valor |
|---|---|
| Aggregates | 7 |
| Bounded Contexts | 3 |
| Value Objects | 2 (Email, PasswordHash) |
| Enumeraciones | 4 (TipoUsuario, CanalAcceso, EstadoUsuario, AccionAlLlegarLimiteSesiones) |
| Domain Services | 2 (ISesionService, ITokenBlacklistService) |
| Repositorios | 9 (2 opcionales) |
| Invariantes totales | 51+ |
| Eventos de dominio | 20 |

---

**Fecha:** 2026-04-15  
**Estado:** ✅ Implementado
