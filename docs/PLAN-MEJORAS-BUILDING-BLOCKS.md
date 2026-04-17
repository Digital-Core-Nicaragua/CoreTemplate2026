# Plan de Mejoras — Building Blocks Transversales

**Estado**: Pendiente de implementación  
**Prioridad**: Alta  
**Impacto**: Afecta toda la arquitectura — aplicar antes de crear nuevos módulos

---

## Contexto

Durante la revisión de arquitectura contra el proyecto de referencia **LunaERP**, se identificaron las siguientes brechas en CoreTemplate:

1. `ICurrentUser`, `ICurrentTenant`, `ICurrentBranch` viven en `Infrastructure` — deben moverse
2. `DateTime.UtcNow` directo en el dominio — no testeable
3. Sin building block de **Auditoría** dedicado
4. Sin building block de **Logging estructurado** con correlación
5. Sin building block de **Monitoring / Health Checks**
6. `DependencyInjection.cs` no existe en las capas Api de cada módulo

---

## Estructura objetivo

```
src/BuildingBlocks/
  CoreTemplate.SharedKernel           → (existente, se expande con Abstractions/)
  CoreTemplate.Api.Common             → (existente, sin cambios)
  CoreTemplate.Infrastructure         → (existente, se limpia — implementaciones de SharedKernel.Abstractions)
  CoreTemplate.Auditing               → NUEVO — building block de auditoría
  CoreTemplate.Logging                → NUEVO — building block de logging
  CoreTemplate.Monitoring             → NUEVO — building block de health/metrics
```

---

## Fase 1 — Mover contratos a SharedKernel (sin cambios de infraestructura)

**Objetivo**: Mover los contratos de contexto fuera de Infrastructure, al lugar correcto según el patrón de LunaERP.

**Decisión de diseño**: Las interfaces `ICurrentUser`, `ICurrentTenant`, `ICurrentBranch` e `IDateTimeProvider` van en `CoreTemplate.SharedKernel/Abstractions/` — igual que LunaERP nueva que tiene `IDateTimeProvider` e `ITenantContext` en `SharedKernel/Abstractions/`. No se crea un proyecto `Abstractions` separado.

**Qué hacer**:
- Crear carpeta `CoreTemplate.SharedKernel/Abstractions/`
- Mover a esta carpeta:
  - `ICurrentUser` → `CoreTemplate.SharedKernel.Abstractions`
  - `ICurrentTenant` → `CoreTemplate.SharedKernel.Abstractions`
  - `ICurrentBranch` → `CoreTemplate.SharedKernel.Abstractions`
  - Agregar `IDateTimeProvider` → `CoreTemplate.SharedKernel.Abstractions`
- Las implementaciones permanecen en `CoreTemplate.Infrastructure`
- Actualizar referencias: todos los módulos de `Application` referencian `SharedKernel` (ya lo hacen)
- Eliminar los archivos originales de `CoreTemplate.Infrastructure/Services/`

**Archivos a crear en SharedKernel**:
```
CoreTemplate.SharedKernel/
  Abstractions/
    ICurrentUser.cs
    ICurrentTenant.cs
    ICurrentBranch.cs
    IDateTimeProvider.cs
```

**Regla de dependencia — sin cambios** (SharedKernel ya era dependencia de todos):
```
Domain      → SharedKernel
Application → Domain + SharedKernel
Infrastructure → Application + Domain + SharedKernel
Api         → Application + SharedKernel + Api.Common
```

> Nota: `CoreTemplate.Application.Abstractions` puede crearse como proyecto vacío en el futuro para contratos específicos de Application (servicios externos, etc.). No es prioritario ahora.

---

## Fase 2 — CoreTemplate.Auditing

**Objetivo**: Registro automático de cambios en entidades y acciones de usuario.

**Componentes**:
```
CoreTemplate.Auditing/
  Abstractions/
    IAuditService.cs         → contrato para registrar auditoría
    IAuditContext.cs         → quién hizo qué (userId, tenantId, ip, userAgent)
  Models/
    AuditLog.cs              → entidad de auditoría (no aggregate — es infraestructura)
    AuditActionType.cs       → enum: Created, Updated, Deleted, Login, Logout, etc.
  Interceptors/
    AuditSaveChangesInterceptor.cs  → EF Core interceptor — captura automáticamente cambios
  Persistence/
    AuditDbContextExtensions.cs    → configura tabla AuditLogs en cualquier DbContext
  Services/
    AuditService.cs          → implementación que persiste en BD
  DependencyInjection.cs
  CoreTemplate.Auditing.csproj
```

**Diseño del modelo**:
```csharp
public class AuditLog
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? UsuarioId { get; init; }
    public string NombreEntidad { get; init; }   // "Usuario", "CatalogoItem"
    public string EntidadId { get; init; }        // Guid del aggregate
    public AuditActionType Accion { get; init; }
    public string? ValoresAnteriores { get; init; }  // JSON
    public string? ValoresNuevos { get; init; }      // JSON
    public DateTime OcurridoEn { get; init; }
    public string? DireccionIp { get; init; }
    public string? UserAgent { get; init; }
    public string? CorrelationId { get; init; }
}
```

**Integración con EF Core**:  
El interceptor `AuditSaveChangesInterceptor` se registra en `BaseDbContext` y captura automáticamente `Added`, `Modified` y `Deleted` antes de cada `SaveChangesAsync`.

**Tabla**: `Shared.AuditLogs` — schema propio para no contaminar los módulos.

---

## Fase 3 — CoreTemplate.Logging

**Objetivo**: Logging estructurado con correlación por request.

**Componentes**:
```
CoreTemplate.Logging/
  Abstractions/
    IAppLogger.cs            → wrapper de ILogger con métodos tipados
    ICorrelationContext.cs   → CorrelationId, TenantId, UserId por request
  Middleware/
    CorrelationMiddleware.cs → genera/propaga X-Correlation-Id en cada request
  Services/
    CorrelationContext.cs    → implementación scoped de ICorrelationContext
  Configuration/
    LoggingExtensions.cs     → AddCoreLogging() — configura Serilog + enrichers
    LoggingOptions.cs        → opciones: nivel mínimo, sinks, campos extra
  DependencyInjection.cs
  CoreTemplate.Logging.csproj
```

**Diseño de IAppLogger**:
```csharp
public interface IAppLogger
{
    void Info(string mensaje, params object[] args);
    void Warning(string mensaje, params object[] args);
    void Error(Exception ex, string mensaje, params object[] args);
    void Debug(string mensaje, params object[] args);
    IAppLogger ForContext<T>();  // equivalente a Log.ForContext<T>()
}
```

**CorrelationId**:  
- Si el request trae header `X-Correlation-Id` → se reutiliza
- Si no → se genera un `Guid.NewGuid()`
- Se incluye automáticamente en todos los logs del request
- Se retorna en la respuesta HTTP como `X-Correlation-Id`

**Enrichers de Serilog** que se agregan automáticamente:
- `CorrelationId`
- `TenantId`
- `UserId`
- `MachineName`
- `Environment`

---

## Fase 4 — CoreTemplate.Monitoring

**Objetivo**: Health checks para DB, Redis y dependencias externas + endpoint `/health`.

**Componentes**:
```
CoreTemplate.Monitoring/
  Abstractions/
    IHealthCheckService.cs   → abstracción sobre IHealthCheck de ASP.NET
  HealthChecks/
    DatabaseHealthCheck.cs   → verifica conectividad con SQL Server / PostgreSQL
    RedisHealthCheck.cs      → verifica conectividad con Redis (si está habilitado)
  Configuration/
    MonitoringExtensions.cs  → AddCoreMonitoring() — registra todos los health checks
    HealthCheckOptions.cs    → timeouts, etiquetas, endpoints
  DependencyInjection.cs
  CoreTemplate.Monitoring.csproj
```

**Endpoints expuestos**:
```
GET /health          → resumen (Healthy / Degraded / Unhealthy)
GET /health/detail   → detalle por componente (solo en Development/Staging)
GET /health/ready    → para Kubernetes readiness probe
GET /health/live     → para Kubernetes liveness probe
```

**Health checks incluidos**:
| Check | Etiqueta | Descripción |
|---|---|---|
| SQL Server | `db` | `CanConnectAsync()` al DbContext principal |
| PostgreSQL | `db` | Alternativa cuando Provider = PostgreSQL |
| Redis | `cache` | `PingAsync()` (solo si `EnableTokenBlacklist: true` y Provider = Redis) |

---

## Fase 5 — DependencyInjection en capas Api de módulos

**Objetivo**: Encapsular el registro de cada módulo — `Program.cs` solo llama `AddAuthModule()`.

**Patrón a aplicar en cada módulo Api**:
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
```

**Program.cs resultante**:
```csharp
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddCatalogosModule(builder.Configuration);
```

---

## Orden de implementación recomendado

```
Fase 1 → Mover contratos a SharedKernel/Abstractions  (fundación — sin crear proyectos nuevos)
Fase 5 → DI en módulos Api                            (bajo riesgo, mejora inmediata)
Fase 3 → CoreTemplate.Logging                         (antes que Auditing — Auditing lo usa)
Fase 2 → CoreTemplate.Auditing                        (requiere Logging)
Fase 4 → CoreTemplate.Monitoring                      (independiente, se puede hacer en paralelo)
```

---

## Dependencias entre nuevos building blocks

```
SharedKernel (+ Abstractions/)
    ↑           ↑
Logging      Monitoring
    ↑
Auditing
    ↑
Infrastructure (implementaciones de SharedKernel.Abstractions)
```

---

## Paquetes NuGet a agregar en Directory.Packages.props

```xml
<!-- Health Checks -->
<PackageVersion Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="10.0.5" />
<PackageVersion Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="10.0.5" />
<PackageVersion Include="AspNetCore.HealthChecks.Redis" Version="9.0.0" />

<!-- Logging enrichers -->
<PackageVersion Include="Serilog.Enrichers.Environment" Version="3.0.0" />
<PackageVersion Include="Serilog.Enrichers.Process" Version="3.0.0" />
<PackageVersion Include="Serilog.Enrichers.Thread" Version="4.0.0" />
```

---

## Cambios en archivos existentes

| Archivo | Cambio |
|---|---|
| `CoreTemplate.Infrastructure/Services/ICurrentUser.cs` | Mover a `SharedKernel/Abstractions/`, eliminar original |
| `CoreTemplate.Infrastructure/Services/ICurrentTenant.cs` | Ídem |
| `CoreTemplate.Infrastructure/Services/ICurrentBranch.cs` | Ídem |
| `CoreTemplate.Infrastructure/DependencyInjection.cs` | Registrar `IDateTimeProvider` y actualizar namespaces |
| `BaseDbContext.cs` | Registrar `AuditSaveChangesInterceptor` |
| `Program.cs` | Agregar `UseCorrelationMiddleware()`, `MapHealthChecks()` |
| `ARQUITECTURA.md` | Actualizar diagrama — ya actualizado |
| `CONVENCIONES.md` | Actualizar reglas — ya actualizado |
| `Directory.Packages.props` | Agregar nuevos paquetes |

---

## Métricas de éxito

- [ ] Ningún módulo de `Application` referencia `CoreTemplate.Infrastructure`
- [ ] Cero ocurrencias de `DateTime.UtcNow` en proyectos de `Domain`
- [ ] `dotnet test` pasa con 0 fallos tras todos los cambios
- [ ] `/health` retorna 200 con todos los checks en `Healthy`
- [ ] Todos los requests tienen `X-Correlation-Id` en respuesta
- [ ] `AuditLogs` se populan al crear/modificar/eliminar entidades

---

*Documento creado como guía de implementación. Actualizar estado de cada fase al completar.*
