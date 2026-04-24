# PdfTemplates — Documentación

> **Tipo:** Building Block (`CoreTemplate.Pdf`) + Módulo de negocio (`CoreTemplate.Modules.PdfTemplates`)
> **Proyecto:** CoreTemplate

---

## Estructura de Documentos

| Documento | Descripción |
|---|---|
| **01-EventStorming/01-BIG-PICTURE.md** | Flujos de dominio, integraciones con módulos, políticas, hotspots |
| **02-Analisis-Tradicional/01-REQUERIMIENTOS-FUNCIONALES.md** | RF con criterios de aceptación |
| **02-Analisis-Tradicional/02-REQUERIMIENTOS-NO-FUNCIONALES.md** | Rendimiento, extensibilidad, multi-tenant |
| **02-Analisis-Tradicional/03-CASOS-DE-USO.md** | CU por módulo consumidor |
| **02-Analisis-Tradicional/04-MODELO-DOMINIO-Y-CONTRATOS.md** | Aggregate, contratos, BD, estructura de proyectos |
| **03-Guias/01-AGREGAR-NUEVO-DISENIO.md** | Guía paso a paso para crear un nuevo diseño de PDF |

---

## Resumen

Dos componentes que trabajan juntos:

### CoreTemplate.Pdf (Building Block)
Infraestructura pura — genera bytes de PDF usando QuestPDF.
Sin base de datos. Sin aggregates.
Contiene los **diseños** (templates) en código C#.

**Diseños incluidos desde el inicio:**
- `vertical-estandar` — A4 vertical, encabezado con logo, tabla de contenido, pie de página
- `horizontal-estandar` — A4 horizontal, ideal para reportes con muchas columnas
- `compacto` — A4 vertical sin márgenes grandes, ideal para recibos
- `moderno` — A4 vertical con banda lateral de color corporativo

**Agregar nuevo diseño:** crear clase + registrar en DI + crear fila en BD. Ver `03-Guias/01-AGREGAR-NUEVO-DISENIO.md`.

### Módulo PdfTemplates
Gestiona la configuración corporativa de cada plantilla en base de datos.
Cada tenant puede tener su propia versión con su logo, colores y datos.

---

## Cómo funciona la separación diseño / configuración

```
CÓDIGO C# (diseños — estructura visual)
  vertical-estandar  → dónde va el logo, cómo se ven las tablas, estilo del encabezado
  moderno            → banda lateral de color, tipografía diferente

BD (configuración — datos corporativos por tenant)
  PdfPlantilla {
    codigoTemplate: "vertical-estandar",   ← apunta al diseño en código
    nombreEmpresa:  "Empresa ABC",
    logoUrl:        "https://...",
    colorEncabezado: "#1a2e5a"
  }
```

---

## Módulos consumidores

| Módulo | Plantillas | Uso |
|---|---|---|
| Nómina | `nomina.comprobante-pago` | Comprobante mensual por empleado |
| Contabilidad | `contabilidad.factura`, `contabilidad.recibo` | Documentos fiscales |
| RRHH | `rrhh.contrato`, `rrhh.constancia-laboral` | Documentos de personal |

---

## Relación con otros building blocks

```
Módulo Nómina
    ↓ usa
IPdfGenerator (CoreTemplate.Pdf)
    ↓ combina
PdfPlantilla (BD) + ComprobantePagoData (datos del negocio)
    ↓ genera
byte[] PDF
    ↓ sube via
IStorageService (CoreTemplate.Storage)
    ↓ envía via
IEmailTemplateSender (Módulo EmailTemplates)
```

---

**Estado:** Documentado — pendiente implementación
**Fecha:** 2026-04-22
