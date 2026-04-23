# Storage — Documentación

> **Tipo:** Building Block transversal + Módulo Archivos
> **Proyecto:** CoreTemplate

---

## Estructura de Documentos

| Documento | Descripción |
|---|---|
| **01-EventStorming/01-BIG-PICTURE.md** | Flujos de dominio del módulo Archivos, integraciones con módulos consumidores, hotspots |
| **02-Analisis-Tradicional/01-REQUERIMIENTOS-FUNCIONALES.md** | 10 RF con criterios de aceptación |
| **02-Analisis-Tradicional/02-REQUERIMIENTOS-NO-FUNCIONALES.md** | Intercambiabilidad, seguridad, rendimiento, extensibilidad |
| **02-Analisis-Tradicional/03-CASOS-DE-USO.md** | CU por módulo consumidor, flujo completo RRHH |
| **02-Analisis-Tradicional/04-MODELO-DOMINIO-Y-CONTRATOS.md** | Contratos, aggregate ArchivoAdjunto, configuración, estructura de proyectos |

---

## Resumen

Dos componentes que trabajan juntos:

### CoreTemplate.Storage (Building Block)
Infraestructura pura — sube, descarga y elimina archivos.
Sin base de datos. Sin aggregates.

**Proveedores soportados:**
- `Local` — disco del servidor (desarrollo / producción simple)
- `S3` — AWS S3 (producción escalable)
- `Firebase` — Firebase Storage (preparado para implementar)

**Cambiar proveedor:** solo modificar `appsettings.json`, sin tocar código.

### Módulo Archivos
Gestiona los metadatos de los archivos en base de datos.
Expone API REST para subir, consultar y eliminar archivos.
Internamente usa `IStorageService`.

**Soporte multi-tenant:**
- `ArchivoAdjunto` implementa `IHasTenant` — el `BaseDbContext` aplica el QueryFilter automáticamente
- Cada empresa solo ve sus propios archivos
- El `TenantId` se asigna automáticamente desde `ICurrentTenant` al subir

---

## Módulos consumidores

| Módulo | Contextos de uso | Tipos de archivo |
|---|---|---|
| RRHH | CVs, fotos, cédulas, contratos, records | PDF, JPEG, PNG, DOC |
| Contabilidad | Facturas, soportes, comprobantes | PDF, JPEG, PNG |
| Nómina | Comprobantes de pago | PDF |
| Cualquier módulo futuro | Cualquier documento | Configurable |

---

## Relación con Email

Storage y Email se complementan:
- Nómina genera un PDF → lo sube via Storage → obtiene URL → la envía por Email
- Contabilidad sube una factura → obtiene URL → la envía por Email al cliente

---

**Estado:** Documentado — pendiente implementación
**Fecha:** 2026-04-22
