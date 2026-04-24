# Event Storming — Big Picture
# Building Block: CoreTemplate.Pdf + Módulo PdfTemplates

> **Nivel:** Big Picture + Process Level
> **Fecha:** 2026-04-22

---

## Leyenda

| Símbolo | Color | Elemento |
|---|---|---|
| 🟠 | Naranja | Evento de dominio |
| 🔵 | Azul | Comando |
| 🟡 | Amarillo | Aggregate |
| 🟣 | Morado | Política |
| 🟢 | Verde | Read Model |
| 🔴 | Rojo | Hotspot |
| ⚡ | — | Evento externo |
| 👤 | — | Actor humano |
| 🤖 | — | Sistema automático |

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| 👤 **Administrador** | Humano | Configura plantillas y marca corporativa |
| 🤖 **Seeder** | Automático | Crea plantillas del sistema al arrancar |
| 🤖 **Módulo Nómina** | Sistema | Genera comprobantes de pago |
| 🤖 **Módulo Contabilidad** | Sistema | Genera facturas y recibos |
| 🤖 **Módulo RRHH** | Sistema | Genera contratos y constancias |
| 👨‍💻 **Desarrollador** | Humano | Agrega nuevos diseños en código |

---

## Flujo: Seed inicial de plantillas

```
🤖 Seeder → 🔵 SeedPlantillasPdf
    Por cada plantilla del sistema:

    ¿Existe en BD?
    2a. SÍ → no sobreescribir (admin puede haberla editado)
    2b. NO:
        🟡 PdfPlantilla → Crear con valores por defecto
        🟠 PlantillaPdfCreada { codigo, modulo, esDeSistema: true }
        → Guardar en BD
```

---

## Flujo: Administrador configura marca corporativa

```
👤 Administrador → 🔵 ActualizarPlantillaPdf {
    id,
    nombreEmpresa: "Empresa ABC",
    logoUrl: "https://s3.../logo.png",
    colorEncabezado: "#1a2e5a"
}
    🟡 PdfPlantilla → Actualizar(...)
    🟠 PlantillaPdfActualizada { id, codigo, modificadoPor }
    → Retornar 200

Desde ese momento:
→ Todos los PDFs generados con esa plantilla usan la nueva marca
→ Sin redeployar la aplicación
```

---

## Flujo: Administrador cambia el diseño

```
👤 Administrador → 🔵 ActualizarPlantillaPdf {
    codigoTemplate: "moderno"   ← cambia de "vertical-estandar" a "moderno"
}
    🟡 PdfPlantilla → Actualizar(codigoTemplate: "moderno")

    ¿Existe el diseño "moderno" registrado en DI?
    2a. NO → 🟠 DisenioNoEncontrado → Error 400
    2b. SÍ:
        🟠 PlantillaPdfActualizada
        → Retornar 200
```

---

## Flujo: Módulo consumidor genera PDF

```
🤖 Módulo Nómina → 🔵 GenerarPdf {
    codigo: "nomina.comprobante-pago",
    datos: ComprobantePagoContent { NombreEmpleado, Periodo, SalarioNeto... }
}

IPdfGenerator:
    1. 🟢 ConsultarPdfPlantilla { codigo, tenantId }
       ¿Existe plantilla del tenant?
       2a. SÍ → usar plantilla del tenant
       2b. NO → usar plantilla global (IgnoreQueryFilters)
       2c. NO existe ninguna → 🟠 PlantillaNoEncontrada → Error

    2. PdfTemplateFactory → resolver diseño por CodigoTemplate
       ¿Existe el diseño en DI?
       3a. NO → 🟠 DisenioNoRegistrado → Error (no debería pasar si seeder corrió)
       3b. SÍ → obtener instancia del template

    3. template.Generar(plantillaData, contenido)
       → QuestPDF genera el PDF
       🟠 PdfGenerado { codigo, tamanioBytes, tenantId }
       → Retornar byte[]
```

---

## Flujo: Nómina genera, sube y envía comprobante

```
⚡ NominaCalculadaEvent { empleadoId, periodo, salarioNeto }
    🟣 POLÍTICA: Generar comprobante de pago

    🤖 NominaHandler:

    Paso 1 — Generar PDF
    🔵 GenerarPdf { "nomina.comprobante-pago", ComprobantePagoContent }
    🟠 PdfGenerado { bytes }

    Paso 2 — Subir a Storage
    🔵 SubirArchivo {
        contenido: pdfStream,
        contexto: "nomina/comprobantes/2025/01",
        moduloOrigen: "Nomina"
    }
    🟠 ArchivoSubido { url, rutaAlmacenada }

    Paso 3 — Enviar por correo
    🔵 EnviarConPlantilla {
        codigo: "nomina.comprobante-pago",
        para: empleado.Email,
        adjuntos: [pdfBytes]
    }
    🟠 CorreoEnviado

    Paso 4 — Guardar referencia
    🟡 Nomina → AsignarComprobanteArchivoId(archivoId)
    🟠 ComprobantePdfAsignado
```

---

## Flujo: Desarrollador agrega nuevo diseño

```
👨‍💻 Desarrollador:
    1. Crea NuevoTemplate.cs en CoreTemplate.Pdf/Templates/
    2. Registra en DI: services.AddSingleton<IPdfDocumentTemplate, NuevoTemplate>()
    3. Redeploya la aplicación

🤖 Sistema al iniciar:
    PdfTemplateFactory → registra "nuevo-codigo" como diseño disponible
    🟠 DisenioRegistrado { codigo: "nuevo-codigo" }

👤 Administrador:
    4. Crea plantilla en BD apuntando al nuevo diseño:
       POST /api/pdf-templates { codigoTemplate: "nuevo-codigo", ... }
    🟠 PlantillaPdfCreada

→ Cualquier tenant puede usar el nuevo diseño desde ese momento
```

---

## Políticas Automáticas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Seed al arrancar | Aplicación inicia | Crear plantillas del sistema si no existen |
| P2 | Fail-fast diseño | Diseño no registrado en DI | No iniciar la aplicación |
| P3 | Tenant → Global | Plantilla del tenant no existe | Usar plantilla global |
| P4 | Variables en textos | Generar PDF | Reemplazar {{SistemaNombre}}, {{FechaGeneracion}}, etc. |
| P5 | Logo opcional | LogoUrl vacío o inaccesible | Continuar sin logo, no fallar |

---

## Eventos de Dominio

| Evento | Trigger | Datos |
|---|---|---|
| `PlantillaPdfCreada` | Crear plantilla | id, codigo, modulo |
| `PlantillaPdfActualizada` | Editar plantilla | id, codigo, modificadoPor |
| `PlantillaPdfActivada` | Activar | id, codigo |
| `PlantillaPdfDesactivada` | Desactivar | id, codigo |
| `PdfGenerado` | Generación exitosa | codigo, tamanioBytes, tenantId |

---

## Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Qué pasa si el logo no carga? | El diseño continúa sin logo. Log warning. No falla. |
| H2 | ¿PDFs grandes bloquean el servidor? | Para lotes → background jobs en el futuro. |
| H3 | ¿Cómo previsualizar sin datos reales? | El endpoint preview acepta datos de ejemplo en el body. |
| H4 | ¿Licencia QuestPDF en producción? | Community es gratis < $1M USD/año. Ver RNF-PDF-006. |
| H5 | ¿Cómo manejar múltiples páginas? | QuestPDF lo maneja automáticamente con `EnsureSpace`. |
| H6 | ¿El diseño puede tener imágenes del negocio (fotos)? | Sí — `IPdfContent` puede incluir rutas de imágenes. |

---

**Estado:** Documentado
**Fecha:** 2026-04-22
