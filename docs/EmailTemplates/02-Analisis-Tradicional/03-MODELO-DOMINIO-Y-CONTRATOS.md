# EmailTemplates — Modelo de Dominio y Contratos

> **Módulo:** CoreTemplate.Modules.EmailTemplates
> **Fecha:** 2026-04-22

---

## Aggregate: EmailTemplate

```
EmailTemplate (AggregateRoot)
  + Id                  : Guid
  + TenantId            : Guid?         -- null = plantilla global del sistema
  + Codigo              : string        -- "auth.reset-password" (único, inmutable)
  + Nombre              : string        -- "Restablecimiento de contraseña"
  + Modulo              : string        -- "Auth" | "RRHH" | "Nomina" | "Sistema"
  + Asunto              : string        -- "Restablece tu contraseña en {{SistemaNombre}}"
  + CuerpoHtml          : string        -- HTML con variables {{Variable}}
  + VariablesDisponibles: string[]      -- ["NombreUsuario", "LinkReset", "ExpiraEn"]
  + UsarLayout          : bool          -- true = envolver en sistema.layout
  + EsDeSistema         : bool          -- true = no se puede eliminar
  + EsActivo            : bool
  + CreadoEn            : DateTime
  + ModificadoEn        : DateTime?
  + ModificadoPor       : Guid?

Métodos:
  + Crear(codigo, nombre, modulo, asunto, cuerpoHtml, variables) : Result<EmailTemplate>
  + Actualizar(asunto, cuerpoHtml, variables)                    : Result
  + Activar()                                                    : Result
  + Desactivar()                                                 : Result

Eventos:
  + PlantillaCreada     { templateId, codigo, modulo }
  + PlantillaActualizada { templateId, codigo, modificadoPor }
  + PlantillaActivada   { templateId, codigo }
  + PlantillaDesactivada { templateId, codigo }
```

---

## Contrato principal: IEmailTemplateSender

```
IEmailTemplateSender
  + EnviarAsync(EnviarConPlantillaRequest request) : Task<EmailResult>
```

```
EnviarConPlantillaRequest (record)
  + CodigoTemplate      : string              -- "auth.reset-password"
  + Para                : string              -- destinatario
  + NombreDestinatario  : string?
  + Variables           : Dictionary<string, string>  -- { "NombreUsuario": "Juan" }
  + Adjuntos            : EmailAdjunto[]?
  + CC                  : string[]?
```

---

## Servicio de renderizado: ITemplateRenderer

```
ITemplateRenderer
  + RenderizarAsync(plantilla: EmailTemplate, variables: Dictionary<string,string>)
      : Task<TemplateRenderResult>

TemplateRenderResult (record)
  + AsuntoRenderizado   : string
  + CuerpoRenderizado   : string   -- con layout aplicado si UsarLayout = true
```

Implementación interna — reemplaza `{{Variable}}` por el valor del diccionario.
Variables globales se inyectan automáticamente:
- `{{SistemaNombre}}` ← de `IConfiguration["AppSettings:Nombre"]`
- `{{SistemaUrl}}` ← de `IConfiguration["AppSettings:Url"]`
- `{{AnioActual}}` ← `DateTime.UtcNow.Year`
- `{{FechaActual}}` ← `IDateTimeProvider.UtcNow`

---

## Repositorio

```
IEmailTemplateRepository
  + ObtenerPorCodigoAsync(codigo, tenantId?) : Task<EmailTemplate?>
  + ObtenerPorIdAsync(id)                   : Task<EmailTemplate?>
  + ListarAsync(modulo?, soloActivos?)       : Task<IReadOnlyList<EmailTemplate>>
  + GuardarAsync(template)                  : Task
  + ExisteCodigoAsync(codigo, tenantId?)    : Task<bool>
```

---

## Modelo de datos

### Tabla: EmailTemplates.Plantillas

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | null = global |
| Codigo | nvarchar(100) | Único por tenant. Ej: "auth.reset-password" |
| Nombre | nvarchar(200) | Nombre descriptivo |
| Modulo | nvarchar(50) | "Auth", "RRHH", "Sistema" |
| Asunto | nvarchar(500) | Puede contener variables |
| CuerpoHtml | nvarchar(MAX) | HTML completo con variables |
| VariablesDisponibles | nvarchar(1000) | JSON array: ["NombreUsuario","LinkReset"] |
| UsarLayout | bit | Default: 1 |
| EsDeSistema | bit | Default: 0 |
| EsActivo | bit | Default: 1 |
| CreadoEn | datetime2 | |
| ModificadoEn | datetime2? | |
| ModificadoPor | uniqueidentifier? | UsuarioId |

**Índice único:** `(Codigo, TenantId)` — permite mismo código por tenant diferente

---

## Estructura de proyectos

```
src/BuildingBlocks/CoreTemplate.Email/
  Templates/                              ← archivos HTML de fallback
    sistema.layout.html
    auth.reset-password.html
    auth.cuenta-bloqueada.html
    auth.bienvenida.html
    auth.password-cambiado.html
    auth.2fa-activado.html
    auth.nueva-sesion.html

src/Modules/EmailTemplates/
  CoreTemplate.Modules.EmailTemplates.Domain/
    Aggregates/
      EmailTemplate.cs
    Events/
      EmailTemplateEvents.cs
    Repositories/
      IEmailTemplateRepository.cs

  CoreTemplate.Modules.EmailTemplates.Application/
    Abstractions/
      IEmailTemplateSender.cs
      ITemplateRenderer.cs
    Commands/
      CrearPlantilla/
        CrearPlantillaCommand.cs
        CrearPlantillaHandler.cs
      ActualizarPlantilla/
        ActualizarPlantillaCommand.cs
        ActualizarPlantillaHandler.cs
      ActivarPlantilla/
        ActivarPlantillaCommand.cs
      DesactivarPlantilla/
        DesactivarPlantillaCommand.cs
      EnviarPrueba/
        EnviarPruebaCommand.cs
        EnviarPruebaHandler.cs
    Queries/
      GetPlantillas/
        GetPlantillasQuery.cs
      GetPlantillaById/
        GetPlantillaByIdQuery.cs
      PreviewPlantilla/
        PreviewPlantillaQuery.cs
        PreviewPlantillaHandler.cs
    EventHandlers/                        ← handlers de eventos de Auth
      RestablecimientoSolicitadoHandler.cs
      UsuarioBloqueadoHandler.cs
      PasswordCambiadoHandler.cs
      DosFactoresActivadoHandler.cs
      UsuarioRegistradoHandler.cs
    DTOs/
      EmailTemplateDto.cs
      PreviewResultDto.cs

  CoreTemplate.Modules.EmailTemplates.Infrastructure/
    Persistence/
      EmailTemplatesDbContext.cs          ← schema: EmailTemplates
      Configurations/
        EmailTemplateConfiguration.cs
      EmailTemplatesDataSeeder.cs         ← seed de plantillas del sistema
    Repositories/
      EmailTemplateRepository.cs
    Services/
      EmailTemplateSender.cs              ← implementa IEmailTemplateSender
      TemplateRenderer.cs                 ← implementa ITemplateRenderer
      FallbackTemplateLoader.cs           ← carga archivos .html del proyecto

  CoreTemplate.Modules.EmailTemplates.Api/
    Controllers/
      EmailTemplatesController.cs
    Contracts/
      EmailTemplateContracts.cs
    DependencyInjection.cs
```

---

## Diagrama de dependencias

```
Evento Auth (RestablecimientoSolicitadoEvent)
    ↓
EmailTemplates.RestablecimientoSolicitadoHandler
    ↓
IEmailTemplateSender (EmailTemplateSender)
    ├── IEmailTemplateRepository → BD (EmailTemplates.Plantillas)
    │       ↓ si no hay en BD
    │   FallbackTemplateLoader → archivo auth.reset-password.html
    ├── ITemplateRenderer → reemplaza {{Variables}} + aplica layout
    └── IEmailSender → Mailjet / SMTP / SendGrid
```

---

## Configuración en appsettings

```json
{
  "EmailTemplateSettings": {
    "GuardarHistorial": false,
    "Handlers": {
      "RestablecimientoSolicitado": true,
      "UsuarioBloqueado": true,
      "PasswordCambiado": true,
      "DosFactoresActivado": true,
      "UsuarioRegistrado": false
    }
  },
  "AppSettings": {
    "Nombre": "Mi Sistema",
    "Url": "https://misistema.com",
    "LogoUrl": "https://misistema.com/logo.png"
  }
}
```

---

## Endpoints del módulo

| Método | Ruta | Descripción | Permiso |
|---|---|---|---|
| GET | `/api/email-templates` | Listar plantillas (filtro por módulo, estado) | `EmailTemplates.Ver` |
| GET | `/api/email-templates/{id}` | Obtener plantilla por ID | `EmailTemplates.Ver` |
| POST | `/api/email-templates` | Crear plantilla personalizada | `EmailTemplates.Gestionar` |
| PUT | `/api/email-templates/{id}` | Actualizar asunto y cuerpo | `EmailTemplates.Editar` |
| PUT | `/api/email-templates/{id}/activar` | Activar plantilla | `EmailTemplates.Gestionar` |
| PUT | `/api/email-templates/{id}/desactivar` | Desactivar plantilla | `EmailTemplates.Gestionar` |
| POST | `/api/email-templates/{id}/preview` | Vista previa renderizada | `EmailTemplates.Ver` |
| POST | `/api/email-templates/{id}/enviar-prueba` | Enviar correo de prueba | `EmailTemplates.EnviarPrueba` |

---

**Fecha:** 2026-04-22
