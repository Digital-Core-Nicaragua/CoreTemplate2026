# Plan Maestro — Pendientes de Implementación

> **Fecha:** 2026-04-22
> **Estado del sistema:** Building blocks + Auth + Catálogos + Email + Storage +
> Archivos + EmailTemplates + PdfTemplates implementados y compilando.

---

## Resumen de estado actual

| Componente | Implementado | Documentado |
|---|---|---|
| SharedKernel, Infrastructure, Api.Common | ✅ | ✅ |
| Auditing, Logging, Monitoring | ✅ | ✅ |
| Auth (JWT, 2FA, Sesiones, Roles, Multi-tenant, Portal) | ✅ | ✅ |
| Catálogos (patrón de referencia) | ✅ | ✅ |
| CoreTemplate.Email (Mailjet, SMTP, SendGrid) | ✅ | ✅ |
| CoreTemplate.Storage (Local, S3, Firebase) | ✅ | ✅ |
| CoreTemplate.Pdf (QuestPDF, 4 diseños) | ✅ | ✅ |
| Módulo Archivos | ✅ | ✅ |
| Módulo EmailTemplates | ✅ | ✅ |
| Módulo PdfTemplates | ✅ | ✅ |
| rename.ps1 | ✅ | ✅ |
| Arquitectura actualizada | ✅ | ✅ |

---

## 🔴 ALTA PRIORIDAD

### 1. Módulo Notificaciones en Tiempo Real (SignalR)
**Documento completo:** `docs/Notificaciones/README.md`
**Esfuerzo:** 3-4 días

```
Componentes a crear:
  CoreTemplate.Notifications (building block)
    → INotificationSender, NotificationMessage, NotificationResult
    → NotificationHub (SignalR, autenticación JWT)
    → SignalRNotificationSender

  Módulo Notificaciones
    → Aggregate Notificacion (IHasTenant)
    → NotificacionesDbContext (schema: Notificaciones)
    → Commands: MarcarComoLeida, MarcarTodasComoLeidas
    → Queries: GetMisNotificaciones, GetConteoNoLeidas
    → GET /api/notificaciones
    → WS  /hubs/notificaciones

  Handlers de eventos de Auth
    → SesionCreadaEvent     → 🔒 "Nueva sesión iniciada"
    → UsuarioBloqueadoEvent → ⚠️ "Tu cuenta fue bloqueada"
    → PasswordCambiadoEvent → 🔒 "Tu contraseña fue cambiada"
```

---

### 2. Módulo Configuración del Sistema
**Documento completo:** `docs/Configuracion/README.md`
**Esfuerzo:** 3-4 días

```
Componentes a crear:
  Aggregate ConfiguracionItem (IHasTenant)
    → Clave (inmutable), Valor, Tipo, Grupo, EsEditable

  IConfiguracionService
    → ObtenerStringAsync, ObtenerIntAsync, ObtenerBoolAsync
    → Cache IMemoryCache con TTL 10 minutos

  ConfiguracionDbContext (schema: Configuracion)
  ConfiguracionDataSeeder
    → sistema.nombre, sistema.moneda, sistema.zona-horaria
    → facturacion.serie, facturacion.prefijo, facturacion.impuesto
    → nomina.dia-pago, rrhh.dias-vacaciones

  GET /api/configuracion
  PUT /api/configuracion/{clave}

  Integración posterior:
    → PdfTemplates lee sistema.nombre de ConfiguracionService
    → EmailTemplates lee sistema.nombre de ConfiguracionService
```

---

### 3. Auditoría Visible (endpoints de consulta)
**Documento completo:** `docs/Auditoria/README.md`
**Esfuerzo:** 1-2 días
**Nota:** El building block ya guarda los logs — solo falta la capa de consulta.

```
Componentes a crear:
  AuditoriaController
  Queries: GetAuditLogs (con filtros), GetAuditLogById
  GET /api/auditoria?entidad=&usuarioId=&accion=&fechaDesde=&fechaHasta=
  GET /api/auditoria/{id}
  Permiso nuevo: Auditoria.Ver
```

---

## 🟡 MEDIA PRIORIDAD

### 4. Tests de módulos nuevos
**Esfuerzo:** 3-5 días

```
tests/
  CoreTemplate.Email.Tests/
  CoreTemplate.Storage.Tests/
  CoreTemplate.Modules.EmailTemplates.Tests/
  CoreTemplate.Modules.Archivos.Tests/
  CoreTemplate.Modules.PdfTemplates.Tests/
```

### 5. Configuración de Email por Tenant
**Documento completo:** `docs/Email/03-Mejoras/01-CONFIGURACION-EMAIL-POR-TENANT.md`
**Esfuerzo:** 3-5 días

```
Componentes a crear:
  IEncryptionService / AesEncryptionService (AES-256)
  Aggregate ConfiguracionEmailTenant (credenciales cifradas)
  TenantAwareEmailSender
  EmailSenderFactory
  GET/POST/DELETE /api/email-config
  POST /api/email-config/probar
```

### 6. Configuración de PDF por Tenant
**Documento completo:** `docs/PdfTemplates/03-Mejoras/01-CONFIGURACION-PDF-POR-TENANT.md`
**Esfuerzo:** 2-3 días

```
Componentes a crear:
  Aggregate ConfiguracionPdfTenant
  Modificar ModuloPdfGenerator para aplicar config del tenant
  GET/POST/DELETE /api/pdf-templates/configuracion-tenant
```

---

## 🟢 BAJA PRIORIDAD (documentadas, implementar cuando se necesiten)

### 7. Mejoras de PdfTemplates
**Documento:** `docs/PdfTemplates/03-Mejoras/README.md`

```
□ Templates en múltiples idiomas
□ Imágenes en el contenido (fotos de empleados, productos)
□ Generación en lote / batch (ZIP con múltiples PDFs)
□ Firma digital de documentos
□ Historial de PDFs generados
```

### 8. Mejoras de EmailTemplates
```
□ Historial de correos enviados
□ Reintentos automáticos con Polly
□ Templates en múltiples idiomas
```

### 9. Mejoras de Storage
```
□ Compresión automática de imágenes
□ Virus scanning (decorador de IStorageService)
□ CDN (CloudFront para S3)
□ Límite de almacenamiento por tenant
□ Implementar Firebase Storage (estructura ya existe)
```

### 10. Mejoras de Auth
```
□ Registro por Teléfono / WhatsApp (Portal Clientes) — ya documentado
□ Magic Link (login sin contraseña)
□ Política de contraseñas históricas
□ IP Whitelist por usuario/tenant
```

---

## Orden de implementación recomendado

```
Paso 1 — Auditoría Visible        (1-2 días, bajo riesgo, valor inmediato)
Paso 2 — Configuración del Sistema (3-4 días, base para módulos de negocio)
Paso 3 — Notificaciones SignalR    (3-4 días, alto valor para UX)
Paso 4 — Tests nuevos módulos      (3-5 días, calidad)
Paso 5 — Config Email por Tenant   (cuando haya tenants con correo propio)
Paso 6 — Config PDF por Tenant     (cuando haya tenants con marca propia)
Paso 7+— Mejoras menores           (según necesidad)
```

---

## Módulos de negocio (fuera del scope de la plantilla)

Se crean usando Catálogos como patrón de referencia cuando el sistema
se use en un proyecto real:

```
□ RRHH          → Empleados, Candidatos, Contratos, Vacaciones
□ Nómina        → Períodos, Cálculos, Comprobantes (PDF + Email)
□ Contabilidad  → Facturas, Recibos, Cuentas (PDF + Storage)
□ Inventario    → Productos, Movimientos, Almacenes
□ CRM           → Clientes, Oportunidades, Seguimientos
```

---

**Fecha:** 2026-04-22
