# PdfTemplates — Requerimientos Funcionales

> **Building Block:** CoreTemplate.Pdf
> **Módulo:** CoreTemplate.Modules.PdfTemplates
> **Fecha:** 2026-04-22
> **Total:** 13 RF

---

## Contexto

El sistema de plantillas PDF tiene dos capas:

- `CoreTemplate.Pdf` — building block que genera el PDF usando QuestPDF.
  Contiene los **diseños** en código C# (estructura visual, layout, estilos).

- `CoreTemplate.Modules.PdfTemplates` — módulo que gestiona la **configuración
  corporativa** en BD (logo, colores, nombre empresa, pie de página).

**Separación clave:**
- El diseño (dónde va cada elemento) vive en código → se cambia creando una nueva clase
- Los datos corporativos (logo, colores, textos) viven en BD → se cambian desde la UI

---

## RF-PDF-001: Generar PDF desde plantilla
**Prioridad:** Crítica

### Descripción
Cualquier módulo puede generar un PDF pasando el código de plantilla y los datos del negocio.

### Criterios de Aceptación
- Contrato: `IPdfGenerator.GenerarAsync(codigo, tenantId, IPdfContent datos)`
- Resuelve la plantilla de BD (tenant → global → error)
- Resuelve el diseño en código según `CodigoTemplate`
- Combina configuración corporativa + datos del negocio
- Retorna `byte[]` del PDF generado
- Si la plantilla no existe → error descriptivo
- Si el diseño no está registrado → error al iniciar la aplicación (fail-fast)

---

## RF-PDF-002: Gestionar plantillas PDF (CRUD)
**Prioridad:** Crítica

### Descripción
El administrador puede crear, editar y gestionar plantillas PDF desde la UI.

### Criterios de Aceptación
- Crear plantilla con: código único, nombre, módulo, codigoTemplate, datos corporativos
- Editar todos los campos excepto el código (inmutable)
- Activar / desactivar plantillas
- No se puede eliminar una plantilla del sistema (`EsDeSistema = true`)
- Listar plantillas con filtro por módulo, diseño y estado

---

## RF-PDF-003: Diseños disponibles (código C#)
**Prioridad:** Crítica

### Descripción
Los diseños son clases C# registradas en DI. El sistema incluye 4 diseños base.

### Diseños incluidos:

| CodigoTemplate | Descripción | Orientación |
|---|---|---|
| `vertical-estandar` | Encabezado con logo + nombre, tabla de contenido, pie de página | Vertical A4 |
| `horizontal-estandar` | Igual pero orientación horizontal, ideal para reportes anchos | Horizontal A4 |
| `compacto` | Márgenes reducidos, sin encabezado grande, ideal para recibos | Vertical A4 |
| `moderno` | Banda lateral de color corporativo, tipografía moderna | Vertical A4 |

### Criterios de Aceptación
- Cada diseño implementa `IPdfDocumentTemplate`
- El `PdfTemplateFactory` resuelve el diseño por `CodigoTemplate`
- Si se solicita un `CodigoTemplate` no registrado → error al iniciar (fail-fast)
- Agregar un nuevo diseño no requiere modificar código existente

---

## RF-PDF-004: Configuración corporativa por plantilla
**Prioridad:** Crítica

### Descripción
Cada plantilla en BD almacena los datos corporativos que el diseño usa para renderizar.

### Campos configurables:

| Campo | Descripción | Ejemplo |
|---|---|---|
| `NombreEmpresa` | Nombre en el encabezado | "Empresa ABC S.A." |
| `LogoUrl` | URL del logo (local o S3) | "https://..." |
| `ColorEncabezado` | Color de fondo del header | "#1a2e5a" |
| `ColorTextoHeader` | Color del texto del header | "#ffffff" |
| `ColorAcento` | Color de líneas y bordes | "#4f46e5" |
| `TextoSecundario` | Subtítulo o datos fiscales | "RUC: 001-123456-0001" |
| `TextoPiePagina` | Texto del pie de página | "Documento generado por {{SistemaNombre}}" |
| `MostrarNumeroPagina` | Mostrar "Página X de Y" | true |
| `MostrarFechaGeneracion` | Mostrar fecha de generación | true |
| `MarcaDeAgua` | Texto de marca de agua | "BORRADOR" (vacío = sin marca) |
| `Orientacion` | Orientación del documento | "Vertical" / "Horizontal" |

---

## RF-PDF-005: Soporte multi-tenant
**Prioridad:** Alta

### Descripción
Cada tenant puede tener su propia versión de cualquier plantilla con su marca corporativa.

### Jerarquía de resolución:
```
1. Plantilla del tenant actual (TenantId = tenant-A)
2. Plantilla global del sistema (TenantId = null)
3. Error — la plantilla no existe
```

### Criterios de Aceptación
- `PdfPlantilla` implementa `IHasTenant`
- El QueryFilter de `BaseDbContext` aplica automáticamente
- Las plantillas globales usan `IgnoreQueryFilters()` igual que EmailTemplates
- Cada tenant puede tener su logo y colores propios

---

## RF-PDF-006: Seed inicial de plantillas del sistema
**Prioridad:** Crítica

### Descripción
Al arrancar, el seeder crea las plantillas del sistema si no existen.

### Plantillas incluidas:

| Código | Módulo | Diseño | Descripción |
|---|---|---|---|
| `sistema.vertical-estandar` | Sistema | `vertical-estandar` | Plantilla base vertical |
| `sistema.horizontal-estandar` | Sistema | `horizontal-estandar` | Plantilla base horizontal |
| `nomina.comprobante-pago` | Nómina | `vertical-estandar` | Comprobante de pago mensual |
| `contabilidad.factura` | Contabilidad | `vertical-estandar` | Factura estándar |
| `contabilidad.recibo` | Contabilidad | `compacto` | Recibo de pago |
| `rrhh.contrato` | RRHH | `vertical-estandar` | Contrato de trabajo |
| `rrhh.constancia-laboral` | RRHH | `compacto` | Constancia laboral |

### Criterios de Aceptación
- Si la plantilla ya existe en BD → no sobreescribir
- Las plantillas del sistema tienen `EsDeSistema = true`
- El seeder usa `IgnoreQueryFilters()` para no ser bloqueado por el QueryFilter de tenant

---

## RF-PDF-007: Vista previa del PDF
**Prioridad:** Alta

### Descripción
El administrador puede generar una vista previa del PDF con datos de ejemplo.

### Criterios de Aceptación
- Endpoint: `POST /api/pdf-templates/{id}/preview`
- Recibe datos de ejemplo en el body
- Retorna el PDF como `application/pdf` para visualizar en el navegador
- No guarda el PDF generado

---

## RF-PDF-008: Listar diseños disponibles
**Prioridad:** Media

### Descripción
El administrador puede ver qué diseños están disponibles para asignar a una plantilla.

### Criterios de Aceptación
- Endpoint: `GET /api/pdf-templates/disenios`
- Retorna lista de `{ codigo, nombre, descripcion, orientacion }`
- Los diseños se registran en DI — la lista es dinámica
- Útil para el selector en la UI al crear/editar una plantilla

---

## RF-PDF-009: Permisos del módulo
**Prioridad:** Alta

### Permisos:

| Código | Descripción |
|---|---|
| `PdfTemplates.Ver` | Ver y listar plantillas |
| `PdfTemplates.Editar` | Editar configuración corporativa |
| `PdfTemplates.Gestionar` | Activar/desactivar, crear plantillas |
| `PdfTemplates.Preview` | Generar vista previa |

---

## RF-PDF-010: Variables en textos de plantilla
**Prioridad:** Media

### Descripción
Los campos de texto de la plantilla soportan variables que se reemplazan al generar.

### Variables disponibles en textos:
- `{{SistemaNombre}}` — nombre del sistema
- `{{FechaGeneracion}}` — fecha y hora de generación
- `{{NombreEmpresa}}` — nombre de la empresa de la plantilla
- `{{AnioActual}}` — año actual

### Criterios de Aceptación
- Aplica a: `TextoPiePagina`, `TextoSecundario`, `MarcaDeAgua`
- El reemplazo ocurre al momento de generar el PDF

---

## RF-PDF-011: Integración con Storage
**Prioridad:** Alta

### Descripción
Los PDFs generados pueden subirse automáticamente al proveedor de almacenamiento.

### Criterios de Aceptación
- El módulo consumidor decide si sube el PDF o solo lo retorna como bytes
- Si sube: usa `IStorageService` con contexto `{modulo}/pdfs/{año}/{mes}`
- Retorna la URL del PDF almacenado para compartir o adjuntar al correo

---

## RF-PDF-012: Integración con Email
**Prioridad:** Alta

### Descripción
Los PDFs generados pueden enviarse por correo como adjunto.

### Criterios de Aceptación
- El módulo consumidor combina `IPdfGenerator` + `IEmailTemplateSender`
- El PDF se adjunta como `EmailAdjunto` con `ContentType = "application/pdf"`
- El nombre del archivo es configurable por el módulo consumidor

---

## RF-PDF-013: Endpoint para generar y descargar PDF
**Prioridad:** Alta

### Descripción
Cada módulo expone un endpoint que genera el PDF y lo retorna para descarga directa.

### Criterios de Aceptación
- Retorna `File(pdfBytes, "application/pdf", "nombre-archivo.pdf")`
- El navegador lo abre o descarga según la configuración del cliente
- Opcionalmente también lo sube a Storage y retorna la URL

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 4 |
| Alta | 7 |
| Media | 2 |
| **Total** | **13** |

---

**Fecha:** 2026-04-22
