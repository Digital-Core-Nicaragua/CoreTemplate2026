# Plan de Implementación: Configuración de Email por Tenant

> **Módulo:** CoreTemplate.Email + CoreTemplate.Modules.EmailTemplates
> **Tipo:** Mejora futura — multi-tenant avanzado
> **Prioridad:** Alta (si los tenants son empresas independientes)
> **Esfuerzo estimado:** 3-5 días de desarrollo
> **Fecha de documentación:** 2026-04-22
> **Estado:** Pendiente de implementación

---

## ¿Por qué implementar esto?

Hoy todos los correos del sistema salen desde un único remitente configurado en `appsettings.json`.
En un sistema multi-tenant real donde cada tenant es una empresa independiente, esto es un problema:

| Problema | Impacto |
|---|---|
| Todos los correos salen desde `noreply@misistema.com` | Los empleados de Empresa A reciben correos que no parecen de su empresa |
| Un tenant no puede usar su propio servidor SMTP corporativo | Empresas con políticas de TI estrictas no pueden adoptar el sistema |
| Las métricas de Mailjet/SendGrid son compartidas entre todos los tenants | Un tenant no puede ver sus propias estadísticas de entrega |
| Si un tenant tiene problemas de entregabilidad, afecta a todos | Un tenant con mala reputación de dominio perjudica a los demás |

**Caso de uso típico:**
- Empresa A → usa su servidor SMTP corporativo, correos desde `rrhh@empresa-a.com`
- Empresa B → usa Mailjet con su propia cuenta y API key
- Empresa C → pequeña, usa la configuración global del sistema (sin configuración propia)

---

## Arquitectura propuesta

### Componentes nuevos

```
src/Modules/EmailTemplates/
  CoreTemplate.Modules.EmailTemplates.Domain/
    Aggregates/
      ConfiguracionEmailTenant.cs      ← NUEVO aggregate
    Repositories/
      IConfiguracionEmailTenantRepository.cs  ← NUEVO

  CoreTemplate.Modules.EmailTemplates.Application/
    Commands/
      ConfiguracionEmailTenantCommands.cs     ← NUEVO (Crear, Actualizar, Eliminar)
    Queries/
      GetConfiguracionEmailTenantQuery.cs     ← NUEVO
    DTOs/
      ConfiguracionEmailTenantDto.cs          ← NUEVO

  CoreTemplate.Modules.EmailTemplates.Infrastructure/
    Services/
      TenantAwareEmailSender.cs               ← NUEVO (reemplaza uso directo de IEmailSender)
      EmailSenderFactory.cs                   ← NUEVO (crea sender según proveedor)
      EncryptionService.cs                    ← NUEVO (cifra/descifra credenciales)
    Repositories/
      ConfiguracionEmailTenantRepository.cs   ← NUEVO

src/BuildingBlocks/CoreTemplate.Email/
  Abstractions/
    ITenantEmailSender.cs                     ← NUEVO (contrato tenant-aware)
```

---

## Aggregate: ConfiguracionEmailTenant

```
ConfiguracionEmailTenant (AggregateRoot, IHasTenant)
  + Id                  : Guid
  + TenantId            : Guid          (requerido, no null — es por tenant)
  + Provider            : string        "Mailjet" | "Smtp" | "SendGrid"
  + FromEmail           : string        "notificaciones@empresa-a.com"
  + FromName            : string        "Empresa A"
  + EsActivo            : bool

  ─── Credenciales cifradas ──────────────────────────────────────────────────
  + ApiKeyCifrado       : string?       cifrado con AES-256
  + SecretKeyCifrado    : string?       cifrado con AES-256

  ─── Configuración SMTP ─────────────────────────────────────────────────────
  + SmtpHost            : string?
  + SmtpPort            : int?
  + SmtpUseSsl          : bool
  + SmtpUsernameCifrado : string?       cifrado con AES-256
  + SmtpPasswordCifrado : string?       cifrado con AES-256

  + CreadoEn            : DateTime
  + ModificadoEn        : DateTime?
```

**Regla de negocio importante:**
- Solo puede existir UNA configuración activa por tenant
- Si se crea una nueva, la anterior se desactiva automáticamente
- Si no existe configuración para el tenant → usar configuración global de appsettings

---

## Tabla en BD

```
Schema: EmailTemplates
Tabla: ConfiguracionEmailTenant

| Campo              | Tipo           | Descripción                              |
|--------------------|----------------|------------------------------------------|
| Id                 | uniqueidentifier | PK                                     |
| TenantId           | uniqueidentifier | FK lógica al tenant (NOT NULL)          |
| Provider           | nvarchar(20)   | "Mailjet", "Smtp", "SendGrid"            |
| FromEmail          | nvarchar(200)  | Remitente del correo                     |
| FromName           | nvarchar(200)  | Nombre del remitente                     |
| ApiKeyCifrado      | nvarchar(500)  | API Key cifrada con AES-256              |
| SecretKeyCifrado   | nvarchar(500)  | Secret Key cifrada con AES-256           |
| SmtpHost           | nvarchar(200)  | Host SMTP (solo si Provider = Smtp)      |
| SmtpPort           | int            | Puerto SMTP                              |
| SmtpUseSsl         | bit            | Usar SSL                                 |
| SmtpUsernameCifrado| nvarchar(500)  | Username SMTP cifrado                    |
| SmtpPasswordCifrado| nvarchar(500)  | Password SMTP cifrado                    |
| EsActivo           | bit            | Solo una activa por tenant               |
| CreadoEn           | datetime2      |                                          |
| ModificadoEn       | datetime2?     |                                          |

Índice único: (TenantId, EsActivo) WHERE EsActivo = 1
→ Garantiza que solo haya una configuración activa por tenant
```

---

## Cifrado de credenciales — OBLIGATORIO

Las credenciales (ApiKey, SecretKey, passwords SMTP) **NUNCA** se guardan en texto plano en BD.

### Implementación con AES-256

```csharp
public interface IEncryptionService
{
    string Cifrar(string texto);
    string Descifrar(string textoCifrado);
}

internal sealed class AesEncryptionService(IConfiguration config) : IEncryptionService
{
    // La clave de cifrado viene de configuración/secrets — NUNCA hardcodeada
    // appsettings: "EncryptionSettings:Key": "<clave-256-bits-en-base64>"
    private readonly string _key = config["EncryptionSettings:Key"]
        ?? throw new InvalidOperationException("EncryptionSettings:Key no configurado.");

    public string Cifrar(string texto)
    {
        // Implementar con System.Security.Cryptography.Aes
        // AES-256-CBC con IV aleatorio prefijado al resultado
    }

    public string Descifrar(string textoCifrado)
    {
        // Extraer IV del prefijo y descifrar
    }
}
```

**Configuración requerida en appsettings (o mejor, en variables de entorno):**
```json
{
  "EncryptionSettings": {
    "Key": "<clave-aleatoria-256-bits-en-base64>"
  }
}
```

**Generar la clave:**
```csharp
// Ejecutar una vez para generar la clave
var key = new byte[32]; // 256 bits
RandomNumberGenerator.Fill(key);
Console.WriteLine(Convert.ToBase64String(key));
```

---

## TenantAwareEmailSender — el componente clave

Este servicio reemplaza el uso directo de `IEmailSender` en `EmailTemplateSender`.
En cada envío consulta si el tenant tiene configuración propia y crea el sender apropiado.

```csharp
internal sealed class TenantAwareEmailSender(
    IConfiguracionEmailTenantRepository repo,
    ICurrentTenant currentTenant,
    IEmailSenderFactory factory,
    IEmailSender globalSender,  // fallback — el configurado en appsettings
    IAppLogger logger) : ITenantEmailSender
{
    public async Task<EmailResult> EnviarAsync(EmailMessage mensaje, CancellationToken ct)
    {
        // 1. Buscar configuración del tenant actual
        var config = await repo.ObtenerActivaAsync(currentTenant.TenantId, ct);

        // 2. Si no tiene configuración propia → usar sender global
        if (config is null)
            return await globalSender.EnviarAsync(mensaje, ct);

        // 3. Crear sender con las credenciales del tenant (descifradas en memoria)
        var tenantSender = factory.Crear(config);

        // 4. Enviar con el sender del tenant
        return await tenantSender.EnviarAsync(mensaje, ct);
    }
}
```

**Importante:** El sender del tenant se crea en cada request — no se cachea en DI porque las credenciales pueden cambiar. Para optimizar se puede agregar un cache con TTL corto (5 minutos).

---

## EmailSenderFactory — crea el sender correcto

```csharp
internal sealed class EmailSenderFactory(
    IEncryptionService encryption,
    IAppLogger logger) : IEmailSenderFactory
{
    public IEmailSender Crear(ConfiguracionEmailTenant config)
    {
        return config.Provider switch
        {
            "Mailjet" => new MailjetEmailSender(
                Options.Create(new MailjetSettings
                {
                    ApiKey = encryption.Descifrar(config.ApiKeyCifrado!),
                    SecretKey = encryption.Descifrar(config.SecretKeyCifrado!),
                    FromEmail = config.FromEmail,
                    FromName = config.FromName
                }),
                logger),

            "Smtp" => new SmtpEmailSender(
                Options.Create(new SmtpSettings
                {
                    Host = config.SmtpHost!,
                    Port = config.SmtpPort ?? 587,
                    UseSsl = config.SmtpUseSsl,
                    Username = config.SmtpUsernameCifrado is not null
                        ? encryption.Descifrar(config.SmtpUsernameCifrado) : string.Empty,
                    Password = config.SmtpPasswordCifrado is not null
                        ? encryption.Descifrar(config.SmtpPasswordCifrado) : string.Empty,
                    FromEmail = config.FromEmail,
                    FromName = config.FromName
                }),
                logger),

            _ => throw new InvalidOperationException(
                $"Proveedor '{config.Provider}' no soportado en configuración de tenant.")
        };
    }
}
```

---

## Endpoints de la API

```
Módulo: EmailTemplates
Prefijo: /api/email-config

| Método | Ruta                              | Descripción                        | Permiso                    |
|--------|-----------------------------------|------------------------------------|----------------------------|
| GET    | /api/email-config                 | Ver configuración del tenant actual| EmailConfig.Ver            |
| POST   | /api/email-config                 | Crear/actualizar configuración     | EmailConfig.Gestionar      |
| DELETE | /api/email-config                 | Eliminar configuración (usa global)| EmailConfig.Gestionar      |
| POST   | /api/email-config/probar          | Enviar correo de prueba            | EmailConfig.Gestionar      |
```

### Ejemplo de request para crear configuración Mailjet:

```json
POST /api/email-config
{
  "provider": "Mailjet",
  "fromEmail": "notificaciones@empresa-a.com",
  "fromName": "Empresa A",
  "apiKey": "3044a890ebc4fae0ebd641fd66f47b0d",
  "secretKey": "9d44abdde91126bf0a2c4cef0e712866"
}
```

### Ejemplo para SMTP corporativo:

```json
POST /api/email-config
{
  "provider": "Smtp",
  "fromEmail": "rrhh@empresa-a.com",
  "fromName": "RRHH Empresa A",
  "smtpHost": "mail.empresa-a.com",
  "smtpPort": 587,
  "smtpUseSsl": true,
  "smtpUsername": "rrhh@empresa-a.com",
  "smtpPassword": "password-del-servidor"
}
```

### Endpoint de prueba:

```json
POST /api/email-config/probar
{
  "destinatario": "admin@empresa-a.com"
}
→ Envía un correo de prueba usando la configuración del tenant actual
→ Retorna: { "exitoso": true, "messageId": "...", "proveedor": "Mailjet" }
```

---

## Cambio en EmailTemplateSender

El único cambio en código existente es en `EmailTemplateSender` — reemplazar `IEmailSender` por `ITenantEmailSender`:

```csharp
// ANTES
internal sealed class EmailTemplateSender(
    IEmailTemplateRepository repo,
    ITemplateRenderer renderer,
    IEmailSender emailSender,          // ← sender global
    ...)

// DESPUÉS
internal sealed class EmailTemplateSender(
    IEmailTemplateRepository repo,
    ITemplateRenderer renderer,
    ITenantEmailSender emailSender,    // ← sender tenant-aware
    ...)
```

El resto del código de `EmailTemplateSender` no cambia — la lógica de resolución de tenant es transparente.

---

## Permisos nuevos a agregar en el seeder de Auth

```csharp
("EmailConfig.Ver",       "Ver configuración de email",      "EmailConfig"),
("EmailConfig.Gestionar", "Gestionar configuración de email", "EmailConfig"),
```

---

## Plan de implementación paso a paso

### Fase 1 — Infraestructura de cifrado (Día 1)

```
□ 1.1 Agregar paquete System.Security.Cryptography (ya incluido en .NET)
□ 1.2 Crear IEncryptionService en CoreTemplate.SharedKernel o CoreTemplate.Infrastructure
□ 1.3 Implementar AesEncryptionService con AES-256-CBC
□ 1.4 Agregar "EncryptionSettings:Key" a appsettings (generar clave aleatoria)
□ 1.5 Registrar en DI: services.AddSingleton<IEncryptionService, AesEncryptionService>()
□ 1.6 Escribir tests unitarios de cifrado/descifrado
```

### Fase 2 — Domain y Application (Día 1-2)

```
□ 2.1 Crear aggregate ConfiguracionEmailTenant con IHasTenant
□ 2.2 Crear eventos: ConfiguracionEmailCreada, ConfiguracionEmailActualizada, ConfiguracionEmailEliminada
□ 2.3 Crear IConfiguracionEmailTenantRepository
□ 2.4 Crear DTOs: ConfiguracionEmailTenantDto (sin exponer credenciales descifradas)
□ 2.5 Crear Commands: CrearConfiguracionEmail, ActualizarConfiguracionEmail, EliminarConfiguracionEmail
□ 2.6 Crear Queries: GetConfiguracionEmailTenant
□ 2.7 Validar: solo un proveedor activo por tenant
```

### Fase 3 — Infrastructure (Día 2-3)

```
□ 3.1 Agregar ConfiguracionEmailTenant al EmailTemplatesDbContext
□ 3.2 Crear configuración EF con índice único (TenantId, EsActivo) WHERE EsActivo = 1
□ 3.3 Crear migración: Add_ConfiguracionEmailTenant
□ 3.4 Implementar ConfiguracionEmailTenantRepository con IgnoreQueryFilters donde aplique
□ 3.5 Implementar IEmailSenderFactory (crea sender por proveedor con credenciales descifradas)
□ 3.6 Implementar ITenantEmailSender / TenantAwareEmailSender
□ 3.7 Modificar EmailTemplateSender: IEmailSender → ITenantEmailSender
□ 3.8 Registrar nuevos servicios en DependencyInjection.cs
```

### Fase 4 — API (Día 3-4)

```
□ 4.1 Crear contratos: CrearConfiguracionEmailRequest, ProbarConfiguracionRequest
□ 4.2 Crear EmailConfigController con endpoints CRUD + probar
□ 4.3 Agregar permisos EmailConfig.Ver y EmailConfig.Gestionar al seeder de Auth
□ 4.4 Registrar controller en Program.cs
```

### Fase 5 — Testing y validación (Día 4-5)

```
□ 5.1 Test: cifrado/descifrado de credenciales
□ 5.2 Test: TenantAwareEmailSender usa config del tenant cuando existe
□ 5.3 Test: TenantAwareEmailSender usa sender global cuando no hay config del tenant
□ 5.4 Test: solo una configuración activa por tenant
□ 5.5 Prueba manual: crear config Mailjet para un tenant y enviar correo de prueba
□ 5.6 Prueba manual: tenant sin config usa el sender global
□ 5.7 Prueba manual: cambiar config de Mailjet a SMTP sin reiniciar la app
```

---

## Consideraciones de seguridad

| Riesgo | Mitigación |
|---|---|
| Credenciales en texto plano en BD | AES-256 obligatorio antes de guardar |
| Clave de cifrado en appsettings | Usar variables de entorno o AWS Secrets Manager en producción |
| Exponer credenciales en la API | El DTO de respuesta nunca incluye ApiKey ni SecretKey — solo indica si están configuradas |
| Rotación de credenciales | El endpoint de actualización cifra las nuevas credenciales y desactiva la configuración anterior |
| Logs con credenciales | El logger nunca registra ApiKey, SecretKey ni passwords |

---

## Consideraciones de rendimiento

| Situación | Solución |
|---|---|
| Consulta a BD en cada envío de correo | Cache en memoria con TTL de 5 minutos por TenantId |
| Creación de sender en cada request | Aceptable — los senders son ligeros. Con cache se evita la consulta a BD |
| Muchos tenants con configuración propia | El cache por TenantId escala bien |

### Cache opcional (implementar si hay problemas de rendimiento):

```csharp
// En TenantAwareEmailSender — cache con IMemoryCache
var cacheKey = $"email-config-{currentTenant.TenantId}";
if (!_cache.TryGetValue(cacheKey, out ConfiguracionEmailTenant? config))
{
    config = await repo.ObtenerActivaAsync(currentTenant.TenantId, ct);
    _cache.Set(cacheKey, config, TimeSpan.FromMinutes(5));
}
```

---

## Impacto en código existente

| Archivo | Cambio | Impacto |
|---|---|---|
| `EmailTemplateSender.cs` | `IEmailSender` → `ITenantEmailSender` | Mínimo — solo cambia el tipo inyectado |
| `EmailTemplatesDbContext.cs` | Agregar `DbSet<ConfiguracionEmailTenant>` | Mínimo |
| `EmailTemplates.Infrastructure/DependencyInjection.cs` | Registrar nuevos servicios | Mínimo |
| `Program.cs` | Registrar nuevo controller | Mínimo |
| `AuthDataSeeder.cs` | Agregar 2 permisos nuevos | Mínimo |
| Resto del sistema | Sin cambios | Ninguno |

---

## Lo que NO cambia

- `IEmailSender` y sus implementaciones (Mailjet, SMTP, SendGrid) — sin cambios
- `EmailTemplate` aggregate — sin cambios
- `EmailTemplateSender` lógica de resolución de plantillas — sin cambios
- Todos los módulos consumidores (Auth, RRHH, Nómina) — sin cambios
- La configuración global en appsettings sigue funcionando como fallback

---

## Dependencias externas

No se requieren paquetes NuGet adicionales:
- `System.Security.Cryptography` — ya incluido en .NET 10
- `Microsoft.Extensions.Caching.Memory` — ya incluido en ASP.NET Core

---

## Criterios de aceptación para considerar la feature completa

```
✅ Un tenant puede configurar Mailjet con sus propias credenciales
✅ Un tenant puede configurar SMTP con su servidor corporativo
✅ Un tenant sin configuración usa el sender global de appsettings
✅ Las credenciales se guardan cifradas en BD (verificable inspeccionando la BD)
✅ El DTO de respuesta nunca expone las credenciales
✅ El endpoint /probar envía un correo real con la configuración del tenant
✅ Cambiar la configuración no requiere reiniciar la aplicación
✅ Los logs no contienen credenciales
✅ Tests unitarios de cifrado/descifrado pasan
✅ Tests de TenantAwareEmailSender pasan (con mock del repositorio)
```

---

**Fecha de documentación:** 2026-04-22
**Estimación:** 3-5 días de desarrollo
**Prerequisito:** Tener implementado el módulo EmailTemplates (✅ ya implementado)
**Siguiente paso cuando se decida implementar:** Iniciar por Fase 1 — infraestructura de cifrado
