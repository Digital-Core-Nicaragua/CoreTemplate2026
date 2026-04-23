# Storage — Requerimientos Funcionales

> **Building Block:** CoreTemplate.Storage
> **Fecha:** 2026-04-22
> **Total:** 10 RF

---

## Contexto

`CoreTemplate.Storage` es un building block transversal de infraestructura.
Permite a cualquier módulo subir, descargar y eliminar archivos sin conocer
el proveedor de almacenamiento subyacente.

El building block gestiona el **almacenamiento físico** del archivo.
Los **metadatos** (nombre, tipo, módulo origen, entidad relacionada) los gestiona
el módulo `Archivos` que consume este building block.

**Módulos consumidores previstos:**
- RRHH → CVs, cédulas, contratos, fotos de empleados, records
- Contabilidad → facturas escaneadas, soportes en PDF, comprobantes
- Nómina → comprobantes de pago generados
- Cualquier módulo futuro que requiera almacenar archivos

---

## RF-STORAGE-001: Subir archivo
**Prioridad:** Crítica

### Descripción
El sistema permite subir un archivo y obtener una URL para accederlo posteriormente.

### Criterios de Aceptación
- Recibe: stream del archivo, nombre original, contexto lógico, tipo MIME
- El `contexto` define la carpeta lógica: `"rrhh/candidatos/cv"`, `"contabilidad/facturas/2025"`
- Genera un nombre único de almacenamiento (GUID + extensión original) para evitar colisiones
- Retorna `StorageResult` con: URL de acceso, ruta interna, proveedor, tamaño en bytes
- Si falla → retorna `StorageResult { Exitoso = false, Error = "..." }`

---

## RF-STORAGE-002: Obtener URL de acceso
**Prioridad:** Crítica

### Descripción
Dado la ruta interna de un archivo almacenado, retorna la URL para visualizarlo o descargarlo.

### Criterios de Aceptación
- Para proveedor Local: retorna URL relativa al servidor (`/archivos/rrhh/cv/archivo.pdf`)
- Para proveedor S3: retorna URL firmada con expiración configurable (default: 1 hora)
- Para proveedor Firebase: retorna URL de descarga con token
- Si el archivo no existe → retorna null o lanza `ArchivoNoEncontradoException`

---

## RF-STORAGE-003: Eliminar archivo
**Prioridad:** Alta

### Descripción
El sistema permite eliminar un archivo del almacenamiento dado su ruta interna.

### Criterios de Aceptación
- Recibe la ruta interna retornada al subir el archivo
- Si el archivo no existe → no lanza error (idempotente)
- Registra la eliminación en log

---

## RF-STORAGE-004: Proveedor configurable sin cambio de código
**Prioridad:** Crítica

### Descripción
El proveedor de almacenamiento se configura en `appsettings.json`.
Cambiar de proveedor no requiere modificar ningún módulo consumidor.

### Criterios de Aceptación
- Proveedores soportados: `Local`, `S3`, `Firebase`
- Configuración: `"StorageSettings": { "Provider": "Local" }`
- Si el proveedor configurado no existe → error al iniciar la aplicación (fail-fast)
- Cada proveedor tiene su propia sección de configuración

---

## RF-STORAGE-005: Proveedor Local (disco del servidor)
**Prioridad:** Crítica

### Descripción
Almacena archivos en el sistema de archivos del servidor.

### Criterios de Aceptación
- Configuración: `BasePath` (ruta base en disco), `BaseUrl` (URL base para acceso HTTP)
- Crea subdirectorios automáticamente según el `contexto`
- Sirve archivos via endpoint HTTP estático o controlador dedicado
- Útil para desarrollo y despliegues en servidor único

---

## RF-STORAGE-006: Proveedor AWS S3
**Prioridad:** Alta

### Descripción
Almacena archivos en un bucket de Amazon S3.

### Criterios de Aceptación
- Configuración: `BucketName`, `Region`, `AccessKey`, `SecretKey` (o IAM Role)
- La clave del objeto en S3 incluye el contexto: `rrhh/candidatos/cv/{guid}.pdf`
- URLs firmadas con expiración configurable (default: 3600 segundos)
- Soporta buckets privados (acceso solo via URL firmada)
- Soporta autenticación via IAM Role (sin credenciales en config, recomendado en producción)

---

## RF-STORAGE-007: Proveedor Firebase Storage
**Prioridad:** Media

### Descripción
Almacena archivos en Firebase Storage (Google Cloud Storage).

### Criterios de Aceptación
- Configuración: `ProjectId`, `Bucket`, `ServiceAccountKeyPath` o `ServiceAccountKeyJson`
- Retorna URL de descarga con token de Firebase
- Estructura de carpetas igual al contexto recibido

---

## RF-STORAGE-008: Validación de tipo de archivo
**Prioridad:** Alta

### Descripción
El sistema valida que el tipo de archivo sea permitido antes de almacenarlo.

### Criterios de Aceptación
- Lista de tipos MIME permitidos configurable en appsettings
- Default permitidos: `application/pdf`, `image/jpeg`, `image/png`, `image/webp`,
  `application/msword`, `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- Si el tipo no está permitido → retorna `StorageResult { Exitoso = false, Error = "TipoNoPermitido" }`
- Validación por Content-Type Y por extensión del archivo (doble validación)

---

## RF-STORAGE-009: Validación de tamaño
**Prioridad:** Alta

### Descripción
El sistema valida que el archivo no supere el tamaño máximo configurado.

### Criterios de Aceptación
- Tamaño máximo configurable en appsettings (default: 20 MB)
- Si supera el límite → retorna `StorageResult { Exitoso = false, Error = "TamanioExcedido" }`
- El tamaño máximo puede configurarse diferente por contexto (futuro)

---

## RF-STORAGE-010: Registro en log
**Prioridad:** Alta

### Descripción
Cada operación de almacenamiento queda registrada en el log estructurado.

### Criterios de Aceptación
- Log de subida: contexto, nombre original, nombre almacenado, tamaño, proveedor
- Log de eliminación: ruta eliminada, proveedor
- Log de fallo: operación, error, proveedor
- Usa `IAppLogger` de `CoreTemplate.Logging`
- Incluye `X-Correlation-Id` en cada entrada

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 4 |
| Alta | 5 |
| Media | 1 |
| **Total** | **10** |

---

**Fecha:** 2026-04-22

---

## Addendum — Multi-tenant

> **Fecha:** 2026-04-22

### RF-STORAGE-011: Aislamiento de archivos por tenant
**Prioridad:** Crítica

### Descripción
En modo multi-tenant cada empresa solo puede ver y acceder a sus propios archivos.

### Criterios de Aceptación
- `ArchivoAdjunto` implementa `IHasTenant` — el `BaseDbContext` aplica el QueryFilter
  automático que filtra por `TenantId` del request en todas las consultas
- Al subir un archivo, el `TenantId` del usuario autenticado se asigna automáticamente
  via `ICurrentTenant` en el handler `SubirArchivoHandler`
- Un tenant no puede obtener la URL ni los metadatos de archivos de otro tenant
- En modo single-tenant (`IsMultiTenant = false`) el `TenantId` es null y el filtro no aplica

### Aislamiento en el proveedor de almacenamiento

El `contexto` incluye implícitamente el aislamiento por la estructura de carpetas,
pero el aislamiento real lo garantiza el QueryFilter en BD — no el path del archivo.

```
Tenant A sube: rrhh/candidatos/cv/{guid-A}.pdf
Tenant B sube: rrhh/candidatos/cv/{guid-B}.pdf

→ Mismo path lógico, diferente TenantId en BD
→ Tenant A nunca verá el archivo de Tenant B porque el QueryFilter lo filtra
```

---

**Fecha addendum:** 2026-04-22
