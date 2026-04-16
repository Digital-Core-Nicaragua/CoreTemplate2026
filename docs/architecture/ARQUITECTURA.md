# CoreTemplate — Arquitectura del Sistema

---

## 1. Estilo Arquitectónico

CoreTemplate implementa **Clean Architecture** combinada con **Domain-Driven Design (DDD)** y **CQRS** usando MediatR.

### Principios base
- Las capas internas no conocen las capas externas
- El dominio no tiene dependencias externas (ni EF Core, ni MediatR, ni nada)
- La lógica de negocio vive en el dominio, no en los handlers
- Los handlers orquestan, no deciden
- Los repositorios abstraen la persistencia

---

## 2. Capas

```
┌─────────────────────────────────────────┐
│              Api (Controllers)           │  ← Recibe HTTP, delega a MediatR
├─────────────────────────────────────────┤
│           Application (Handlers)         │  ← Orquesta dominio y repositorios
├─────────────────────────────────────────┤
│              Domain (Aggregates)         │  ← Lógica de negocio pura
├─────────────────────────────────────────┤
│          Infrastructure (EF Core)        │  ← Persistencia, repositorios
└─────────────────────────────────────────┘
         ↑ SharedKernel (transversal) ↑
```

### Reglas de dependencia
- `Domain` → solo `SharedKernel`
- `Application` → `Domain` + `SharedKernel`
- `Infrastructure` → `Application` + `Domain` + `SharedKernel`
- `Api` → `Application` + `SharedKernel` + `Api.Common`
- `Host` → todo (punto de composición)

---

## 3. Estructura de proyectos

```
BuildingBlocks/
  CoreTemplate.SharedKernel          → Result, PagedResult, AggregateRoot, Entity, ValueObject, IDomainEvent
  CoreTemplate.Api.Common            → ApiResponse, BaseApiController, GlobalExceptionHandler, ValidationBehavior
  CoreTemplate.Infrastructure        → BaseDbContext, BaseRepository, configuración EF Core base

Host/
  CoreTemplate.Api                   → Program.cs, appsettings, punto de entrada

Modules/
  Auth/
    CoreTemplate.Modules.Auth.Domain          → Usuario, Rol, Permiso, RefreshToken, eventos
    CoreTemplate.Modules.Auth.Application     → Commands, Queries, Handlers, Validators, DTOs
    CoreTemplate.Modules.Auth.Infrastructure  → DbContext, Repositorios, JWT service, Password service
    CoreTemplate.Modules.Auth.Api             → Controllers, Contracts (Request DTOs)

  Catalogos/
    CoreTemplate.Modules.Catalogos.Domain          → CatalogoItem, eventos
    CoreTemplate.Modules.Catalogos.Application     → Commands, Queries, Handlers, Validators, DTOs
    CoreTemplate.Modules.Catalogos.Infrastructure  → DbContext, Repositorios
    CoreTemplate.Modules.Catalogos.Api             → Controllers, Contracts
```

---

## 4. Patrón CQRS

Todos los casos de uso se implementan como Commands o Queries de MediatR.

### Command (escribe)
```
Controller → Command → ValidationBehavior → CommandHandler → Repository → DB
```

### Query (lee)
```
Controller → Query → QueryHandler → Repository → DB → DTO
```

### Pipeline de MediatR
1. `ValidationBehavior` — ejecuta FluentValidation antes del handler
2. Handler — lógica principal

---

## 5. Multi-tenant

### Resolución del tenant
El tenant se resuelve en el siguiente orden de prioridad:
1. Header HTTP: `X-Tenant-Id`
2. Subdominio: `tenant1.miapp.com`
3. Claim JWT: `tenant_id`

### Filtrado automático
Cuando `IsMultiTenant = true`, el `BaseDbContext` aplica un `QueryFilter` global en todas las entidades que implementan `IHasTenant`:

```csharp
// Se aplica automáticamente en todas las queries
modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _currentTenantId);
```

### Modo single-tenant
Cuando `IsMultiTenant = false`:
- No se registra el middleware de tenant
- `TenantId` en entidades se ignora (no se filtra)
- El `BaseDbContext` no aplica query filters de tenant

---

## 6. Autenticación JWT

### Flujo de login
```
POST /api/auth/login
  → Validar credenciales
  → Verificar estado de cuenta (activa, no bloqueada)
  → Si 2FA habilitado y configurado → retornar token temporal
  → Si no → generar AccessToken + RefreshToken
  → Registrar en auditoría
```

### Flujo de 2FA
```
POST /api/auth/2fa/verify
  → Validar token temporal
  → Validar código TOTP
  → Generar AccessToken + RefreshToken definitivos
```

### Flujo de refresh
```
POST /api/auth/refresh
  → Validar RefreshToken (existe, no expirado, no revocado)
  → Revocar RefreshToken anterior
  → Generar nuevo AccessToken + RefreshToken
```

---

## 7. Manejo de errores

Todos los errores retornan `ApiResponse<T>` con `success: false`.

### Errores de validación (400)
Capturados por `ValidationBehavior` antes de llegar al handler.

### Errores de negocio (400, 404, 409)
Retornados por los handlers usando `Result.Failure(...)` y mapeados en el controller.

### Errores no controlados (500)
Capturados por `GlobalExceptionHandler` — nunca expone stack traces.

### Formato estándar de respuesta
```json
{
  "success": false,
  "message": "El usuario no fue encontrado.",
  "data": null,
  "errors": ["El usuario con id X no existe."]
}
```

---

## 8. Logging

Serilog configurado con:
- Console sink (desarrollo)
- File sink (producción) — rolling diario
- Structured logging — todos los logs incluyen `RequestId`, `TenantId` (si multi-tenant), `UserId`

---

## 9. Decisiones de diseño

| Decisión | Alternativa considerada | Razón |
|---|---|---|
| MediatR para CQRS | Servicios de aplicación directos | Desacoplamiento, pipeline behaviors, testabilidad |
| FluentValidation | DataAnnotations | Validaciones complejas, reutilizables, separadas del modelo |
| Result<T> en lugar de excepciones | Excepciones de dominio | Control de flujo explícito, sin try/catch en controllers |
| Repositorios por aggregate | Generic repository | Evita métodos innecesarios, cada aggregate tiene su contrato |
| SaveChangesAsync en repositorio | Unit of Work | Simplicidad — cada operación es su propia transacción |
| File-scoped namespaces | Block namespaces | Menos indentación, más legible |
