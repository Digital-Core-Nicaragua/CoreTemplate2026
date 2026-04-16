# Guía de Aggregates — Módulo Auth

> **Fecha:** 2026-04-15

---

## Cómo leer un Aggregate en CoreTemplate

Todos los aggregates siguen el mismo patrón:

```csharp
public sealed class NombreAggregate : AggregateRoot<Guid>
{
    // 1. Propiedades privadas (solo setters privados)
    public string Propiedad { get; private set; }

    // 2. Colecciones como backing fields
    private readonly List<EntidadHija> _items = [];
    public IReadOnlyList<EntidadHija> Items => _items.AsReadOnly();

    // 3. Constructor privado (EF Core lo necesita)
    private NombreAggregate() { }

    // 4. Factory method estático (único punto de creación)
    public static Result<NombreAggregate> Crear(...)
    {
        // Validaciones → retornar Result.Failure si falla
        // Crear instancia → retornar Result.Success
        // Disparar evento de dominio
    }

    // 5. Métodos de negocio (retornan Result)
    public Result Accion()
    {
        // Verificar invariante
        // Cambiar estado
        // Disparar evento si aplica
        return Result.Success();
    }
}
```

---

## Aggregate: Usuario

### Puntos clave de implementación

**Factory method con TipoUsuario:**
```csharp
public static Result<Usuario> Crear(
    Email email, string nombre, PasswordHash passwordHash,
    Guid? tenantId = null, TipoUsuario tipoUsuario = TipoUsuario.Humano)
```

**PuedeAutenticarse() — desbloqueo automático:**
```csharp
public bool PuedeAutenticarse()
{
    if (Estado == EstadoUsuario.Bloqueado && BloqueadoHasta.HasValue)
    {
        if (DateTime.UtcNow >= BloqueadoHasta.Value)
        {
            Estado = EstadoUsuario.Activo;
            BloqueadoHasta = null;
            IntentosFallidos = 0;
        }
    }
    return Estado == EstadoUsuario.Activo;
}
```

**IncrementarIntentosFallidos — solo Humano:**
```csharp
// En LoginCommandHandler:
if (usuario.TipoUsuario == TipoUsuario.Humano)
{
    usuario.IncrementarIntentosFallidos(lockout.MaxFailedAttempts, lockout.LockoutDurationMinutes);
}
```

**CambiarPassword — sin revocar sesiones en el aggregate:**
```csharp
// El aggregate solo cambia el hash y dispara el evento
public Result CambiarPassword(PasswordHash nuevoHash)
{
    PasswordHash = nuevoHash;
    ModificadoEn = DateTime.UtcNow;
    RaiseDomainEvent(new PasswordCambiadoEvent(Id, Email.Valor));
    return Result.Success();
}
// La revocación de sesiones la hace el handler:
await _sesionRepo.RevocarTodasAsync(usuario.Id, ct);
```

---

## Aggregate: Sesion

### Puntos clave de implementación

**RefreshToken como hash SHA256:**
```csharp
// En LoginCommandHandler y RefreshTokenCommandHandler:
private static string ComputarHash(string valor)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

// Al crear sesión:
var refreshToken = _jwtService.GenerarRefreshToken(); // texto plano
var refreshTokenHash = ComputarHash(refreshToken);    // hash para BD
var sesion = Sesion.Crear(..., refreshTokenHash, ...);

// Al buscar sesión:
var hash = ComputarHash(cmd.RefreshToken);
var sesion = await _sesionRepo.GetActivaByRefreshTokenHashAsync(hash, ct);
```

**EsValida como computed property:**
```csharp
public bool EsValida => EsActiva && DateTime.UtcNow < ExpiraEn;
```

---

## Aggregate: Rol

### Puntos clave de implementación

**Roles de sistema no eliminables:**
```csharp
public bool PuedeEliminarse() => !EsSistema;

// En EliminarRolCommandHandler:
if (!rol.PuedeEliminarse())
    return Result.Failure(AuthErrorMessages.RolEsSistema);
if (await _rolRepo.TieneUsuariosAsync(cmd.RolId, ct))
    return Result.Failure(AuthErrorMessages.RolTieneUsuarios);
```

**Sincronización de permisos en ActualizarRol:**
```csharp
// Quitar los que no están en la nueva lista
foreach (var permiso in rol.Permisos.ToList())
    if (!cmd.PermisoIds.Contains(permiso.PermisoId))
        rol.QuitarPermiso(permiso.PermisoId);

// Agregar los nuevos
foreach (var permisoId in cmd.PermisoIds)
    if (!rol.Permisos.Any(p => p.PermisoId == permisoId))
        rol.AgregarPermiso(permisoId);
```

---

## Aggregate: Sucursal

### Puntos clave de implementación

**Código siempre en MAYÚSCULAS:**
```csharp
public static Result<Sucursal> Crear(string codigo, string nombre, Guid? tenantId = null)
{
    // ...validaciones...
    return Result<Sucursal>.Success(new Sucursal
    {
        Codigo = codigo.Trim().ToUpperInvariant(), // ← siempre mayúsculas
        // ...
    });
}
```

**Primera sucursal = principal automáticamente:**
```csharp
public Result AsignarSucursal(Guid sucursalId)
{
    if (_sucursales.Any(s => s.SucursalId == sucursalId))
        return Result.Failure("El usuario ya tiene asignada esta sucursal.");

    var esPrincipal = _sucursales.Count == 0; // ← primera = principal
    _sucursales.Add(UsuarioSucursal.Crear(Id, sucursalId, esPrincipal));
    return Result.Success();
}
```

---

## Value Objects

### Email
```csharp
public sealed class Email : ValueObject
{
    public string Valor { get; private set; }

    public static Result<Email> Crear(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure("El email es requerido.");
        // Validar formato con regex
        // Normalizar a minúsculas
        return Result<Email>.Success(new Email { Valor = email.Trim().ToLowerInvariant() });
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
```

### PasswordHash
```csharp
public sealed class PasswordHash : ValueObject
{
    public string Valor { get; private set; }

    public static Result<PasswordHash> Crear(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return Result<PasswordHash>.Failure("El hash de contraseña es requerido.");
        return Result<PasswordHash>.Success(new PasswordHash { Valor = hash });
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
```

---

## Cómo agregar un nuevo Aggregate

1. Crear `NuevoAggregate.cs` en `Domain/Aggregates/`
2. Heredar de `AggregateRoot<Guid>`
3. Definir propiedades con setters privados
4. Implementar factory method estático que retorna `Result<NuevoAggregate>`
5. Agregar métodos de negocio que retornan `Result`
6. Disparar domain events con `RaiseDomainEvent()`
7. Crear `INuevoAggregateRepository` en `Domain/Repositories/AuthRepositories.cs`
8. Implementar `NuevoAggregateRepository` en `Infrastructure/Repositories/`
9. Agregar `DbSet<NuevoAggregate>` en `AuthDbContext`
10. Crear `NuevoAggregateConfiguration` en `Infrastructure/Persistence/Configurations/`
11. Crear migración: `dotnet ef migrations add Add_NuevoAggregate ...`

---

**Fecha:** 2026-04-15
