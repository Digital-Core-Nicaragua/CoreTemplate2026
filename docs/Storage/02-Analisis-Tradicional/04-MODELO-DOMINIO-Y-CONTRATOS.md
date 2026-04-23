# Storage — Modelo de Dominio y Contratos

> **Building Block:** CoreTemplate.Storage
> **Fecha:** 2026-04-22

---

## Nota sobre el modelo

`CoreTemplate.Storage` es infraestructura pura — no tiene aggregates ni base de datos.
Los **metadatos** de los archivos (nombre original, módulo origen, entidad relacionada)
los gestiona el módulo `Archivos` que consume este building block.

---

## Contratos principales

### IStorageService

```
IStorageService
  + SubirAsync(SubirArchivoRequest request)    : Task<StorageResult>
  + ObtenerUrlAsync(string rutaAlmacenada)     : Task<string?>
  + EliminarAsync(string rutaAlmacenada)       : Task
```

---

### SubirArchivoRequest

```
SubirArchivoRequest (record)
  + Contenido       : Stream   -- stream del archivo (no byte[], para soportar archivos grandes)
  + NombreOriginal  : string   -- "cv-juan-perez.pdf"
  + Contexto        : string   -- "rrhh/candidatos/cv" | "contabilidad/facturas/2025"
  + ContentType     : string   -- "application/pdf" | "image/jpeg"
```

---

### StorageResult

```
StorageResult (record)
  + Exitoso         : bool
  + Url             : string?  -- URL para visualizar/descargar (firmada si S3/Firebase)
  + RutaAlmacenada  : string?  -- "rrhh/candidatos/cv/{guid}.pdf" — guardar en BD del módulo
  + Proveedor       : string?  -- "Local" | "S3" | "Firebase"
  + TamanioBytes    : long
  + Error           : string?  -- descripción si Exitoso = false
```

---

## Módulo Archivos (metadatos en BD)

El módulo `Archivos` es un módulo de negocio separado que usa `IStorageService`
internamente y expone una API para gestionar los metadatos de los archivos.

### ArchivoAdjunto (Aggregate)

```
ArchivoAdjunto
  + Id                : Guid
  + TenantId          : Guid?
  + NombreOriginal    : string   -- "cv-juan-perez.pdf"
  + NombreAlmacenado  : string   -- "{guid}.pdf"
  + RutaAlmacenada    : string   -- "rrhh/candidatos/cv/{guid}.pdf"
  + Url               : string   -- URL actual (puede regenerarse si expira)
  + ContentType       : string   -- "application/pdf"
  + TamanioBytes      : long
  + Proveedor         : string   -- "Local" | "S3" | "Firebase"
  + Contexto          : string   -- "rrhh/candidatos/cv"
  + ModuloOrigen      : string   -- "RRHH" | "Contabilidad" | "Nomina"
  + EntidadId         : Guid?    -- ID del registro relacionado (candidato, factura, etc.)
  + SubidoPor         : Guid     -- UsuarioId
  + FechaSubida       : DateTime
  + EsActivo          : bool
```

### Cómo un módulo referencia sus archivos

```
-- En el módulo RRHH:
Candidato
  + Id              : Guid
  + Nombre          : string
  + CvArchivoId     : Guid?    -- FK lógica a ArchivoAdjunto.Id
  + FotoArchivoId   : Guid?

-- Para obtener la URL del CV:
GET /api/archivos/{cvArchivoId}/url
→ { url: "https://s3.../rrhh/candidatos/cv/{guid}.pdf?X-Amz-Signature=..." }
```

---

## Configuración en appsettings

```json
{
  "StorageSettings": {
    "Provider": "Local",
    "MaxTamanioMB": 20,
    "TiposPermitidos": [
      "application/pdf",
      "image/jpeg",
      "image/png",
      "image/webp",
      "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ]
  },
  "LocalStorageSettings": {
    "BasePath": "C:/archivos/misistema",
    "BaseUrl": "https://misistema.com/archivos"
  },
  "S3Settings": {
    "BucketName": "misistema-archivos",
    "Region": "us-east-1",
    "AccessKey": "<access-key>",
    "SecretKey": "<secret-key>",
    "UrlExpirationSeconds": 3600
  },
  "FirebaseSettings": {
    "ProjectId": "misistema",
    "Bucket": "misistema.appspot.com",
    "ServiceAccountKeyPath": "firebase-key.json"
  }
}
```

---

## Estructura de proyectos

```
src/BuildingBlocks/CoreTemplate.Storage/
  Abstractions/
    IStorageService.cs
    SubirArchivoRequest.cs
    StorageResult.cs
  Providers/
    Local/
      LocalStorageService.cs
      LocalStorageSettings.cs
    S3/
      S3StorageService.cs
      S3Settings.cs
    Firebase/
      FirebaseStorageService.cs      -- estructura base, implementación pendiente
      FirebaseSettings.cs
  Settings/
    StorageSettings.cs               -- Provider, MaxTamanioMB, TiposPermitidos
  Validation/
    ArchivoValidator.cs              -- valida tipo y tamaño antes de subir
  DependencyInjection.cs
  CoreTemplate.Storage.csproj

src/Modules/Archivos/
  CoreTemplate.Modules.Archivos.Domain/
    Aggregates/
      ArchivoAdjunto.cs
    Repositories/
      IArchivoAdjuntoRepository.cs
  CoreTemplate.Modules.Archivos.Application/
    Commands/
      SubirArchivoCommand.cs
      SubirArchivoHandler.cs
      EliminarArchivoCommand.cs
    Queries/
      ObtenerUrlArchivoQuery.cs
      ObtenerArchivoQuery.cs
    DTOs/
      ArchivoAdjuntoDto.cs
  CoreTemplate.Modules.Archivos.Infrastructure/
    Persistence/
      ArchivosDbContext.cs           -- schema: Archivos
      Configurations/
        ArchivoAdjuntoConfiguration.cs
    Repositories/
      ArchivoAdjuntoRepository.cs
  CoreTemplate.Modules.Archivos.Api/
    Controllers/
      ArchivosController.cs
    Contracts/
      SubirArchivoRequest.cs
```

---

## Diagrama de dependencias

```
Módulo RRHH
Módulo Contabilidad   ──► Módulo Archivos ──► IStorageService ──► LocalStorageService ──► Disco
Módulo Nómina                                                  ──► S3StorageService    ──► AWS S3
                                                               ──► FirebaseService     ──► Firebase
```

Los módulos de negocio dependen del módulo `Archivos`.
El módulo `Archivos` depende de `IStorageService`.
Los módulos de negocio NO dependen directamente de `IStorageService`.

---

## Endpoints del módulo Archivos

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/archivos` | Subir archivo (multipart/form-data) |
| GET | `/api/archivos/{id}` | Obtener metadatos del archivo |
| GET | `/api/archivos/{id}/url` | Obtener URL de acceso (firmada si S3) |
| DELETE | `/api/archivos/{id}` | Eliminar archivo |
| GET | `/api/archivos?moduloOrigen=RRHH&entidadId={id}` | Listar archivos de una entidad |

---

**Fecha:** 2026-04-22
