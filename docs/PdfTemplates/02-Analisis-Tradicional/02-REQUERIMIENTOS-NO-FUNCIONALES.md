# PdfTemplates — Requerimientos No Funcionales

> **Fecha:** 2026-04-22

---

## RNF-PDF-001: Extensibilidad de diseños
**Categoría:** Mantenibilidad

- Agregar un nuevo diseño requiere solo:
  1. Crear una clase que implemente `IPdfDocumentTemplate`
  2. Registrarla en DI con `services.AddSingleton<IPdfDocumentTemplate, NuevoTemplate>()`
  3. Crear una fila en BD apuntando al nuevo `CodigoTemplate`
- No se modifica ningún código existente (principio Open/Closed)
- Ver guía completa: `03-Guias/01-AGREGAR-NUEVO-DISENIO.md`

---

## RNF-PDF-002: Separación diseño / configuración
**Categoría:** Mantenibilidad

- El diseño (estructura visual) vive en código C# — versionado con Git
- La configuración corporativa (logo, colores) vive en BD — editable sin redeployar
- Un cambio de logo o color no requiere redeployar la aplicación
- Un cambio de estructura del documento (nueva sección, nueva tabla) sí requiere código

---

## RNF-PDF-003: Rendimiento de generación
**Categoría:** Rendimiento

- La generación de PDF es síncrona internamente (QuestPDF no es async)
- El endpoint que genera el PDF debe ser async para no bloquear el thread pool
- Para PDFs grandes o en lote → usar background jobs (Hangfire, etc.) en el futuro
- Tiempo esperado de generación: < 500ms para documentos de 1-5 páginas

---

## RNF-PDF-004: Calidad de imagen
**Categoría:** Calidad

- Los logos se renderizan con `WithRasterDpi(300)` para alta calidad de impresión
- Las imágenes se cargan desde ruta local (Storage Local) o URL (S3/Firebase)
- Si el logo no está disponible → el diseño continúa sin logo (no falla)

---

## RNF-PDF-005: Multi-tenant
**Categoría:** Seguridad / Aislamiento

- `PdfPlantilla` implementa `IHasTenant` — aislamiento automático por QueryFilter
- Cada tenant solo ve y usa sus propias plantillas
- Las plantillas globales del sistema son visibles para todos los tenants
- Un tenant no puede usar la configuración corporativa de otro tenant

---

## RNF-PDF-006: Licencia QuestPDF
**Categoría:** Legal

- QuestPDF Community License es gratuita para proyectos con ingresos < $1M USD/año
- Para proyectos comerciales grandes → adquirir licencia Professional o Enterprise
- Configurar en Program.cs: `QuestPDF.Settings.License = LicenseType.Community`
- Documentación: https://www.questpdf.com/license/

---

## RNF-PDF-007: Fail-fast en configuración
**Categoría:** Operabilidad

- Si un `CodigoTemplate` referenciado en BD no está registrado en DI → error al iniciar
- Esto evita errores en runtime al intentar generar un PDF con diseño inexistente
- El `PdfTemplateFactory` valida todos los diseños registrados al arrancar

---

**Fecha:** 2026-04-22
