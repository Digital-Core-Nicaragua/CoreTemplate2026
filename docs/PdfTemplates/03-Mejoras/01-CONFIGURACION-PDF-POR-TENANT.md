# Plan de Implementación: Configuración de PDF por Tenant

> **Módulo:** CoreTemplate.Modules.PdfTemplates
> **Esfuerzo estimado:** 2-3 días
> **Fecha:** 2026-04-22
> **Estado:** Pendiente

---

## ¿Por qué?

Hoy si el Tenant A no crea su propia `PdfPlantilla`, todos sus PDFs salen
con los datos de "Mi Sistema" (la plantilla global del seed).

Para que cada empresa tenga su marca en todos los PDFs automáticamente,
sin necesidad de crear una plantilla por cada tipo de documento,
se agrega una tabla de configuración corporativa por tenant.

---

## Diferencia con PdfPlantilla existente

| | PdfPlantilla (existente) | ConfiguracionPdfTenant (nueva) |
|---|---|---|
| Granularidad | Por tipo de documento | Por tenant (aplica a todos) |
| Cuándo usar | Personalizar un documento específico | Configurar la marca corporativa una vez |
| Jerarquía | Tenant → Global | Se aplica como valores por defecto |

**Jerarquía de resolución final:**
```
1. PdfPlantilla del tenant (más específico)
2. ConfiguracionPdfTenant → sobreescribe valores vacíos de la plantilla global
3. PdfPlantilla global del sistema (fallback)
```

---

## Aggregate: ConfiguracionPdfTenant

```
ConfiguracionPdfTenant (AggregateRoot)
  + Id                  : Guid
  + TenantId            : Guid          (requerido, único)
  + NombreEmpresa       : string
  + LogoUrl             : string?
  + ColorEncabezado     : string        "#1a2e5a"
  + ColorTextoHeader    : string        "#ffffff"
  + ColorAcento         : string        "#4f46e5"
  + TextoSecundario     : string?       "RUC: 001-000000-0000"
  + TextoPiePagina      : string?
  + MostrarNumeroPagina : bool
  + MostrarFechaGeneracion : bool
  + CreadoEn            : DateTime
  + ModificadoEn        : DateTime?
```

---

## Cambio en ModuloPdfGenerator

```csharp
// Al resolver la plantilla, si es la global, aplicar config del tenant encima
var plantilla = await repo.ObtenerPorCodigoAsync(codigo, tenantId)
             ?? await repo.ObtenerPorCodigoAsync(codigo, null);

// NUEVO: si la plantilla es global, aplicar config corporativa del tenant
if (plantilla.TenantId is null && tenantId is not null)
{
    var configTenant = await configRepo.ObtenerAsync(tenantId.Value, ct);
    if (configTenant is not null)
        plantilla = plantilla.AplicarConfigTenant(configTenant);
}
```

---

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/pdf-templates/configuracion-tenant` | Ver config del tenant actual |
| POST | `/api/pdf-templates/configuracion-tenant` | Crear/actualizar config |
| DELETE | `/api/pdf-templates/configuracion-tenant` | Eliminar (vuelve a usar global) |

---

## Plan de implementación

### Fase 1 — Domain (Día 1)
```
□ Aggregate ConfiguracionPdfTenant
□ IConfiguracionPdfTenantRepository
□ Método PdfPlantilla.AplicarConfigTenant(config)
```

### Fase 2 — Infrastructure (Día 1-2)
```
□ Agregar ConfiguracionPdfTenant al PdfTemplatesDbContext
□ Migración: Add_ConfiguracionPdfTenant
□ ConfiguracionPdfTenantRepository
□ Modificar ModuloPdfGenerator para aplicar config del tenant
```

### Fase 3 — Application y API (Día 2-3)
```
□ Commands: CrearConfiguracionPdfTenant, ActualizarConfiguracionPdfTenant
□ Query: GetConfiguracionPdfTenant
□ Endpoint en PdfTemplatesController
```

---

**Fecha:** 2026-04-22
