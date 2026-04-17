# CoreTemplate — Convenciones de Código

---

## 1. Nomenclatura

### Idioma
- **Dominio y negocio**: español (`Usuario`, `Registrar`, `CreadoEn`)
- **Infraestructura técnica**: inglés (`DbContext`, `Repository`, `Handler`)
- **Nombres de archivos**: igual que la clase que contienen

### Clases
| Tipo | Convención | Ejemplo |
|---|---|---|
| Aggregate | PascalCase | `Usuario`, `CatalogoItem` |
| Command | PascalCase + Command | `LoginCommand`, `CrearUsuarioCommand` |
| Query | PascalCase + Query | `GetUsuarioByIdQuery` |
| Handler | PascalCase + Handler | `LoginCommandHandler` |
| Validator | PascalCase + Validator | `LoginCommandValidator` |
| Repository interface | I + PascalCase + Repository | `IUsuarioRepository` |
| Repository impl | PascalCase + Repository | `UsuarioRepository` |
| Controller | PascalCase + Controller | `AuthController` |
| DTO | PascalCase + Dto | `UsuarioDto`, `LoginResponseDto` |
| Request contract | PascalCase + Request | `LoginRequest`, `CrearUsuarioRequest` |
| Event | PascalCase + Event | `UsuarioRegistradoEvent` |
| Value Object | PascalCase descriptivo | `Email`, `PasswordHash` |
| Health Check | PascalCase + HealthCheck | `DatabaseHealthCheck`, `RedisHealthCheck` |
| Interceptor EF | PascalCase + Interceptor | `AuditSaveChangesInterceptor` |
| Middleware | PascalCase + Middleware | `CorrelationMiddleware` |
| Extension DI | DependencyInjection | Siempre `DependencyInjection.cs` en cada proyecto |

### Métodos
| Tipo | Convención | Ejemplo |
|---|---|---|
| Factory method dominio | PascalCase | `Crear()`, `Registrar()` |
| Métodos de dominio | PascalCase | `Activar()`, `Bloquear()` |
| Repositorios async | PascalCase + Async | `GetByIdAsync()`, `AddAsync()` |
| Handlers | Handle() | Siempre `Handle()` |

---

## 2. Estructura de archivos por capa

### Domain
```
Aggregates/
  Usuario.cs
Events/
  Auth/
    AuthEvents.cs        ← todos los eventos del módulo en un archivo
Repositories/
  IUsuarioRepository.cs
ValueObjects/
  Email.cs
Enums/
  EstadoUsuario.cs
```

### Application
```
Commands/
  Login/
    LoginCommand.cs
    LoginCommandHandler.cs
    LoginCommandValidator.cs
Queries/
  GetUsuarioById/
    GetUsuarioByIdQuery.cs
    GetUsuarioByIdQueryHandler.cs
DTOs/
  UsuarioDto.cs
  LoginResponseDto.cs
Constants/
  AuthErrorMessages.cs
  AuthSuccessMessages.cs
DependencyInjection.cs
```

### Infrastructure
```
Persistence/
  Configurations/
    UsuarioConfiguration.cs
  AuthDbContext.cs
  AuthDbContextFactory.cs
  AuthDataSeeder.cs
Repositories/
  UsuarioRepository.cs
Services/
  JwtService.cs
  PasswordService.cs
DependencyInjection.cs
```

### Api
```
Controllers/
  AuthController.cs
Contracts/
  LoginRequest.cs
  CrearUsuarioRequest.cs
DependencyInjection.cs       ← NUEVO — AddAuthModule() encapsula todo el módulo
ModuleName.Api.csproj
```

---

## 3. Patrones obligatorios

### Handlers siempre sealed + primary constructor
```csharp
internal sealed class LoginCommandHandler(
    IUsuarioRepository _usuarioRepo,
    IJwtService _jwtService) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        // ...
    }
}
```

### Repositorios incluyen SaveChangesAsync en Add y Update
```csharp
public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
{
    await _db.Usuarios.AddAsync(usuario, ct);
    await _db.SaveChangesAsync(ct);
}
```

### Controllers solo delegan a MediatR
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
{
    var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), ct);
    if (!result.IsSuccess) return UnauthorizedResponse<LoginResponseDto>(result.Errors);
    return SuccessResponse(result.Value!, result.Message);
}
```

### Aggregates usan factory methods estáticos
```csharp
// Correcto
var result = Usuario.Crear(email, nombre, passwordHash);

// Incorrecto
var usuario = new Usuario { Email = email, ... };
```

### Siempre CancellationToken en métodos async
```csharp
Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);
```

### Nunca DateTime.UtcNow en el dominio
```csharp
// Correcto — recibir el tiempo como parámetro
public static Result<Usuario> Crear(Email email, string nombre, PasswordHash hash, DateTime ahora)
{
    var usuario = new Usuario { CreadoEn = ahora };
    ...
}

// Incorrecto — dependencia oculta en el dominio
var usuario = new Usuario { CreadoEn = DateTime.UtcNow };
```

### Registro de módulo encapsulado en DependencyInjection.cs de Api
```csharp
// src/Modules/Auth/CoreTemplate.Modules.Auth.Api/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthApplication(configuration);
        services.AddAuthInfrastructure(configuration);
        return services;
    }
}

// En Program.cs:
builder.Services.AddAuthModule(builder.Configuration);
```

### Logging con IAppLogger
```csharp
// Correcto en handlers de Application
internal sealed class LoginCommandHandler(
    IAppLogger _logger, ...) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        _logger.Info("Intento de login para {Email}", cmd.Email);
        ...
    }
}
```

### Auditoría explícita en acciones sensibles
```csharp
// Para acciones que no son CRUD de entidades (login, logout, etc.)
// el handler llama explícitamente a IAuditService
await _auditService.LogAsync(new AuditLog
{
    NombreEntidad = "Sesion",
    EntidadId = sesion.Id.ToString(),
    Accion = AuditActionType.Login,
    OcurridoEn = _dateTimeProvider.UtcNow
}, ct);
```

---

## 4. Reglas de código

- **File-scoped namespaces** en todos los archivos
- **Sealed** en handlers, repositorios y controllers
- **Private setters** en todas las propiedades de aggregates y entidades
- **No lógica de negocio en handlers** — solo orquestación
- **No excepciones para control de flujo** — usar `Result<T>`
- **No `var` en tipos no obvios** — usar el tipo explícito
- **XML docs** en todas las clases y métodos públicos de SharedKernel y Api.Common
- **No `DateTime.UtcNow` en Domain** — recibir el tiempo como parámetro desde Application
- **No `ILogger<T>` en Application** — usar `IAppLogger` de `CoreTemplate.Logging`
- **No referencia a `CoreTemplate.Infrastructure` desde Application** — usar `CoreTemplate.Abstractions`

---

## 5. Configuración del proyecto

### .editorconfig
```ini
[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8-bom
trim_trailing_whitespace = true
insert_final_newline = true
csharp_style_namespace_declarations = file_scoped:warning
```

### Directory.Packages.props
Todas las versiones de NuGet centralizadas aquí. Nunca especificar versión en `.csproj`.
