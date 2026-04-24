# PdfTemplates — Modelo de Dominio y Contratos

> **Fecha:** 2026-04-22

---

## Aggregate: PdfPlantilla

```
PdfPlantilla (AggregateRoot, IHasTenant)
  ─── Identidad ──────────────────────────────────────────
  + Id                    : Guid
  + TenantId              : Guid?       null = plantilla global del sistema
  + Codigo                : string      "nomina.comprobante-pago" (inmutable)
  + Nombre                : string      "Comprobante de Pago"
  + Modulo                : string      "Nomina" | "Contabilidad" | "RRHH"
  + CodigoTemplate        : string      "vertical-estandar" | "moderno" | "compacto"

  ─── Marca corporativa ──────────────────────────────────
  + NombreEmpresa         : string      "Empresa ABC S.A."
  + LogoUrl               : string?     URL del logo (local o S3)
  + ColorEncabezado       : string      "#1a2e5a"
  + ColorTextoHeader      : string      "#ffffff"
  + ColorAcento           : string      "#4f46e5"
  + TextoSecundario       : string?     "RUC: 001-123456-0001"

  ─── Pie de página ──────────────────────────────────────
  + TextoPiePagina        : string?     "{{NombreEmpresa}} — {{FechaGeneracion}}"
  + MostrarNumeroPagina   : bool        true
  + MostrarFechaGeneracion: bool        true

  ─── Opciones ───────────────────────────────────────────
  + MarcaDeAgua           : string?     "BORRADOR" (vacío = sin marca)
  + EsDeSistema           : bool        no se puede eliminar
  + EsActivo              : bool
  + CreadoEn              : DateTime
  + ModificadoEn          : DateTime?
  + ModificadoPor         : Guid?

Métodos:
  + Crear(...)            : Result<PdfPlantilla>
  + Actualizar(...)       : Result
  + Activar()             : Result
  + Desactivar()          : Result
```

---

## Contratos del Building Block

### IPdfDocumentTemplate

```
IPdfDocumentTemplate
  + Codigo      : string          "vertical-estandar"
  + Nombre      : string          "Vertical Estándar"
  + Descripcion : string
  + Orientacion : string          "Vertical" | "Horizontal"

  + Generar(PdfPlantillaData plantilla, IPdfContent contenido) : byte[]
```

---

### IPdfContent

```
IPdfContent
  + ObtenerDatos() : Dictionary<string, object>
```

Cada módulo implementa esta interfaz con sus datos específicos:

```csharp
// Nómina
public class ComprobantePagoContent : IPdfContent
{
    public string NombreEmpleado { get; set; }
    public string Cargo { get; set; }
    public string Periodo { get; set; }
    public decimal SalarioBruto { get; set; }
    public decimal Deducciones { get; set; }
    public decimal SalarioNeto { get; set; }
}

// Contabilidad
public class FacturaContent : IPdfContent
{
    public string NumeroFactura { get; set; }
    public string Cliente { get; set; }
    public List<LineaFactura> Lineas { get; set; }
    public decimal Total { get; set; }
}
```

---

### IPdfGenerator

```
IPdfGenerator
  + GenerarAsync(
        codigo    : string,
        tenantId  : Guid?,
        contenido : IPdfContent,
        ct        : CancellationToken
    ) : Task<byte[]>
```

---

### PdfPlantillaData (DTO interno del building block)

```
PdfPlantillaData (record)
  + NombreEmpresa         : string
  + LogoUrl               : string?
  + ColorEncabezado       : string
  + ColorTextoHeader      : string
  + ColorAcento           : string
  + TextoSecundario       : string?
  + TextoPiePagina        : string?
  + MostrarNumeroPagina   : bool
  + MostrarFechaGeneracion: bool
  + MarcaDeAgua           : string?
  + FechaGeneracion       : DateTime
  + SistemaNombre         : string
```

---

## Estructura de proyectos

```
src/BuildingBlocks/CoreTemplate.Pdf/
  Abstractions/
    IPdfDocumentTemplate.cs
    IPdfContent.cs
    IPdfGenerator.cs
    PdfPlantillaData.cs
  Templates/
    VerticalEstandarTemplate.cs     diseño A4 vertical clásico
    HorizontalEstandarTemplate.cs   diseño A4 horizontal
    CompactoTemplate.cs             diseño compacto para recibos
    ModernoTemplate.cs              diseño con banda lateral de color
  Services/
    PdfTemplateFactory.cs           resuelve IPdfDocumentTemplate por CodigoTemplate
    PdfGenerator.cs                 implementa IPdfGenerator
  DependencyInjection.cs
  CoreTemplate.Pdf.csproj           → PackageReference: QuestPDF

src/Modules/PdfTemplates/
  CoreTemplate.Modules.PdfTemplates.Domain/
    Aggregates/
      PdfPlantilla.cs
    Events/
      PdfTemplateEvents.cs
    Repositories/
      IPdfPlantillaRepository.cs

  CoreTemplate.Modules.PdfTemplates.Application/
    Commands/
      PdfTemplateCommands.cs        Crear, Actualizar, Activar, Desactivar
    Queries/
      PdfTemplateQueries.cs         GetById, GetAll, GetDisenios, Preview
    DTOs/
      PdfPlantillaDto.cs
      DisenioDisponibleDto.cs
      PreviewPdfRequest.cs

  CoreTemplate.Modules.PdfTemplates.Infrastructure/
    Persistence/
      PdfTemplatesDbContext.cs      schema: PdfTemplates
      Configurations/
        PdfPlantillaConfiguration.cs
      PdfTemplatesDataSeeder.cs
    Repositories/
      PdfPlantillaRepository.cs
    Services/
      PdfGeneratorService.cs        implementa IPdfGenerator usando el repo + factory
    DependencyInjection.cs

  CoreTemplate.Modules.PdfTemplates.Api/
    Controllers/
      PdfTemplatesController.cs
    Contracts/
      PdfTemplateContracts.cs
```

---

## Modelo de datos

### Tabla: PdfTemplates.Plantillas

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | null = global |
| Codigo | nvarchar(100) | Único por tenant. Ej: "nomina.comprobante-pago" |
| Nombre | nvarchar(200) | Nombre descriptivo |
| Modulo | nvarchar(50) | "Nomina", "Contabilidad", "RRHH" |
| CodigoTemplate | nvarchar(50) | "vertical-estandar", "moderno", "compacto" |
| NombreEmpresa | nvarchar(200) | |
| LogoUrl | nvarchar(2000) | |
| ColorEncabezado | nvarchar(7) | Hex color |
| ColorTextoHeader | nvarchar(7) | Hex color |
| ColorAcento | nvarchar(7) | Hex color |
| TextoSecundario | nvarchar(500) | |
| TextoPiePagina | nvarchar(500) | Puede contener variables |
| MostrarNumeroPagina | bit | |
| MostrarFechaGeneracion | bit | |
| MarcaDeAgua | nvarchar(100) | Vacío = sin marca |
| EsDeSistema | bit | |
| EsActivo | bit | |
| CreadoEn | datetime2 | |
| ModificadoEn | datetime2? | |
| ModificadoPor | uniqueidentifier? | |

**Índice único:** `(Codigo, TenantId)`

---

## Endpoints del módulo

| Método | Ruta | Descripción | Permiso |
|---|---|---|---|
| GET | `/api/pdf-templates` | Listar plantillas | `PdfTemplates.Ver` |
| GET | `/api/pdf-templates/{id}` | Obtener por ID | `PdfTemplates.Ver` |
| GET | `/api/pdf-templates/disenios` | Listar diseños disponibles | `PdfTemplates.Ver` |
| POST | `/api/pdf-templates` | Crear plantilla | `PdfTemplates.Gestionar` |
| PUT | `/api/pdf-templates/{id}` | Actualizar configuración | `PdfTemplates.Editar` |
| PUT | `/api/pdf-templates/{id}/activar` | Activar | `PdfTemplates.Gestionar` |
| PUT | `/api/pdf-templates/{id}/desactivar` | Desactivar | `PdfTemplates.Gestionar` |
| POST | `/api/pdf-templates/{id}/preview` | Vista previa PDF | `PdfTemplates.Preview` |

---

**Fecha:** 2026-04-22
