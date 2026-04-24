# CoreTemplate

Plantilla base reutilizable para sistemas **ASP.NET Core 10** con **Clean Architecture + DDD + CQRS**.

Clona esta plantilla, ejecuta un script y tendrás un sistema funcional con autenticación enterprise-grade, notificaciones en tiempo real, gestión de archivos, generación de PDFs, envío de correos con plantillas editables, configuración del sistema desde la UI y auditoría completa.

---

## ¿Qué incluye?

| Módulo / Building Block | Descripción |
|---|---|
| **Auth** | Login JWT, Sesiones, Token Blacklist, 2FA TOTP, Roles, Permisos, Sucursales, Multi-tenant, Portal Clientes |
| **Configuracion** | Parámetros del sistema editables desde la UI sin redeployar (nombre empresa, moneda, series, etc.) |
| **Notificaciones** | Notificaciones en tiempo real via SignalR + historial en BD |
| **EmailTemplates** | Plantillas de correo editables en BD con fallback a archivos HTML |
| **Archivos** | Metadatos de archivos subidos, multi-tenant |
| **PdfTemplates** | Generación de PDFs con QuestPDF, diseños intercambiables, configuración corporativa por tenant |
| **Auditoria** | Consulta de logs de auditoría con filtros (quién hizo qué y cuándo) |
| **Catálogos** | Patrón de referencia para crear nuevos catálogos |
| **CoreTemplate.Email** | Building block: Mailjet, SMTP, SendGrid — cambiar proveedor sin tocar código |
| **CoreTemplate.Storage** | Building block: Local, AWS S3, Firebase — cambiar proveedor sin tocar código |
| **CoreTemplate.Pdf** | Building block: QuestPDF con 4 diseños base (vertical, horizontal, compacto, moderno) |
| **CoreTemplate.Notifications** | Building block: SignalR Hub con autenticación JWT |
| **SharedKernel** | `Result<T>`, `PagedResult<T>`, `AggregateRoot`, `Entity`, `ValueObject`, `IDomainEvent` |
| **Abstractions** | `ICurrentUser`, `ICurrentTenant`, `ICurrentBranch`, `IDateTimeProvider` |
| **Api.Common** | `ApiResponse<T>`, `BaseApiController`, `GlobalExceptionHandler`, `ValidationBehavior` |
| **Infrastructure** | `BaseDbContext` multi-tenant, `TenantMiddleware` |
| **Auditing** | `IAuditService`, `AuditLog`, `AuditSaveChangesInterceptor` |
| **Logging** | `IAppLogger`, `ICorrelationContext`, `CorrelationMiddleware` |
| **Monitoring** | Health checks para DB y Redis, endpoints `/health` |

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server o PostgreSQL
- Redis (opcional, para Token Blacklist en producción)
- PowerShell 5.1+ (para el script de renombrado)

---

## Inicio rápido

### 1. Clonar la plantilla

```bash
git clone https://github.com/tu-usuario/CoreTemplate.git MiSistema
cd MiSistema
```

### 2. Renombrar al nombre de tu sistema

```powershell
.\rename.ps1 -SystemName "MiSistema"
```

### 3. Configurar la base de datos

Edita `src/Host/MiSistema.Api/appsettings.Development.json`:

```json
{
  "DatabaseSettings": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=MiSistemaDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"
  }
}
```

### 4. Ejecutar migraciones

```bash
# Auth
dotnet ef database update --project src/Modules/Auth/MiSistema.Modules.Auth.Infrastructure --startup-project src/Host/MiSistema.Api --context AuthDbContext

# Catalogos
dotnet ef database update --project src/Modules/Catalogos/MiSistema.Modules.Catalogos.Infrastructure --startup-project src/Host/MiSistema.Api --context CatalogosDbContext

# EmailTemplates
dotnet ef database update --project src/Modules/EmailTemplates/MiSistema.Modules.EmailTemplates.Infrastructure --startup-project src/Host/MiSistema.Api --context EmailTemplatesDbContext

# Archivos
dotnet ef database update --project src/Modules/Archivos/MiSistema.Modules.Archivos.Infrastructure --startup-project src/Host/MiSistema.Api --context ArchivosDbContext

# PdfTemplates
dotnet ef database update --project src/Modules/PdfTemplates/MiSistema.Modules.PdfTemplates.Infrastructure --startup-project src/Host/MiSistema.Api --context PdfTemplatesDbContext

# Configuracion
dotnet ef database update --project src/Modules/Configuracion/MiSistema.Modules.Configuracion.Infrastructure --startup-project src/Host/MiSistema.Api --context ConfiguracionDbContext

# Notificaciones
dotnet ef database update --project src/Modules/Notificaciones/MiSistema.Modules.Notificaciones.Infrastructure --startup-project src/Host/MiSistema.Api --context NotificacionesDbContext

# Auditoria (building block — schema Shared)
dotnet ef database update --project src/BuildingBlocks/MiSistema.Auditing --startup-project src/Host/MiSistema.Api --context AuditDbContext
```

### 5. Ejecutar

```bash
cd src/Host/MiSistema.Api
dotnet run
```

Abre `https://localhost:5001/swagger`.

### 6. Primer login

Al arrancar en Development, el seeder crea automáticamente:

| Campo | Valor |
|---|---|
| Email | `admin@coretemplate.com` |
| Password | `Admin@1234!` |
| Rol | `SuperAdmin` |

```bash
POST /api/auth/login
{
  "email": "admin@coretemplate.com",
  "password": "Admin@1234!",
  "canal": 1
}
```

---

## Configuración completa

### appsettings.json

```json
{
  "DatabaseSettings": { "Provider": "SqlServer", "ConnectionString": "..." },
  "TenantSettings": { "IsMultiTenant": false },
  "AuthSettings": { "JwtSecretKey": "...", "AccessTokenExpirationMinutes": 15 },
  "EmailSettings": { "Provider": "Smtp" },
  "SmtpSettings": { "Host": "localhost", "Port": 1025 },
  "MailjetSettings": { "ApiKey": "", "SecretKey": "", "FromEmail": "", "FromName": "" },
  "StorageSettings": { "Provider": "Local", "MaxTamanioMB": 20 },
  "LocalStorageSettings": { "BasePath": "archivos", "RequestPath": "/archivos" },
  "S3Settings": { "BucketName": "", "Region": "us-east-1", "AccessKey": "", "SecretKey": "" },
  "AppSettings": { "Nombre": "Mi Sistema", "Url": "https://localhost:5001" },
  "NotificationSettings": { "Handlers": { "UsuarioBloqueado": true, "PasswordCambiado": true } }
}
```

---

## Endpoints disponibles

### Auth (`/api/auth`) — Login, 2FA, Refresh, Logout, Reset Password
### Usuarios (`/api/usuarios`) — CRUD, roles, sesiones
### Perfil (`/api/perfil`) — Mi perfil, cambiar password, mis sesiones
### Roles (`/api/roles`) — CRUD de roles
### Configuracion (`/api/configuracion`) — Parámetros del sistema
### Notificaciones (`/api/notificaciones`) — Historial, marcar leídas
### EmailTemplates (`/api/email-templates`) — CRUD, preview, enviar prueba
### Archivos (`/api/archivos`) — Subir, obtener URL, eliminar
### PdfTemplates (`/api/pdf-templates`) — CRUD, diseños disponibles, preview
### Auditoria (`/api/auditoria`) — Consulta de logs con filtros
### Catálogos (`/api/catalogos`) — Patrón de referencia

### WebSocket
| Endpoint | Descripción |
|---|---|
| `/hubs/notificaciones` | SignalR Hub — notificaciones en tiempo real |

---

## Notificaciones en tiempo real (SignalR)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notificaciones", {
        accessTokenFactory: () => localStorage.getItem("accessToken")
    })
    .withAutomaticReconnect()
    .build();

connection.on("RecibirNotificacion", (n) => {
    mostrarToast(n.titulo, n.mensaje, n.tipo);
});

await connection.start();
```

Ver guía completa: `docs/Notificaciones/03-Guias/01-MANUAL-DE-USO.md`

---

## Arquitectura

```
src/
├── BuildingBlocks/
│   ├── SharedKernel       → Result, PagedResult, AggregateRoot, Entity, ValueObject
│   ├── Abstractions       → ICurrentUser, ICurrentTenant, ICurrentBranch, IDateTimeProvider
│   ├── Api.Common         → ApiResponse, BaseApiController, GlobalExceptionHandler
│   ├── Infrastructure     → BaseDbContext, TenantMiddleware
│   ├── Auditing           → IAuditService, AuditLog, AuditSaveChangesInterceptor
│   ├── Logging            → IAppLogger, ICorrelationContext, CorrelationMiddleware
│   ├── Monitoring         → Health checks (DB, Redis), endpoints /health
│   ├── Email              → IEmailSender (Mailjet, SMTP, SendGrid)
│   ├── Storage            → IStorageService (Local, S3, Firebase)
│   ├── Pdf                → IPdfDocumentTemplate, QuestPDF, 4 diseños base
│   └── Notifications      → INotificationSender, SignalR Hub
├── Host/
│   └── MiSistema.Api      → Program.cs, appsettings, punto de entrada
└── Modules/
    ├── Auth/              → Autenticación enterprise-grade
    ├── Configuracion/     → Parámetros del sistema editables en BD
    ├── Notificaciones/    → Notificaciones en tiempo real + historial
    ├── EmailTemplates/    → Plantillas de correo editables en BD
    ├── Archivos/          → Metadatos de archivos almacenados
    ├── PdfTemplates/      → Plantillas PDF con QuestPDF
    ├── Auditoria/         → Consulta de logs de auditoría
    └── Catalogos/         → Patrón de referencia para nuevos catálogos

tests/
├── SharedKernel.Tests
├── Auth.Tests
└── Catalogos.Tests
```

---

## Schemas de base de datos

| Schema | Módulo | Tablas principales |
|---|---|---|
| `Auth` | Auth | Usuarios, Roles, Permisos, Sesiones, Sucursales, Acciones |
| `Catalogos` | Catalogos | CatalogoItems |
| `EmailTemplates` | EmailTemplates | Plantillas |
| `Archivos` | Archivos | Archivos |
| `PdfTemplates` | PdfTemplates | Plantillas |
| `Configuracion` | Configuracion | Items |
| `Notificaciones` | Notificaciones | Notificaciones |
| `Shared` | Auditing | AuditLogs |

---

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core | 10 | Framework web |
| Entity Framework Core | 10 | ORM |
| MediatR | 14 | CQRS |
| FluentValidation | 12 | Validaciones |
| BCrypt.Net | 4 | Hash de contraseñas |
| Otp.NET | 1.4 | 2FA TOTP |
| StackExchange.Redis | 2.8 | Token Blacklist (opcional) |
| SignalR | Incluido en ASP.NET Core | Notificaciones en tiempo real |
| Mailjet.Api | 4.0 | Envío de correos |
| AWSSDK.S3 | 4.0 | Almacenamiento en S3 |
| QuestPDF | 2026.2 | Generación de PDFs |
| Serilog | 9 | Logging estructurado |
| xUnit v3 | 3 | Tests unitarios |
| FluentAssertions | 8 | Assertions en tests |
| NSubstitute | 5 | Mocks en tests |
| Swashbuckle | 6.9 | Swagger/OpenAPI |

---

## Tests

```bash
dotnet test
```

---

## Licencia

MIT — libre para uso comercial y personal.
