# EmailTemplates — Requerimientos Funcionales

> **Módulo:** CoreTemplate.Modules.EmailTemplates
> **Fecha:** 2026-04-22
> **Total:** 12 RF

---

## Contexto

`EmailTemplates` es un módulo de negocio que gestiona plantillas de correo editables
desde la UI del sistema. Trabaja en conjunto con el building block `CoreTemplate.Email`:

- `CoreTemplate.Email` → sabe cómo enviar (Mailjet, SMTP, SendGrid)
- `CoreTemplate.Modules.EmailTemplates` → sabe qué enviar (contenido, asunto, variables)

Cualquier módulo que necesite enviar un correo con contenido personalizable
usa `IEmailTemplateSender` en lugar de `IEmailSender` directamente.

**Flujo de resolución de template:**
```
1. ¿Existe template en BD con ese código y está activo? → usar BD
2. Si no → usar archivo .html del proyecto (fallback)
3. Si no existe ninguno → error claro al arrancar
```

---

## RF-ET-001: Gestionar plantillas de correo (CRUD)
**Prioridad:** Crítica

### Descripción
El administrador puede crear, editar y gestionar plantillas de correo desde la UI.

### Criterios de Aceptación
- Crear plantilla con: código único, nombre descriptivo, asunto, cuerpo HTML, módulo, variables disponibles
- Editar asunto y cuerpo HTML de una plantilla existente
- Activar / desactivar plantillas
- No se puede eliminar una plantilla del sistema (solo desactivar)
- El código es inmutable una vez creado
- Listar plantillas con filtro por módulo y estado

---

## RF-ET-002: Variables dinámicas en plantillas
**Prioridad:** Crítica

### Descripción
Las plantillas soportan variables que se reemplazan al momento del envío.

### Criterios de Aceptación
- Sintaxis de variables: `{{NombreVariable}}` (doble llave)
- Cada plantilla declara sus variables disponibles (para documentar al editor)
- Al renderizar, las variables no encontradas se reemplazan por cadena vacía
- Variables globales disponibles en todas las plantillas:
  - `{{SistemaNombre}}` — nombre del sistema (de appsettings)
  - `{{SistemaUrl}}` — URL base del sistema
  - `{{AnioActual}}` — año actual
  - `{{FechaActual}}` — fecha actual formateada

---

## RF-ET-003: Layout base (header/footer corporativo)
**Prioridad:** Alta

### Descripción
Todas las plantillas se renderizan dentro de un layout HTML base que incluye
header y footer corporativo. El layout también es editable desde la UI.

### Criterios de Aceptación
- Existe una plantilla especial con código `sistema.layout` que envuelve el contenido
- El layout incluye: logo (URL configurable), colores corporativos, footer con nombre del sistema
- Si no existe `sistema.layout` en BD → usa el layout base del proyecto
- El cuerpo de cada plantilla se inserta en el placeholder `{{Contenido}}` del layout
- El layout puede desactivarse (enviar sin layout) por plantilla si se requiere

---

## RF-ET-004: Envío usando plantilla
**Prioridad:** Crítica

### Descripción
Los módulos consumidores envían correos referenciando el código de la plantilla
y pasando las variables necesarias.

### Criterios de Aceptación
- Contrato: `IEmailTemplateSender.EnviarAsync(codigo, para, variables, adjuntos?)`
- Resuelve la plantilla (BD → fallback archivo)
- Reemplaza variables en asunto y cuerpo
- Aplica el layout base
- Delega el envío a `IEmailSender`
- Retorna `EmailResult` con indicador de éxito/fallo

---

## RF-ET-005: Plantillas del sistema (seed inicial)
**Prioridad:** Crítica

### Descripción
Al arrancar la aplicación, el seeder registra las plantillas del sistema si no existen.
Estas plantillas tienen archivos `.html` de fallback en el proyecto.

### Plantillas incluidas desde el inicio:

| Código | Módulo | Asunto | Variables |
|---|---|---|---|
| `auth.reset-password` | Auth | Restablece tu contraseña en {{SistemaNombre}} | NombreUsuario, LinkReset, ExpiraEn |
| `auth.cuenta-bloqueada` | Auth | Tu cuenta ha sido bloqueada temporalmente | NombreUsuario, BloqueadaHasta, MotivoBloqueo |
| `auth.bienvenida` | Auth | Bienvenido a {{SistemaNombre}} | NombreUsuario, LinkAcceso |
| `auth.password-cambiado` | Auth | Tu contraseña fue cambiada | NombreUsuario, FechaCambio, Ip |
| `auth.2fa-activado` | Auth | Autenticación de dos factores activada | NombreUsuario, FechaActivacion |
| `auth.nueva-sesion` | Auth | Nueva sesión iniciada en tu cuenta | NombreUsuario, Dispositivo, Ip, Canal, Fecha |
| `sistema.layout` | Sistema | — (es el layout base) | SistemaNombre, SistemaUrl, AnioActual, Contenido |

### Criterios de Aceptación
- Si la plantilla ya existe en BD → no sobreescribir (el admin puede haberla editado)
- Si no existe → crear con los valores del archivo `.html` del proyecto
- Las plantillas del sistema tienen `EsDeSistema = true` (no se pueden eliminar)

---

## RF-ET-006: Vista previa de plantilla
**Prioridad:** Alta

### Descripción
El administrador puede previsualizar cómo se verá una plantilla con datos de ejemplo.

### Criterios de Aceptación
- Endpoint: `POST /api/email-templates/{id}/preview`
- Recibe variables de ejemplo en el body
- Retorna el HTML renderizado (con layout aplicado)
- No envía ningún correo — solo renderiza
- Útil para verificar el diseño antes de activar la plantilla

---

## RF-ET-007: Envío de prueba
**Prioridad:** Media

### Descripción
El administrador puede enviar un correo de prueba con una plantilla a una dirección específica.

### Criterios de Aceptación
- Endpoint: `POST /api/email-templates/{id}/enviar-prueba`
- Recibe: `{ destinatario: "admin@test.com", variables: {...} }`
- Envía el correo real usando el proveedor configurado
- Registra en log que fue un envío de prueba
- Solo accesible con permiso `EmailTemplates.EnviarPrueba`

---

## RF-ET-008: Historial de envíos (opcional, configurable)
**Prioridad:** Baja

### Descripción
El sistema puede registrar un historial de correos enviados para auditoría.

### Criterios de Aceptación
- Configurable: `"EmailSettings": { "GuardarHistorial": false }` (default: false)
- Si activo: registra destinatario, código de plantilla, fecha, resultado, messageId
- No guarda el contenido del correo (privacidad)
- Accesible desde `/api/email-templates/historial` con filtros

---

## RF-ET-009: Permisos del módulo
**Prioridad:** Alta

### Descripción
El acceso al módulo se controla con permisos específicos.

### Permisos:

| Código | Descripción |
|---|---|
| `EmailTemplates.Ver` | Ver y listar plantillas |
| `EmailTemplates.Editar` | Editar asunto y cuerpo de plantillas |
| `EmailTemplates.Gestionar` | Activar/desactivar plantillas |
| `EmailTemplates.EnviarPrueba` | Enviar correos de prueba |

---

## RF-ET-010: Integración con eventos de Auth
**Prioridad:** Crítica

### Descripción
El módulo registra handlers para los eventos de dominio de Auth que requieren
envío de correo. Esto conecta Auth con Email sin acoplamiento directo.

### Eventos de Auth que disparan envío de correo:

| Evento de Auth | Template usado | Cuándo |
|---|---|---|
| `RestablecimientoSolicitadoEvent` | `auth.reset-password` | Al solicitar reset de contraseña |
| `UsuarioBloqueadoEvent` | `auth.cuenta-bloqueada` | Al bloquear cuenta (manual o automático) |
| `PasswordCambiadoEvent` | `auth.password-cambiado` | Al cambiar contraseña exitosamente |
| `DosFactoresActivadoEvent` | `auth.2fa-activado` | Al activar 2FA |
| `UsuarioRegistradoEvent` | `auth.bienvenida` | Al registrar nuevo usuario (configurable) |

### Criterios de Aceptación
- Los handlers son opcionales y configurables (se pueden desactivar por appsettings)
- Si el envío falla → log warning, NO revertir la operación de Auth
- Los handlers se registran en `EmailTemplates.Infrastructure.DependencyInjection`

---

## RF-ET-011: Soporte multi-tenant en plantillas
**Prioridad:** Media

### Descripción
Cuando el sistema es multi-tenant, cada tenant puede tener sus propias plantillas
personalizadas que sobreescriben las del sistema.

### Criterios de Aceptación
- Jerarquía de resolución: Tenant específico → Sistema global → Archivo fallback
- Solo aplica cuando `IsMultiTenant = true`
- Las plantillas de tenant tienen `TenantId` asignado
- Las plantillas del sistema tienen `TenantId = null`

---

## RF-ET-012: Validación de variables al guardar
**Prioridad:** Media

### Descripción
Al guardar una plantilla, el sistema valida que las variables declaradas
estén presentes en el cuerpo HTML.

### Criterios de Aceptación
- Warning (no error) si una variable declarada no aparece en el HTML
- Warning si el HTML contiene `{{Variable}}` no declarada en la lista
- El admin puede ignorar los warnings y guardar igual

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 5 |
| Alta | 4 |
| Media | 2 |
| Baja | 1 |
| **Total** | **12** |

---

**Fecha:** 2026-04-22

---

## Addendum — Multi-tenant y plantillas por empresa

> **Fecha:** 2026-04-22

### RF-ET-013: Plantillas personalizadas por empresa (tenant)
**Prioridad:** Alta

### Descripción
En modo multi-tenant cada empresa puede tener sus propias versiones de las plantillas
del sistema con su logo, colores corporativos y contenido personalizado.

### Criterios de Aceptación
- Una plantilla con `TenantId = tenant-A` tiene prioridad sobre la plantilla global (`TenantId = null`)
- Si el tenant no tiene versión propia → usa la plantilla global del sistema
- Si no hay plantilla global → usa el archivo `.html` del proyecto (fallback)
- El admin de cada tenant puede crear su versión de cualquier plantilla del sistema
- Las plantillas de tenant no son visibles para otros tenants (QueryFilter automático de `BaseDbContext`)

### Jerarquía de resolución

```
1. BD — plantilla del tenant actual     (TenantId = tenant-A)
2. BD — plantilla global del sistema    (TenantId = null, IgnoreQueryFilters)
3. Archivo .html del proyecto           (fallback final)
```

---

### RF-ET-014: Layout corporativo por empresa
**Prioridad:** Alta

### Descripción
Cada empresa puede tener su propio layout (`sistema.layout`) con su logo,
colores y datos de contacto. Todos los correos de esa empresa usarán
automáticamente su diseño corporativo.

### Criterios de Aceptación
- El admin del tenant edita la plantilla `sistema.layout` con `TenantId = tenant-actual`
- Las variables `{{SistemaNombre}}`, `{{SistemaLogoUrl}}`, `{{SistemaUrl}}` se inyectan
  automáticamente por `TemplateRenderer` desde `AppSettings`
- En el futuro (RF-ET-015) estas variables podrán venir de una tabla `ConfiguracionEmailTenant`
  para que cada empresa tenga su propio nombre y logo sin tocar appsettings

### Ejemplo de uso

```
Tenant A (Empresa ABC):
  → Tiene sistema.layout con logo de Empresa ABC y color #1a73e8
  → Todos sus correos (reset, bienvenida, etc.) usan ese diseño

Tenant B (Empresa XYZ):
  → No tiene sistema.layout propio
  → Usa el layout global del sistema
```

---

### Implementación técnica — por qué funciona

`EmailTemplate` implementa `IHasTenant`. El `BaseDbContext` aplica automáticamente
un `QueryFilter` que filtra por `TenantId` del request en modo multi-tenant.

Las plantillas globales (`TenantId = null`) requieren `IgnoreQueryFilters()` al consultarlas
porque el QueryFilter las ocultaría al haber un tenant activo en el contexto.

```csharp
// Plantilla del tenant actual — pasa por QueryFilter normal
await repo.ObtenerPorCodigoAsync("auth.reset-password", currentTenant.TenantId);

// Plantilla global — requiere IgnoreQueryFilters
await db.Plantillas
    .IgnoreQueryFilters()
    .Where(t => t.Codigo == codigo && t.TenantId == null)
    .FirstOrDefaultAsync();
```

---

**Fecha addendum:** 2026-04-22
