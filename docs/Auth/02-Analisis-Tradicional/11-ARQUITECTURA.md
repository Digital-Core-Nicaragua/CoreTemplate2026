# Arquitectura — Módulo Auth

> **Patrón:** Clean Architecture + DDD + CQRS  
> **Fecha:** 2026-04-15

---

## Estructura de Proyectos

```
src/Modules/Auth/
├── CoreTemplate.Modules.Auth.Domain/          → Aggregates, ValueObjects, Events, Repositories (interfaces)
├── CoreTemplate.Modules.Auth.Application/     → Commands, Queries, Handlers, Validators, DTOs
├── CoreTemplate.Modules.Auth.Infrastructure/  → DbContext, Repositories, Services, Migrations
└── CoreTemplate.Modules.Auth.Api/             → Controllers, Contracts (Request DTOs)
```

---

## Capas y Responsabilidades

### Domain Layer
- Aggregates con invariantes de negocio
- Value Objects inmutables
- Domain Events
- Interfaces de repositorios (contratos)
- Sin dependencias externas

### Application Layer
- Commands y Queries (CQRS con MediatR)
- Handlers que orquestan el dominio
- Validators (FluentValidation)
- DTOs de respuesta
- Interfaces de servicios de infraestructura (`IJwtService`, `IPasswordService`, etc.)
- Sin lógica de negocio propia

### Infrastructure Layer
- Implementaciones de repositorios (EF Core)
- `AuthDbContext` con schema `Auth`
- Servicios: `JwtService`, `PasswordService`, `TotpService`, `SesionService`
- `InMemoryTokenBlacklistService`, `RedisTokenBlacklistService`
- `TokenBlacklistMiddleware`
- Migraciones EF Core
- `AuthDataSeeder`

### Api Layer
- Controllers que delegan a MediatR
- Request DTOs (Contracts)
- Sin lógica de negocio

---

## Flujo de un Request

```
HTTP Request
    ↓
TokenBlacklistMiddleware (verifica JTI en blacklist)
    ↓
UseAuthentication (valida JWT)
    ↓
UseAuthorization ([RequirePermission])
    ↓
Controller → _mediator.Send(command/query)
    ↓
ValidationBehavior (FluentValidation pipeline)
    ↓
CommandHandler / QueryHandler
    ↓
Repository → DbContext → SQL Server / PostgreSQL
    ↓
HTTP Response { success, data, errors }
```

---

## Patrones Implementados

| Patrón | Dónde | Para qué |
|---|---|---|
| **CQRS** | Application | Separar escritura (Commands) de lectura (Queries) |
| **Repository** | Domain/Infrastructure | Abstraer acceso a datos |
| **Mediator** | Application | Desacoplar handlers de controllers |
| **Result Pattern** | SharedKernel | Manejo de errores sin excepciones |
| **Domain Events** | Domain | Comunicación entre aggregates |
| **Options Pattern** | Application | Configuración tipada desde appsettings |

---

## Dependencias entre Capas

```
Api → Application → Domain
         ↓
    Infrastructure → Domain
         ↓
    BuildingBlocks (SharedKernel, Infrastructure)
```

**Regla:** Las capas internas no conocen las externas. Domain no conoce Application ni Infrastructure.

---

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core | 10 | Framework web |
| Entity Framework Core | 10 | ORM |
| MediatR | 14 | CQRS |
| FluentValidation | 12 | Validaciones |
| BCrypt.Net | 4 | Hash de contraseñas (work factor 12) |
| Otp.NET | 1.4 | 2FA TOTP |
| StackExchange.Redis | 2.8 | Token Blacklist (opcional) |
| System.IdentityModel.Tokens.Jwt | 8 | Generación/validación JWT |

---

## Configuración del Pipeline (Program.cs)

```csharp
// Orden crítico del middleware
app.UseExceptionHandler();
app.UseSwagger() / app.UseSwaggerUI();
app.UseMiddleware<TenantMiddleware>();        // Si IsMultiTenant = true
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TokenBlacklistMiddleware>(); // Si EnableTokenBlacklist = true
app.UseAuthorization();
app.MapControllers();
```

---

**Fecha:** 2026-04-15
