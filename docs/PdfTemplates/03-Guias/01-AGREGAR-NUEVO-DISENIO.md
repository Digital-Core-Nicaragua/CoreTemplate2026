# Guía: Cómo agregar un nuevo diseño de PDF

> **Módulo:** CoreTemplate.Pdf
> **Audiencia:** Desarrolladores
> **Tiempo estimado:** 30-60 minutos
> **Fecha:** 2026-04-22

---

## Conceptos clave antes de empezar

```
DISEÑO (código C#)              PLANTILLA (base de datos)
──────────────────              ─────────────────────────
Estructura visual               Configuración corporativa
Dónde va el logo                Cuál es el logo
Cómo se ven las tablas          Cuáles son los colores
Estilo del encabezado           Cuál es el nombre de la empresa
Número de columnas              Cuál es el texto del pie

Se cambia redeployando          Se cambia desde la UI
Vive en código C#               Vive en base de datos
```

Un diseño puede ser usado por múltiples plantillas.
Una plantilla apunta a exactamente un diseño.

---

## Paso 1 — Crear la clase del diseño

Crea un nuevo archivo en:
```
src/BuildingBlocks/CoreTemplate.Pdf/Templates/NombreDelTemplate.cs
```

La clase debe implementar `IPdfDocumentTemplate`:

```csharp
using CoreTemplate.Pdf.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoreTemplate.Pdf.Templates;

/// <summary>
/// Diseño [nombre descriptivo].
/// [Descripción de cuándo usarlo y qué lo hace diferente].
/// </summary>
public sealed class NombreDelTemplate : IPdfDocumentTemplate
{
    // ─── Identificación ──────────────────────────────────────────────────────
    // IMPORTANTE: Este código debe ser único entre todos los diseños.
    // Una vez en producción NO cambiar — las plantillas en BD lo referencian.
    public string Codigo => "nombre-del-template";

    public string Nombre => "Nombre descriptivo del diseño";
    public string Descripcion => "Descripción de cuándo usar este diseño.";
    public string Orientacion => "Vertical"; // "Vertical" o "Horizontal"

    // ─── Generación ──────────────────────────────────────────────────────────
    public byte[] Generar(PdfPlantillaData plantilla, IPdfContent contenido)
    {
        var datos = contenido.ObtenerDatos();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Configuración de página
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // ── ENCABEZADO ────────────────────────────────────────────
                page.Header().Element(header => RenderEncabezado(header, plantilla));

                // ── CONTENIDO ─────────────────────────────────────────────
                page.Content().Padding(10).Column(column =>
                {
                    column.Spacing(8);
                    RenderContenido(column, plantilla, datos);
                });

                // ── PIE DE PÁGINA ─────────────────────────────────────────
                page.Footer().Element(footer => RenderPie(footer, plantilla));
            });
        }).GeneratePdf();
    }

    // ─── Encabezado ──────────────────────────────────────────────────────────
    private static void RenderEncabezado(IContainer header, PdfPlantillaData plantilla)
    {
        header.Background(plantilla.ColorEncabezado).Padding(15).Row(row =>
        {
            // Logo (si existe)
            if (!string.IsNullOrWhiteSpace(plantilla.LogoUrl))
            {
                row.ConstantItem(80).AlignMiddle().Image(plantilla.LogoUrl)
                    .WithRasterDpi(300);
            }

            // Nombre de la empresa y texto secundario
            row.RelativeItem().AlignMiddle().Column(col =>
            {
                col.Item().Text(plantilla.NombreEmpresa)
                    .FontColor(plantilla.ColorTextoHeader)
                    .FontSize(16).Bold();

                if (!string.IsNullOrWhiteSpace(plantilla.TextoSecundario))
                {
                    col.Item().Text(plantilla.TextoSecundario)
                        .FontColor(plantilla.ColorTextoHeader)
                        .FontSize(10);
                }
            });
        });
    }

    // ─── Contenido ───────────────────────────────────────────────────────────
    private static void RenderContenido(
        ColumnDescriptor column,
        PdfPlantillaData plantilla,
        Dictionary<string, object> datos)
    {
        // Aquí va la lógica específica de este diseño.
        // Los datos del negocio vienen en el diccionario 'datos'.
        // Ejemplo:
        foreach (var (clave, valor) in datos)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(150).Text(clave).SemiBold();
                row.RelativeItem().Text(valor?.ToString() ?? string.Empty);
            });
        }
    }

    // ─── Pie de página ───────────────────────────────────────────────────────
    private static void RenderPie(IContainer footer, PdfPlantillaData plantilla)
    {
        footer.BorderTop(1).BorderColor(Colors.Grey.Lighten2)
            .Padding(5).Row(row =>
            {
                // Texto del pie
                row.RelativeItem().Text(plantilla.TextoPiePaginaRenderizado)
                    .FontSize(8).FontColor(Colors.Grey.Darken1);

                // Número de página
                if (plantilla.MostrarNumeroPagina)
                {
                    row.ConstantItem(80).AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                }
            });
    }
}
```

---

## Paso 2 — Registrar el diseño en DI

Abre el archivo:
```
src/BuildingBlocks/CoreTemplate.Pdf/DependencyInjection.cs
```

Agrega una línea:

```csharp
// Diseños existentes
services.AddSingleton<IPdfDocumentTemplate, VerticalEstandarTemplate>();
services.AddSingleton<IPdfDocumentTemplate, HorizontalEstandarTemplate>();
services.AddSingleton<IPdfDocumentTemplate, CompactoTemplate>();
services.AddSingleton<IPdfDocumentTemplate, ModernoTemplate>();

// ← Agrega tu nuevo diseño aquí
services.AddSingleton<IPdfDocumentTemplate, NombreDelTemplate>();
```

---

## Paso 3 — Verificar que compila

```bash
dotnet build src\Host\CoreTemplate.Api\CoreTemplate.Api.csproj
```

Si hay errores de compilación en tu template, corrígelos antes de continuar.

---

## Paso 4 — Crear la plantilla en BD

Tienes dos opciones:

### Opción A — Via API (recomendado para plantillas de módulos específicos)

Levanta la API, haz login y ejecuta:

```http
POST /api/pdf-templates
Authorization: Bearer {token}
Content-Type: application/json

{
  "codigo": "modulo.nombre-documento",
  "nombre": "Nombre descriptivo",
  "modulo": "NombreModulo",
  "codigoTemplate": "nombre-del-template",
  "nombreEmpresa": "Mi Empresa S.A.",
  "logoUrl": "",
  "colorEncabezado": "#1a2e5a",
  "colorTextoHeader": "#ffffff",
  "colorAcento": "#4f46e5",
  "textoSecundario": "RUC: 001-000000-0000",
  "textoPiePagina": "{{NombreEmpresa}} — Generado el {{FechaGeneracion}}",
  "mostrarNumeroPagina": true,
  "mostrarFechaGeneracion": true,
  "marcaDeAgua": ""
}
```

### Opción B — Via Seeder (recomendado para plantillas del sistema)

Abre el archivo:
```
src/Modules/PdfTemplates/CoreTemplate.Modules.PdfTemplates.Infrastructure/
  Persistence/PdfTemplatesDataSeeder.cs
```

Agrega una entrada al array `_plantillas`:

```csharp
private static readonly (string Codigo, string Nombre, string Modulo, string CodigoTemplate)[] _plantillas =
[
    // Plantillas existentes...
    ("sistema.vertical-estandar", "Layout Vertical Estándar", "Sistema", "vertical-estandar"),

    // ← Agrega tu nueva plantilla aquí
    ("modulo.nombre-documento", "Nombre descriptivo", "NombreModulo", "nombre-del-template"),
];
```

Luego ejecuta las migraciones si agregaste campos nuevos, o simplemente reinicia la API — el seeder la creará automáticamente.

---

## Paso 5 — Verificar en Swagger

```http
GET /api/pdf-templates/disenios
```

Deberías ver tu nuevo diseño en la lista:
```json
[
  { "codigo": "vertical-estandar", "nombre": "Vertical Estándar", "orientacion": "Vertical" },
  { "codigo": "moderno", "nombre": "Moderno con banda lateral", "orientacion": "Vertical" },
  { "codigo": "nombre-del-template", "nombre": "Nombre descriptivo del diseño", "orientacion": "Vertical" }
]
```

---

## Paso 6 — Generar vista previa

```http
POST /api/pdf-templates/{id}/preview
Authorization: Bearer {token}

{
  "datos": {
    "Campo1": "Valor de ejemplo 1",
    "Campo2": "Valor de ejemplo 2"
  }
}
```

El sistema retorna el PDF generado. Ábrelo en el navegador para verificar el diseño.

---

## Paso 7 — Usar el diseño desde un módulo

En el handler del módulo consumidor:

```csharp
public class GenerarComprobantePagoHandler(IPdfGenerator pdfGenerator)
{
    public async Task<byte[]> Handle(GenerarComprobanteCommand cmd, CancellationToken ct)
    {
        var contenido = new ComprobantePagoContent
        {
            NombreEmpleado = cmd.NombreEmpleado,
            Periodo = cmd.Periodo,
            SalarioNeto = cmd.SalarioNeto
        };

        // El código de plantilla apunta a la fila en BD
        // que a su vez apunta al diseño "nombre-del-template" en código
        return await pdfGenerator.GenerarAsync(
            "modulo.nombre-documento",
            currentTenant.TenantId,
            contenido,
            ct);
    }
}
```

---

## Checklist completo

```
□ Paso 1 — Clase NombreDelTemplate.cs creada en CoreTemplate.Pdf/Templates/
□ Paso 2 — Registrada en DependencyInjection.cs con AddSingleton
□ Paso 3 — Compila sin errores (dotnet build)
□ Paso 4 — Plantilla creada en BD (via API o Seeder)
□ Paso 5 — Aparece en GET /api/pdf-templates/disenios
□ Paso 6 — Vista previa generada y verificada visualmente
□ Paso 7 — Módulo consumidor usa el nuevo diseño correctamente
```

---

## Errores comunes y soluciones

### Error: "Diseño 'nombre-del-template' no encontrado"
**Causa:** Olvidaste registrar el template en DI (Paso 2) o el `Codigo` no coincide exactamente.
**Solución:** Verifica que el valor de `Codigo` en la clase sea idéntico al `CodigoTemplate` en BD.

### Error: El logo no aparece en el PDF
**Causa:** La URL del logo no es accesible desde el servidor, o la ruta local no existe.
**Solución:** Verifica que la URL sea accesible. Para storage local, usa la ruta física del archivo, no la URL HTTP.

### Error: El PDF se genera pero el diseño no es el esperado
**Causa:** La plantilla en BD apunta a otro `CodigoTemplate`.
**Solución:** Verifica con `GET /api/pdf-templates/{id}` que `codigoTemplate` tiene el valor correcto.

### Error: Compilación falla con "QuestPDF no encontrado"
**Causa:** El proyecto que usa QuestPDF no tiene la referencia al building block `CoreTemplate.Pdf`.
**Solución:** Agrega `<ProjectReference Include="...CoreTemplate.Pdf.csproj" />` al `.csproj` del proyecto.

---

## Consejos de diseño con QuestPDF

```csharp
// Usar EnsureSpace para evitar que una sección se corte entre páginas
column.Item().EnsureSpace(150).Column(inner => { ... });

// Salto de página explícito
column.Item().PageBreak();

// Imagen con alta resolución para impresión
row.Item().Image(rutaLocal).WithRasterDpi(300);

// Tabla con estilos consistentes
table.Cell().Element(CellStyle).Text("Contenido");
static IContainer CellStyle(IContainer c) =>
    c.Border(1).Padding(5).AlignCenter().AlignMiddle();

// Colores del sistema de diseño
Colors.White, Colors.Black
Colors.Grey.Lighten1, Colors.Grey.Darken1
// O colores hex de la plantilla:
plantilla.ColorEncabezado  // "#1a2e5a"
```

---

## Referencia rápida de archivos

| Qué hacer | Archivo a modificar |
|---|---|
| Crear nuevo diseño | `CoreTemplate.Pdf/Templates/NuevoTemplate.cs` (nuevo archivo) |
| Registrar diseño en DI | `CoreTemplate.Pdf/DependencyInjection.cs` |
| Agregar plantilla al seeder | `PdfTemplates.Infrastructure/Persistence/PdfTemplatesDataSeeder.cs` |
| Usar desde un módulo | Handler del módulo consumidor via `IPdfGenerator` |
| Ver diseños disponibles | `GET /api/pdf-templates/disenios` |
| Crear plantilla en BD | `POST /api/pdf-templates` |
| Vista previa | `POST /api/pdf-templates/{id}/preview` |

---

**Fecha:** 2026-04-22
**Versión:** 1.0
