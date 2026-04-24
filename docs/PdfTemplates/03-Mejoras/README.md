# PdfTemplates — Mejoras Futuras

> **Fecha:** 2026-04-22

---

## 01 — Configuración de PDF por Tenant

> **Documento:** `01-CONFIGURACION-PDF-POR-TENANT.md` (ver abajo)
> **Estado:** Pendiente
> **Esfuerzo:** 2-3 días

Similar a la configuración de email por tenant. Hoy cada tenant puede tener
su propia `PdfPlantilla` en BD, pero si no la crea, usa la plantilla global
con los datos de "Mi Sistema".

La mejora agrega una tabla `ConfiguracionPdfTenant` con valores corporativos
por defecto que se aplican automáticamente a todas las plantillas del tenant,
sin necesidad de crear una plantilla propia para cada documento.

**Ver documento completo:** `01-CONFIGURACION-PDF-POR-TENANT.md`

---

## 02 — Templates en múltiples idiomas

> **Estado:** Pendiente
> **Esfuerzo:** 1-2 días

Agregar campo `Idioma` al aggregate `PdfPlantilla`.
Al generar un PDF, resolver por: tenant + idioma → tenant + default → global + idioma → global.

```
PdfPlantilla
  + Idioma : string?   "es" | "en" | null (cualquier idioma)
```

Útil para empresas que generan documentos en múltiples idiomas.

---

## 03 — Imágenes en el contenido del PDF

> **Estado:** Pendiente
> **Esfuerzo:** 1 día

Hoy `IPdfContent` solo soporta `Dictionary<string, object>` con texto.
La mejora agrega soporte para imágenes en el contenido:

```csharp
public interface IPdfContent
{
    Dictionary<string, object> ObtenerDatos();
    IEnumerable<PdfImagen>? ObtenerImagenes();  // NUEVO
}

public record PdfImagen(string Clave, string RutaOUrl, int AnchoMax = 200);
```

Útil para: reportes de RRHH con foto del empleado, reportes de inspección con fotos
(como en Rancho Santana), facturas con imagen del producto.

---

## 04 — Generación en lote (batch)

> **Estado:** Pendiente
> **Esfuerzo:** 2-3 días

Generar múltiples PDFs en un solo proceso. Útil para:
- Nómina: generar comprobantes de todos los empleados de una vez
- Contabilidad: generar todas las facturas del mes

```csharp
public interface IModuloPdfGenerator
{
    // Existente
    Task<byte[]> GenerarAsync(string codigo, Guid? tenantId, IPdfContent contenido, CancellationToken ct);

    // NUEVO — genera múltiples PDFs y los retorna como ZIP
    Task<byte[]> GenerarLoteAsync(
        string codigo, Guid? tenantId,
        IEnumerable<IPdfContent> contenidos,
        CancellationToken ct);
}
```

Implementación: usar background job (Hangfire) para lotes grandes.

---

## 05 — Firma digital de PDFs

> **Estado:** Pendiente (requiere investigación)
> **Esfuerzo:** 3-5 días

Agregar firma digital a documentos legales (contratos, constancias).
Opciones: iTextSharp (licencia AGPL), PdfPig (MIT), servicio externo.

Requiere decisión de arquitectura antes de implementar.

---

## 06 — Historial de PDFs generados

> **Estado:** Pendiente
> **Esfuerzo:** 1 día

Registrar cada PDF generado en una tabla de historial:

```
PdfGenerado
  + Id, TenantId, CodigoPlantilla, UsuarioId
  + ArchivoId (FK a Archivos.Archivos si se subió)
  + TamanioBytes, GeneradoEn
```

Útil para auditoría y para evitar regenerar documentos ya existentes.
