# Arquitectura del Sistema — Estado Actual

> **Última actualización:** 2026-04-22
> **Versión:** 2.0 — incluye Email, Storage, Archivos, EmailTemplates, PdfTemplates

---

## Diagrama general

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         HOST — CoreTemplate.Api                         │
│                    Program.cs · appsettings · Swagger                   │
└──────────────────────────────┬──────────────────────────────────────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
┌───────▼──────┐    ┌──────────▼──────────┐    ┌─────▼──────────────────┐
│   MÓDULOS    │    │   BUILDING BLOCKS   │    │  MÓDULOS TRANSVERSALES │
│  de negocio  │    │   (infraestructura) │    │  (servicios comunes)   │
└──────────────┘    └─────────────────────┘    └────────────────────────┘
```

---

## Building Blocks (infraestructura pura)

```
src/BuildingBlocks/
│
├── CoreTemplate.SharedKernel        Result<T>, PagedResult, AggregateRoot,
│   └── Abstractions/                Entity, ValueObject, IDomainEvent
│       ICurrentUser, ICurrentTenant ICurrentBranch, IDateTimeProvider
│
├── CoreTemplate.Infrastructure      BaseDbContext (multi-tenant), IHasTenant,
│                                    TenantMiddleware, implementaciones de Abstractions
│
├── CoreTemplate.Api.Common          ApiResponse<T>, BaseApiController,
│                                    GlobalExceptionHandler, ValidationBehavior
│
├── CoreTemplate.Auditing            IAuditService, AuditLog, AuditSaveChangesInterceptor
│                                    AuditDbContext (schema: Shared)
│
├── CoreTemplate.Logging             IAppLogger, ICorrelationContext,
│                                    CorrelationMiddleware, Serilog config
│
├── CoreTemplate.Monitoring          Health checks (DB, Redis), /health endpoints
│
├── CoreTemplate.Email               IEmailSender, EmailMessage, EmailResult
│   └── Providers/                   MailjetEmailSender, SmtpEmailSender, SendGridEmailSender
│                                    Cambiar proveedor = cambiar appsettings
│
├── CoreTemplate.Storage             IStorageService, SubirArchivoRequest, StorageResult
│   └── Providers/                   LocalStorageService, S3StorageService, FirebaseStorageService
│                                    Cambiar proveedor = cambiar appsettings
│
└── CoreTemplate.Pdf                 IPdfDocumentTemplate, IPdfContent, IPdfGenerator
    └── Templates/                   VerticalEstandarTemplate, HorizontalEstandarTemplate,
                                     CompactoTemplate, ModernoTemplate
                                     Agregar diseño = nueva clase + registrar en DI
```

---

## Módulos implementados

```
src/Modules/
│
├── Auth/                            Autenticación enterprise-grade
│   ├── Domain/                      Usuario, Sesion, Rol, Permiso, Sucursal,
│   │                                AsignacionRol, Accion, ConfiguracionTenant,
│   │                                UsuarioCliente (Portal Clientes)
│   ├── Application/                 Login, 2FA, Refresh, Logout, Registro,
│   │                                Roles, Permisos, Sesiones, Sucursales,
│   │                                Portal Clientes, OAuth (Google, Facebook)
│   ├── Infrastructure/              AuthDbContext (schema: Auth), JWT, BCrypt,
│   │                                TOTP, TokenBlacklist (InMemory/Redis)
│   └── Api/                         /api/auth, /api/usuarios, /api/roles,
│                                    /api/perfil, /api/sucursales, /api/acciones,
│                                    /api/tenants, /api/portal
│
├── Catalogos/                       Patrón de referencia para nuevos catálogos
│   └── ...                          CatalogoItem — copiar y renombrar
│
├── EmailTemplates/                  Plantillas de correo editables en BD
│   ├── Domain/                      EmailTemplate (IHasTenant)
│   ├── Application/                 IEmailTemplateSender, ITemplateRenderer,
│   │                                Handlers de eventos de Auth
│   ├── Infrastructure/              EmailTemplatesDbContext (schema: EmailTemplates),
│   │                                FallbackTemplateLoader, Templates HTML base
│   └── Api/                         /api/email-templates
│
├── Archivos/                        Metadatos de archivos almacenados
│   ├── Domain/                      ArchivoAdjunto (IHasTenant)
│   ├── Application/                 SubirArchivoCommand, GetArchivoUrlQuery
│   ├── Infrastructure/              ArchivosDbContext (schema: Archivos)
│   └── Api/                         /api/archivos
│
└── PdfTemplates/                    Plantillas PDF con QuestPDF
    ├── Domain/                      PdfPlantilla (IHasTenant)
    ├── Application/                 IModuloPdfGenerator, Preview
    ├── Infrastructure/              PdfTemplatesDbContext (schema: PdfTemplates),
    │                                ModuloPdfGenerator, PdfTemplateFactory
    └── Api/                         /api/pdf-templates
```

---

## Módulos pendientes de implementar

```
src/Modules/
│
├── Notificaciones/                  Notificaciones en tiempo real (SignalR)
│   └── docs/Notificaciones/README.md
│
├── Configuracion/                   Parámetros del sistema editables en BD
│   └── docs/Configuracion/README.md
│
└── [Módulos de negocio del sistema] RRHH, Nómina, Contabilidad, etc.
    └── Usar Catalogos como patrón de referencia
```

---

## Flujo de dependencias entre módulos

```
Módulo de Negocio (RRHH, Nómina, Contabilidad)
    │
    ├── IEmailTemplateSender  → EmailTemplates → CoreTemplate.Email
    ├── IModuloPdfGenerator   → PdfTemplates   → CoreTemplate.Pdf (QuestPDF)
    ├── IStorageService       → Archivos       → CoreTemplate.Storage
    ├── INotificationSender   → Notificaciones → CoreTemplate.Notifications (SignalR)
    └── IConfiguracionService → Configuracion  → BD
```

---

## Schemas de base de datos

| Schema | Módulo | Tablas principales |
|---|---|---|
| `Auth` | Auth | Usuarios, Roles, Permisos, Sesiones, Sucursales, Acciones, ConfiguracionesTenant, UsuariosCliente |
| `Catalogos` | Catalogos | CatalogoItems |
| `EmailTemplates` | EmailTemplates | Plantillas |
| `Archivos` | Archivos | Archivos |
| `PdfTemplates` | PdfTemplates | Plantillas |
| `Shared` | Auditing | AuditLogs |
| `Notificaciones` | Notificaciones (pendiente) | Notificaciones |
| `Configuracion` | Configuracion (pendiente) | Items |

---

## Configuración en appsettings.json

```json
{
  "DatabaseSettings":        { "Provider", "ConnectionString" },
  "TenantSettings":          { "IsMultiTenant", "TenantResolutionStrategy" },
  "AuthSettings":            { "JwtSecretKey", "2FA", "Sesiones", "TokenBlacklist" },
  "LockoutSettings":         { "MaxFailedAttempts", "LockoutDurationMinutes" },
  "PasswordPolicy":          { "MinLength", "RequireUppercase", ... },
  "TokenBlacklistSettings":  { "Provider": "InMemory|Redis" },
  "OrganizationSettings":    { "EnableBranches" },
  "CustomerPortalSettings":  { "EnableCustomerPortal", "OAuth" },
  "AppSettings":             { "Nombre", "Url", "LogoUrl" },
  "EmailSettings":           { "Provider": "Mailjet|Smtp|SendGrid" },
  "MailjetSettings":         { "ApiKey", "SecretKey", "FromEmail", "FromName" },
  "SmtpSettings":            { "Host", "Port", "UseSsl", "Username", "Password" },
  "SendGridSettings":        { "ApiKey", "FromEmail", "FromName" },
  "EmailTemplateSettings":   { "Handlers": { ... } },
  "StorageSettings":         { "Provider": "Local|S3|Firebase", "MaxTamanioMB" },
  "LocalStorageSettings":    { "BasePath", "RequestPath" },
  "S3Settings":              { "BucketName", "Region", "AccessKey", "SecretKey" },
  "FirebaseSettings":        { "ProjectId", "Bucket", "ServiceAccountKeyPath" }
}
```

---

## Patrones arquitectónicos usados

| Patrón | Dónde |
|---|---|
| Clean Architecture | Todos los módulos (Domain → Application → Infrastructure → Api) |
| DDD | Aggregates, Value Objects, Domain Events, Repositories |
| CQRS | MediatR — Commands y Queries separados |
| Multi-tenant | BaseDbContext + IHasTenant + QueryFilter automático |
| Building Blocks | Email, Storage, Pdf, Logging, Auditing, Monitoring |
| Provider Pattern | Email (Mailjet/SMTP/SendGrid), Storage (Local/S3/Firebase) |
| Fallback Pattern | EmailTemplates (BD → archivo), PdfTemplates (tenant → global) |
| Event-Driven | IDomainEvent + INotificationHandler (MediatR) |

---

**Fecha:** 2026-04-22
