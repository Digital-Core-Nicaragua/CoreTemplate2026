# Storage — Casos de Uso

> **Building Block:** CoreTemplate.Storage
> **Fecha:** 2026-04-22

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| **Módulo Consumidor** | Sistema | Cualquier módulo que inyecta `IStorageService` |
| **Usuario Final** | Humano | Persona que sube o descarga archivos via UI |
| **AWS S3** | Sistema Externo | Proveedor de almacenamiento en nube |
| **Firebase Storage** | Sistema Externo | Proveedor alternativo en nube |
| **Sistema de Archivos** | Sistema Local | Disco del servidor (desarrollo/producción simple) |

---

## CU-STORAGE-001: Subir archivo

**Actor:** Módulo Consumidor (via handler de comando)
**Precondición:** `IStorageService` registrado en DI, proveedor configurado

**Flujo principal:**
1. El usuario sube un archivo via endpoint del módulo (ej: `POST /api/rrhh/candidatos/{id}/cv`)
2. El handler recibe el `IFormFile`
3. Construye `SubirArchivoRequest`:
   - `NombreOriginal`: "cv-juan-perez.pdf"
   - `Contexto`: "rrhh/candidatos/cv"
   - `ContentType`: "application/pdf"
4. Llama a `IStorageService.SubirAsync(request)`
5. El building block valida tipo y tamaño
6. Genera nombre único: `{guid}.pdf`
7. Sube al proveedor configurado en la ruta: `rrhh/candidatos/cv/{guid}.pdf`
8. Retorna `StorageResult { Exitoso = true, Url, RutaAlmacenada, TamanioBytes }`
9. El módulo guarda `RutaAlmacenada` en su entidad de dominio

**Flujo alternativo — tipo no permitido:**
- Paso 5: tipo MIME no está en la lista permitida
- Retorna `StorageResult { Exitoso = false, Error = "TipoNoPermitido" }`
- El handler retorna 400 al cliente

**Flujo alternativo — tamaño excedido:**
- Paso 5: archivo supera el límite configurado
- Retorna `StorageResult { Exitoso = false, Error = "TamanioExcedido" }`

---

## CU-STORAGE-002: Obtener URL para visualizar o descargar

**Actor:** Módulo Consumidor
**Precondición:** El archivo fue subido previamente, `RutaAlmacenada` conocida

**Flujo principal:**
1. El usuario solicita ver/descargar un archivo (ej: `GET /api/rrhh/candidatos/{id}/cv`)
2. El handler obtiene la `RutaAlmacenada` desde su entidad de dominio
3. Llama a `IStorageService.ObtenerUrlAsync(rutaAlmacenada)`
4. El building block genera la URL según el proveedor:
   - Local: `https://misistema.com/archivos/rrhh/candidatos/cv/{guid}.pdf`
   - S3: URL firmada con expiración de 1 hora
   - Firebase: URL con token de descarga
5. Retorna la URL al módulo
6. El módulo retorna la URL al cliente para que abra/descargue el archivo

---

## CU-STORAGE-003: Eliminar archivo

**Actor:** Módulo Consumidor
**Precondición:** El archivo existe, `RutaAlmacenada` conocida

**Flujo principal:**
1. El módulo necesita eliminar un archivo (ej: empleado eliminado, documento reemplazado)
2. Llama a `IStorageService.EliminarAsync(rutaAlmacenada)`
3. El building block elimina el archivo del proveedor
4. Registra la eliminación en log
5. El módulo elimina la referencia de su entidad de dominio

**Flujo alternativo — archivo no existe:**
- El proveedor no encuentra el archivo
- La operación es idempotente — no retorna error
- Registra warning en log

---

## CU-STORAGE-004: Cambiar proveedor de almacenamiento

**Actor:** Administrador del sistema
**Precondición:** Nuevo proveedor configurado en appsettings

**Flujo principal:**
1. Administrador modifica `appsettings.json`: `"Provider": "S3"`
2. Configura credenciales del nuevo proveedor
3. Reinicia la aplicación
4. El building block registra la nueva implementación automáticamente
5. Nuevas subidas van al nuevo proveedor
6. Archivos existentes permanecen en el proveedor anterior (migración manual si se requiere)

---

## Casos de uso por módulo

### RRHH

| Operación | Contexto | Tipos permitidos |
|---|---|---|
| Subir CV | `rrhh/candidatos/cv` | PDF, DOC, DOCX |
| Subir foto de empleado | `rrhh/empleados/fotos` | JPEG, PNG, WEBP |
| Subir cédula/documento | `rrhh/empleados/documentos` | PDF, JPEG, PNG |
| Subir contrato firmado | `rrhh/contratos` | PDF |
| Subir record policial | `rrhh/empleados/records` | PDF, JPEG, PNG |

### Contabilidad

| Operación | Contexto | Tipos permitidos |
|---|---|---|
| Subir factura escaneada | `contabilidad/facturas/{año}` | PDF, JPEG, PNG |
| Subir soporte de pago | `contabilidad/soportes/{año}` | PDF, JPEG, PNG |
| Subir comprobante | `contabilidad/comprobantes/{año}` | PDF |

### Nómina

| Operación | Contexto | Tipos permitidos |
|---|---|---|
| Subir comprobante generado | `nomina/comprobantes/{año}/{mes}` | PDF |

---

## Flujo completo: RRHH sube CV de candidato

```
Usuario (RRHH) → POST /api/rrhh/candidatos/{id}/cv
    [IFormFile: cv-juan-perez.pdf, 245KB]

SubirCvCandidatoHandler
    → IStorageService.SubirAsync({
        NombreOriginal: "cv-juan-perez.pdf",
        Contexto: "rrhh/candidatos/cv",
        ContentType: "application/pdf",
        Contenido: stream
      })

IStorageService (S3StorageService)
    → Validar: PDF permitido ✅
    → Validar: 245KB < 20MB ✅
    → Generar nombre: "a1b2c3d4-e5f6-....pdf"
    → Subir a S3: "rrhh/candidatos/cv/a1b2c3d4-....pdf"
    → Retornar StorageResult {
        Exitoso: true,
        Url: "https://s3.../rrhh/candidatos/cv/a1b2c3d4-....pdf?X-Amz-Signature=...",
        RutaAlmacenada: "rrhh/candidatos/cv/a1b2c3d4-....pdf",
        Proveedor: "S3",
        TamanioBytes: 250880
      }

SubirCvCandidatoHandler
    → candidato.AsignarCv(rutaAlmacenada: "rrhh/candidatos/cv/a1b2c3d4-....pdf")
    → repo.GuardarAsync(candidato)
    → Retornar 200 { url: "https://s3.../..." }
```

---

**Fecha:** 2026-04-22
