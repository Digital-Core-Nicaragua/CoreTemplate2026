# PdfTemplates — Casos de Uso

> **Fecha:** 2026-04-22

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| **Administrador** | Humano | Gestiona plantillas y configuración corporativa |
| **Módulo Consumidor** | Sistema | Nómina, Contabilidad, RRHH — generan PDFs |
| **Sistema (Seeder)** | Automático | Crea plantillas del sistema al arrancar |

---

## CU-PDF-001: Configurar marca corporativa de una plantilla

**Actor:** Administrador
**Permiso:** `PdfTemplates.Editar`

**Flujo:**
1. Admin abre la plantilla `nomina.comprobante-pago`
2. Actualiza:
   - `LogoUrl`: URL del logo subido via módulo Archivos
   - `NombreEmpresa`: "Empresa ABC S.A."
   - `ColorEncabezado`: "#1a2e5a"
   - `TextoSecundario`: "RUC: 001-123456-0001 | Tel: 2222-3333"
   - `TextoPiePagina`: "{{NombreEmpresa}} — Documento generado el {{FechaGeneracion}}"
3. Guarda → `PlantillaPdfActualizada`
4. Desde ese momento todos los comprobantes de nómina usan la nueva marca

---

## CU-PDF-002: Cambiar el diseño de una plantilla

**Actor:** Administrador
**Permiso:** `PdfTemplates.Gestionar`

**Flujo:**
1. Admin consulta los diseños disponibles: `GET /api/pdf-templates/disenios`
   ```json
   [
     { "codigo": "vertical-estandar", "nombre": "Vertical Estándar", "orientacion": "Vertical" },
     { "codigo": "moderno", "nombre": "Moderno con banda lateral", "orientacion": "Vertical" },
     { "codigo": "compacto", "nombre": "Compacto", "orientacion": "Vertical" }
   ]
   ```
2. Edita la plantilla `rrhh.contrato` cambiando `CodigoTemplate` de `vertical-estandar` a `moderno`
3. Genera una vista previa para verificar el resultado
4. Confirma el cambio

---

## CU-PDF-003: Vista previa de plantilla

**Actor:** Administrador
**Permiso:** `PdfTemplates.Preview`

**Flujo:**
1. Admin abre la plantilla `nomina.comprobante-pago`
2. Hace clic en "Vista previa"
3. Envía datos de ejemplo:
   ```json
   {
     "NombreEmpleado": "Juan Pérez",
     "Cargo": "Desarrollador Senior",
     "Periodo": "Enero 2025",
     "SalarioBruto": "3000.00",
     "Deducciones": "450.00",
     "SalarioNeto": "2550.00"
   }
   ```
4. El sistema genera el PDF con la configuración corporativa actual + datos de ejemplo
5. Retorna el PDF para visualizar en el navegador (`application/pdf`)
6. No se guarda ningún archivo

---

## CU-PDF-004: Módulo Nómina genera comprobante de pago

**Actor:** Módulo Nómina (automático)
**Precondición:** Plantilla `nomina.comprobante-pago` activa

**Flujo:**
```
1. NominaHandler calcula el pago del empleado
2. Llama IPdfGenerator.GenerarAsync(
       codigo: "nomina.comprobante-pago",
       tenantId: currentTenant.TenantId,
       datos: new ComprobantePagoData {
           NombreEmpleado: "Juan Pérez",
           Periodo: "Enero 2025",
           SalarioBruto: 3000,
           Deducciones: 450,
           SalarioNeto: 2550
       })

3. IPdfGenerator resuelve:
   a. PdfPlantilla de BD (tenant → global)
   b. Diseño en código: VerticalEstandarTemplate
   c. Genera PDF combinando ambos

4. Sube PDF via IStorageService:
   contexto: "nomina/comprobantes/2025/01"
   → retorna URL

5. Envía correo via IEmailTemplateSender:
   codigo: "nomina.comprobante-pago"
   adjuntos: [pdfBytes]

6. Guarda ArchivoId en la entidad Nomina
```

---

## CU-PDF-005: Tenant personaliza su plantilla

**Actor:** Administrador del Tenant
**Precondición:** Sistema en modo multi-tenant

**Flujo:**
1. Admin del Tenant A crea su versión de `nomina.comprobante-pago`:
   ```http
   POST /api/pdf-templates
   {
     "codigo": "nomina.comprobante-pago",
     "codigoTemplate": "moderno",
     "nombreEmpresa": "Empresa ABC S.A.",
     "logoUrl": "https://s3.../tenant-a/logo.png",
     "colorEncabezado": "#1a2e5a"
   }
   ```
   → Se crea con `TenantId = tenant-A`

2. Desde ese momento, cuando Nómina del Tenant A genera un comprobante:
   - Usa el diseño "moderno" con el logo de Empresa ABC
   - El Tenant B sigue usando la plantilla global con el diseño "vertical-estandar"

---

## CU-PDF-006: Agregar nuevo diseño al sistema

**Actor:** Desarrollador
**Ver guía completa:** `03-Guias/01-AGREGAR-NUEVO-DISENIO.md`

**Resumen del flujo:**
1. Crear clase `NuevoTemplate : IPdfDocumentTemplate` en `CoreTemplate.Pdf/Templates/`
2. Registrar en DI: `services.AddSingleton<IPdfDocumentTemplate, NuevoTemplate>()`
3. Crear plantilla en BD via API o seeder apuntando al nuevo `CodigoTemplate`
4. Cualquier tenant puede usar el nuevo diseño desde ese momento

---

## Flujo completo: Nómina → PDF → Storage → Email

```
NominaCalculadaEvent
    ↓
NominaHandler
    ├── IPdfGenerator.GenerarAsync("nomina.comprobante-pago", datos)
    │       ├── PdfPlantillaRepository → obtiene config corporativa
    │       ├── PdfTemplateFactory → resuelve VerticalEstandarTemplate
    │       └── template.Generar(plantilla, datos) → byte[]
    │
    ├── IStorageService.SubirAsync(pdfStream, "nomina/comprobantes/2025/01")
    │       └── retorna URL
    │
    └── IEmailTemplateSender.EnviarAsync(
            "nomina.comprobante-pago",
            empleado.Email,
            variables,
            adjuntos: [pdfBytes]
        )
```

---

**Fecha:** 2026-04-22
