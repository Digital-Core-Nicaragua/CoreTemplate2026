# SAD — Diagramas de Arquitectura

> Complementa: `docs/architecture/ARQUITECTURA.md`  
> Fecha: 2026-04-15

---

## Diagrama 1: Paquetes — Dependencias entre Proyectos

Muestra qué proyecto referencia a cuál (dependencias reales de `.csproj`).

```mermaid
graph TD
    subgraph Host
        API[CoreTemplate.Api]
    end

    subgraph Modules
        subgraph Auth
            AuthApi[Auth.Api]
            AuthApp[Auth.Application]
            AuthDom[Auth.Domain]
            AuthInf[Auth.Infrastructure]
        end
        subgraph Catalogos
            CatApi[Catalogos.Api]
            CatApp[Catalogos.Application]
            CatDom[Catalogos.Domain]
            CatInf[Catalogos.Infrastructure]
        end
    end

    subgraph BuildingBlocks
        SK[SharedKernel]
        AB[Abstractions]
        AC[Api.Common]
        INF[Infrastructure]
        AUD[Auditing]
        LOG[Logging]
        MON[Monitoring]
    end

    API --> AuthApi
    API --> CatApi
    API --> INF
    API --> AUD
    API --> LOG
    API --> MON

    AuthApi --> AuthApp
    AuthApi --> AC
    AuthApp --> AuthDom
    AuthApp --> SK
    AuthApp --> AB
    AuthInf --> AuthApp
    AuthInf --> AuthDom
    AuthInf --> INF
    AuthInf --> AUD
    AuthDom --> SK

    CatApi --> CatApp
    CatApi --> AC
    CatApp --> CatDom
    CatApp --> SK
    CatApp --> AB
    CatInf --> CatApp
    CatInf --> CatDom
    CatInf --> INF
    CatDom --> SK

    INF --> SK
    INF --> AB
    AUD --> SK
    AUD --> AB
    LOG --> AB
    AC --> SK
```

---

## Diagrama 2: Componentes — Ensamblado en Program.cs

Muestra cómo se registran los módulos y building blocks en el Host.

```mermaid
graph LR
    subgraph Host["CoreTemplate.Api (Program.cs)"]
        PROG[Program.cs]
    end

    subgraph Registro["Registro de Servicios"]
        PROG --> BB[AddBuildingBlocks]
        PROG --> AUTH[AddAuthModule]
        PROG --> CAT[AddCatalogosModule]
    end

    subgraph BuildingBlocks["Building Blocks registrados"]
        BB --> SK[SharedKernel]
        BB --> INF[Infrastructure\nBaseDbContext\nTenantMiddleware]
        BB --> AUD[Auditing\nIAuditService\nInterceptor]
        BB --> LOG[Logging\nIAppLogger\nCorrelationMiddleware]
        BB --> MON[Monitoring\nHealth Checks]
    end

    subgraph AuthMod["Auth Module registrado"]
        AUTH --> AUTHDB[AuthDbContext]
        AUTH --> AUTHREP[Repositories]
        AUTH --> AUTHSVC[JwtService\nPasswordService\nTotpService\nSesionService]
        AUTH --> AUTHBL[TokenBlacklist\nInMemory / Redis]
    end

    subgraph CatMod["Catalogos Module registrado"]
        CAT --> CATDB[CatalogosDbContext]
        CAT --> CATREP[Repositories]
    end
```

---

## Diagrama 3: Componentes — Pipeline de un Request HTTP

Muestra el orden exacto del middleware y el flujo hasta la base de datos.

```mermaid
flowchart TD
    REQ[HTTP Request] --> EX[GlobalExceptionHandler]
    EX --> SW[Swagger]
    SW --> COR[CorrelationMiddleware\nX-Correlation-Id]
    COR --> TEN{IsMultiTenant?}
    TEN -- Sí --> TENM[TenantMiddleware\nX-Tenant-Id → ICurrentTenant]
    TEN -- No --> SERI
    TENM --> SERI[Serilog Request Logging]
    SERI --> HTTPS[HTTPS Redirection]
    HTTPS --> AUTHN[UseAuthentication\nValidar JWT]
    AUTHN --> BL{EnableTokenBlacklist?}
    BL -- Sí --> BLM[TokenBlacklistMiddleware\nVerificar JTI]
    BL -- No --> AUTHZ
    BLM --> AUTHZ[UseAuthorization\nRequirePermission]
    AUTHZ --> CTRL[Controller]
    CTRL --> MED[MediatR]
    MED --> VAL[ValidationBehavior\nFluentValidation]
    VAL --> HDL[Command / Query Handler]
    HDL --> REPO[Repository]
    REPO --> DB[(SQL Server\nPostgreSQL)]
    HDL --> RESP[ApiResponse T]
    RESP --> RES[HTTP Response]
```

---

## Diagrama 4: Despliegue

Muestra la infraestructura en producción.

```mermaid
graph TD
    subgraph Cliente
        WEB[Browser / App]
    end

    subgraph Servidor
        API[CoreTemplate.Api\nASP.NET Core 10\n:5001]
    end

    subgraph Persistencia
        DB[(SQL Server\no PostgreSQL)]
        REDIS[(Redis\nToken Blacklist)]
    end

    WEB -->|HTTPS| API
    API -->|EF Core| DB
    API -->|StackExchange.Redis\nOpcional| REDIS

    API -->|/health/ready| HEALTH[Health Checks]
    API -->|/health/live| HEALTH
    HEALTH --> DB
    HEALTH --> REDIS
```

---

## Diagrama 5: Reglas de Dependencia entre Capas

Muestra qué puede depender de qué (la regla de Clean Architecture).

```mermaid
flowchart LR
    DOM[Domain\nAggregates\nValueObjects\nEvents]
    APP[Application\nHandlers\nCommands\nQueries]
    INF[Infrastructure\nDbContext\nRepositories\nServices]
    API[Api\nControllers\nContracts]
    SK[SharedKernel]
    AB[Abstractions]

    DOM --> SK
    APP --> DOM
    APP --> SK
    APP --> AB
    INF --> APP
    INF --> DOM
    INF --> SK
    INF --> AB
    API --> APP
    API --> SK

    style DOM fill:#4CAF50,color:#fff
    style APP fill:#2196F3,color:#fff
    style INF fill:#FF9800,color:#fff
    style API fill:#9C27B0,color:#fff
    style SK fill:#607D8B,color:#fff
    style AB fill:#607D8B,color:#fff
```
