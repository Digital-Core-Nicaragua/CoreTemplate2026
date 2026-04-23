# Event Storming — Big Picture
# Building Block: CoreTemplate.Storage + Módulo Archivos

> **Nivel:** Big Picture + Process Level
> **Fecha:** 2026-04-22
> **Nota:** Storage es infraestructura transversal. Los eventos de dominio propios
> pertenecen al módulo `Archivos` que gestiona los metadatos.

---

## Leyenda

| Símbolo | Color | Elemento |
|---|---|---|
| 🟠 | Naranja | Evento de dominio / infraestructura |
| 🔵 | Azul | Comando |
| 🟡 | Amarillo | Aggregate |
| 🟣 | Morado | Política |
| 🟢 | Verde | Read Model |
| 🔴 | Rojo | Hotspot |
| ⚡ | — | Evento externo (del módulo consumidor) |
| 👤 | — | Actor humano |
| 🤖 | — | Sistema / Módulo consumidor |

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| 👤 **Analista RRHH** | Humano | Sube CVs, documentos de candidatos y empleados |
| 👤 **Contador** | Humano | Sube facturas escaneadas, soportes |
| 👤 **Analista Nómina** | Humano | Genera y sube comprobantes de pago |
| 👤 **Empleado** | Humano | Descarga sus propios documentos |
| 🤖 **Módulo RRHH** | Sistema | Consume IStorageService para archivos de RRHH |
| 🤖 **Módulo Contabilidad** | Sistema | Consume IStorageService para documentos contables |
| 🤖 **Módulo Nómina** | Sistema | Consume IStorageService para comprobantes |
| 🌐 **AWS S3** | Sistema Externo | Almacenamiento en nube |
| 🌐 **Firebase Storage** | Sistema Externo | Almacenamiento alternativo |

---

## MÓDULO ARCHIVOS — Flujos de dominio

### Flujo: Subir archivo

```
👤 Usuario → 🔵 SubirArchivo { archivo, contexto, moduloOrigen, entidadId }
    🟡 ArchivoAdjunto → Validar tipo MIME permitido
    🟡 ArchivoAdjunto → Validar tamaño <= máximo configurado

    3a. SI validación falla:
        🟠 ArchivoRechazado { razon: "TipoNoPermitido" | "TamanioExcedido" }
        → Retornar 400

    3b. SI validación OK:
        IStorageService → Generar nombre único { guid + extension }
        IStorageService → Subir al proveedor configurado
        
        4a. SI fallo del proveedor:
            🟠 SubidaFallida { nombreOriginal, proveedor, error }
            → Retornar 500

        4b. SI subida exitosa:
            🟡 ArchivoAdjunto → Crear {
                NombreOriginal, NombreAlmacenado, RutaAlmacenada,
                Url, ContentType, TamanioBytes, Proveedor,
                Contexto, ModuloOrigen, EntidadId, SubidoPor
            }
            🟠 ArchivoSubido { archivoId, url, rutaAlmacenada, proveedor }
            → Retornar 201 { archivoId, url }
```

---

### Flujo: Obtener URL de acceso

```
👤 Usuario / 🤖 Módulo → 🔵 ObtenerUrlArchivo { archivoId }
    🟢 ConsultarArchivoAdjunto { archivoId }
    
    2a. SI no existe:
        🟠 ArchivoNoEncontrado
        → Retornar 404
    
    2b. SI existe:
        IStorageService → ObtenerUrlAsync(rutaAlmacenada)
        
        3a. SI proveedor Local:
            → URL estática: "https://misistema.com/archivos/rrhh/cv/{guid}.pdf"
        
        3b. SI proveedor S3:
            → URL firmada con expiración: "https://s3.amazonaws.com/...?X-Amz-Signature=..."
        
        3c. SI proveedor Firebase:
            → URL con token: "https://firebasestorage.googleapis.com/...?token=..."
        
        🟠 UrlArchivoGenerada { archivoId, url, expiraEn? }
        → Retornar 200 { url }
```

---

### Flujo: Eliminar archivo

```
👤 Administrador / 🤖 Módulo → 🔵 EliminarArchivo { archivoId }
    🟢 ConsultarArchivoAdjunto { archivoId }
    
    2a. SI no existe:
        → Retornar 404
    
    2b. SI existe:
        IStorageService → EliminarAsync(rutaAlmacenada)
        🟡 ArchivoAdjunto → Desactivar (soft delete)
        🟠 ArchivoEliminado { archivoId, rutaAlmacenada, proveedor }
        → Retornar 200
```

---

### Flujo: Listar archivos de una entidad

```
🤖 Módulo RRHH → 🔵 ListarArchivosEntidad { moduloOrigen: "RRHH", entidadId: candidatoId }
    🟢 ConsultarArchivosPorEntidad { moduloOrigen, entidadId }
    → Retornar lista de ArchivoAdjuntoDto []
```

---

## Integración con módulos consumidores

### RRHH → Storage

```
⚡ CandidatoCreado (evento de RRHH)
    → El analista puede subir CV, foto, documentos

👤 Analista RRHH → 🔵 SubirArchivo {
    contexto: "rrhh/candidatos/cv",
    moduloOrigen: "RRHH",
    entidadId: candidatoId
}
🟠 ArchivoSubido { archivoId, url }
🟣 POLÍTICA: Módulo RRHH guarda archivoId en Candidato.CvArchivoId
```

```
⚡ EmpleadoContratado (evento de RRHH)
    → Se pueden subir: contrato firmado, cédula, record policial

👤 Analista RRHH → 🔵 SubirArchivo {
    contexto: "rrhh/empleados/documentos",
    moduloOrigen: "RRHH",
    entidadId: empleadoId
}
🟠 ArchivoSubido
```

### Nómina → Storage

```
⚡ NominaCalculada (evento de Nómina)
    🟣 POLÍTICA: Generar PDF de comprobante por empleado

🤖 Módulo Nómina → 🔵 SubirArchivo {
    contexto: "nomina/comprobantes/2025/01",
    moduloOrigen: "Nomina",
    entidadId: nominaId
}
🟠 ArchivoSubido { archivoId, url }
🟣 POLÍTICA: Módulo Nómina guarda archivoId en Nomina.ComprobanteArchivoId
🟣 POLÍTICA: Enviar correo al empleado con URL del comprobante (via IEmailSender)
```

### Contabilidad → Storage

```
👤 Contador → 🔵 SubirArchivo {
    contexto: "contabilidad/facturas/2025",
    moduloOrigen: "Contabilidad",
    entidadId: facturaId
}
🟠 ArchivoSubido { archivoId, url }
🟣 POLÍTICA: Módulo Contabilidad guarda archivoId en Factura.DocumentoArchivoId
```

---

## Políticas Automáticas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Log de toda operación | `ArchivoSubido`, `ArchivoEliminado`, `SubidaFallida` | Registrar en log estructurado |
| P2 | Nombre único siempre | `SubirArchivo` | Generar GUID como nombre de almacenamiento |
| P3 | No propagar excepciones | Error del proveedor | Encapsular en `StorageResult` |
| P4 | Fail-fast en configuración | Proveedor inválido al iniciar | No iniciar la aplicación |
| P5 | Soft delete en BD | `EliminarArchivo` | Marcar `EsActivo = false`, no borrar registro |

---

## Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Las URLs de S3 expiran? ¿Cómo manejar eso? | URL firmada con 1h. Para URLs permanentes usar bucket público o CloudFront. |
| H2 | ¿Qué pasa si se cambia de proveedor con archivos existentes? | Los archivos viejos quedan en el proveedor anterior. Migración manual o script. |
| H3 | ¿Control de acceso por archivo? ¿Solo el dueño puede ver su CV? | En v1: acceso por autenticación del sistema. En v2: permisos por archivo en módulo Archivos. |
| H4 | ¿Virus scanning antes de almacenar? | No en v1. Se puede agregar como decorador de IStorageService sin cambiar consumidores. |
| H5 | ¿Compresión de imágenes antes de subir? | No en v1. Se puede agregar como decorador. |
| H6 | ¿Límite de almacenamiento por tenant? | No en v1. Preparado para agregar en StorageSettings. |
| H7 | ¿Cómo sirve archivos el proveedor Local en producción? | Via endpoint autenticado en ArchivosController o middleware de archivos estáticos. |

---

## Eventos de dominio del módulo Archivos

| Evento | Trigger | Datos |
|---|---|---|
| `ArchivoSubido` | Subida exitosa | archivoId, url, rutaAlmacenada, proveedor, tamanio |
| `ArchivoRechazado` | Validación fallida | nombreOriginal, razon |
| `SubidaFallida` | Error del proveedor | nombreOriginal, proveedor, error |
| `UrlArchivoGenerada` | Solicitud de URL | archivoId, url, expiraEn |
| `ArchivoEliminado` | Eliminación exitosa | archivoId, rutaAlmacenada |

---

**Estado:** Documentado
**Fecha:** 2026-04-22

---

## Addendum — Multi-tenant

> **Fecha:** 2026-04-22

### Aislamiento de archivos por tenant

```
👤 Analista RRHH (Tenant A) → 🔵 SubirArchivo { cv.pdf, contexto: "rrhh/candidatos/cv" }
    SubirArchivoHandler → ICurrentTenant.TenantId = tenant-A
    ArchivoAdjunto.Crear(..., tenantId: tenant-A)
    🟠 ArchivoSubido { archivoId, tenantId: tenant-A }

👤 Analista RRHH (Tenant B) → 🔵 ListarArchivos { moduloOrigen: "RRHH" }
    BaseDbContext QueryFilter → WHERE TenantId = tenant-B
    → Solo ve archivos de Tenant B
    → Archivos de Tenant A son invisibles
```

### Política de aislamiento

```
🟣 POLÍTICA: QueryFilter automático de BaseDbContext
    CUANDO IsMultiTenant = true
    ENTONCES filtrar ArchivoAdjunto por TenantId = currentTenant.TenantId
    → Garantiza que cada empresa solo ve sus propios archivos
```

### Hotspot resuelto

| # | Hotspot | Resolución |
|---|---|---|
| H8 | ¿Cómo garantizar que un tenant no vea archivos de otro? | `ArchivoAdjunto` implementa `IHasTenant`. El `BaseDbContext` aplica el QueryFilter automáticamente. No requiere lógica adicional en los handlers. |

---

**Fecha addendum:** 2026-04-22
