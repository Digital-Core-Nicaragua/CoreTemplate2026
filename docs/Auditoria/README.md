# Auditoría Visible — Documentación

> **Módulo:** Extensión de `CoreTemplate.Auditing`
> **Fecha:** 2026-04-22
> **Estado:** Pendiente de implementación
> **Prerequisito:** `CoreTemplate.Auditing` ya implementado ✅

---

## Contexto

El building block `CoreTemplate.Auditing` ya guarda automáticamente todos los cambios
en la tabla `Shared.AuditLogs`. El problema es que no hay ningún endpoint para consultarlos.

El administrador no puede responder preguntas como:
- ¿Quién eliminó este registro?
- ¿Qué cambios hizo el usuario X ayer?
- ¿Cuándo se modificó esta factura y qué cambió?

**Este módulo agrega los endpoints de consulta sobre los logs ya existentes.**
No requiere cambios en el building block — solo agregar la capa de consulta.

---

## Requerimientos Funcionales

### RF-AUD-001: Consultar historial de una entidad
**Prioridad:** Crítica

```
GET /api/auditoria?entidadId={guid}&entidad=Factura
→ Lista todos los cambios de esa entidad ordenados por fecha desc
```

### RF-AUD-002: Consultar actividad de un usuario
**Prioridad:** Alta

```
GET /api/auditoria?usuarioId={guid}&fechaDesde=2025-01-01&fechaHasta=2025-01-31
→ Lista todas las acciones del usuario en el período
```

### RF-AUD-003: Consultar por tipo de acción
**Prioridad:** Media

```
GET /api/auditoria?accion=Deleted&entidad=Usuario
→ Lista todos los registros eliminados de la entidad Usuario
```

### RF-AUD-004: Ver detalle de un cambio
**Prioridad:** Alta

```
GET /api/auditoria/{id}
→ Retorna el registro completo con ValoresAnteriores y ValoresNuevos (JSON diff)
```

### RF-AUD-005: Exportar a PDF/Excel
**Prioridad:** Baja

```
GET /api/auditoria/exportar?formato=pdf&...filtros...
→ Genera reporte de auditoría usando PdfTemplates
```

---

## Endpoints

| Método | Ruta | Descripción | Permiso |
|---|---|---|---|
| GET | `/api/auditoria` | Listar logs con filtros (paginado) | `Auditoria.Ver` |
| GET | `/api/auditoria/{id}` | Ver detalle de un log | `Auditoria.Ver` |
| GET | `/api/auditoria/entidad/{entidadId}` | Historial de una entidad | `Auditoria.Ver` |
| GET | `/api/auditoria/usuario/{usuarioId}` | Actividad de un usuario | `Auditoria.Ver` |

### Filtros disponibles en GET /api/auditoria

| Parámetro | Tipo | Descripción |
|---|---|---|
| `entidad` | string | Nombre de la entidad: "Usuario", "Factura" |
| `entidadId` | Guid | ID del registro específico |
| `usuarioId` | Guid | ID del usuario que realizó la acción |
| `accion` | string | Created, Updated, Deleted, Login, Logout |
| `fechaDesde` | DateTime | Fecha de inicio del período |
| `fechaHasta` | DateTime | Fecha de fin del período |
| `pagina` | int | Número de página (default: 1) |
| `tamano` | int | Tamaño de página (default: 20, max: 100) |

---

## DTO de respuesta

```csharp
public record AuditLogDto(
    Guid Id,
    Guid? TenantId,
    Guid? UsuarioId,
    string? NombreUsuario,      // join lógico con tabla Usuarios
    string NombreEntidad,
    string EntidadId,
    string Accion,
    string? ValoresAnteriores,  // JSON
    string? ValoresNuevos,      // JSON
    DateTime OcurridoEn,
    string? DireccionIp,
    string? CorrelationId);
```

---

## Plan de implementación

### Fase 1 — Application (Día 1)
```
□ Crear Queries: GetAuditLogs, GetAuditLogById, GetAuditLogsPorEntidad
□ Crear DTOs: AuditLogDto, AuditLogFiltrosQuery
□ Reutilizar AuditDbContext existente (solo lectura)
□ DependencyInjection.cs
```

### Fase 2 — API (Día 1)
```
□ AuditoriaController con todos los endpoints
□ Agregar permiso Auditoria.Ver al seeder de Auth
□ Registrar en Program.cs
```

### Fase 3 — Mejoras opcionales (Día 2)
```
□ Join lógico con tabla Usuarios para mostrar nombre del usuario
□ Exportar a PDF usando IModuloPdfGenerator
□ Dashboard de actividad reciente
```

---

**Estado:** Documentado — pendiente de implementación
**Fecha:** 2026-04-22
