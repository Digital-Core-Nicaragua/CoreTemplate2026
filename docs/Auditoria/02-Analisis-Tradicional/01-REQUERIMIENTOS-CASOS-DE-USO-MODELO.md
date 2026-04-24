# Auditoría Visible — Requerimientos Funcionales, Casos de Uso y Modelo

> **Fecha:** 2026-04-22
> **Prerequisito:** `CoreTemplate.Auditing` ya implementado ✅
> **Nota:** No requiere cambios en el building block — solo agregar capa de consulta.

---

## Requerimientos Funcionales

### RF-AUD-001: Listar logs de auditoría con filtros
**Prioridad:** Crítica

### Criterios de Aceptación
- `GET /api/auditoria` con filtros opcionales:
  - `entidad` — nombre de la entidad: "Usuario", "Factura"
  - `entidadId` — ID del registro específico
  - `usuarioId` — ID del usuario que realizó la acción
  - `accion` — Created, Updated, Deleted, Login, Logout
  - `fechaDesde` / `fechaHasta` — rango de fechas
- Paginado (default: 20, max: 100)
- Ordenado por `OcurridoEn` descendente
- Permiso requerido: `Auditoria.Ver`

### RF-AUD-002: Ver detalle de un log
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/auditoria/{id}`
- Retorna el registro completo incluyendo `ValoresAnteriores` y `ValoresNuevos` (JSON)
- Útil para ver exactamente qué campos cambiaron

### RF-AUD-003: Historial de una entidad específica
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/auditoria/entidad/{entidadId}`
- Lista todos los cambios de ese registro ordenados por fecha
- Permite ver la "historia de vida" de un registro

### RF-AUD-004: Actividad de un usuario
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/auditoria/usuario/{usuarioId}`
- Lista todas las acciones del usuario con filtro de fechas
- Útil para investigar actividad sospechosa

### RF-AUD-005: Multi-tenant
**Prioridad:** Crítica

### Criterios de Aceptación
- Los logs de Tenant A no son visibles para Tenant B
- `AuditLog` ya tiene `TenantId` — el QueryFilter aplica automáticamente

---

## Casos de Uso

### CU-AUD-001: Admin investiga quién eliminó un registro

```
GET /api/auditoria?entidadId={guid}&accion=Deleted
→ Retorna: quién lo eliminó, cuándo, desde qué IP
```

### CU-AUD-002: Admin revisa actividad de un usuario

```
GET /api/auditoria/usuario/{usuarioId}?fechaDesde=2025-01-01&fechaHasta=2025-01-31
→ Lista todas las acciones del usuario en enero 2025
```

### CU-AUD-003: Admin ve el historial completo de una factura

```
GET /api/auditoria/entidad/{facturaId}
→ Retorna: creación, modificaciones, quién las hizo y qué cambió exactamente
```

---

## Modelo de datos (ya existe en BD)

### Tabla: Shared.AuditLogs (ya creada por CoreTemplate.Auditing)

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | Multi-tenant |
| UsuarioId | uniqueidentifier? | Quién realizó la acción |
| NombreEntidad | nvarchar(100) | "Usuario", "Factura" |
| EntidadId | nvarchar(100) | ID del registro |
| Accion | nvarchar(20) | Created, Updated, Deleted, Login... |
| ValoresAnteriores | nvarchar(MAX)? | JSON con valores antes del cambio |
| ValoresNuevos | nvarchar(MAX)? | JSON con valores después del cambio |
| OcurridoEn | datetime2 | Fecha y hora UTC |
| DireccionIp | nvarchar(50)? | IP del request |
| UserAgent | nvarchar(500)? | Navegador/cliente |
| CorrelationId | nvarchar(100)? | X-Correlation-Id del request |

---

## DTO de respuesta

```csharp
public record AuditLogDto(
    Guid Id,
    Guid? TenantId,
    Guid? UsuarioId,
    string? NombreUsuario,       // join lógico con Auth.Usuarios
    string NombreEntidad,
    string EntidadId,
    string Accion,
    string? ValoresAnteriores,   // JSON
    string? ValoresNuevos,       // JSON
    DateTime OcurridoEn,
    string? DireccionIp,
    string? CorrelationId);
```

---

## Estructura de proyectos

```
src/Modules/Auditoria/
  CoreTemplate.Modules.Auditoria.Application/
    Queries/
      GetAuditLogsQuery.cs          con filtros y paginación
      GetAuditLogByIdQuery.cs
      GetAuditLogsPorEntidadQuery.cs
      GetAuditLogsPorUsuarioQuery.cs
    DTOs/
      AuditLogDto.cs
      AuditLogFiltrosQuery.cs

  CoreTemplate.Modules.Auditoria.Api/
    Controllers/
      AuditoriaController.cs
```

**Nota:** No necesita Domain ni Infrastructure propios.
Reutiliza `AuditDbContext` del building block `CoreTemplate.Auditing`.

---

## Endpoints

| Método | Ruta | Descripción | Permiso |
|---|---|---|---|
| GET | `/api/auditoria` | Listar con filtros (paginado) | `Auditoria.Ver` |
| GET | `/api/auditoria/{id}` | Ver detalle | `Auditoria.Ver` |
| GET | `/api/auditoria/entidad/{entidadId}` | Historial de entidad | `Auditoria.Ver` |
| GET | `/api/auditoria/usuario/{usuarioId}` | Actividad de usuario | `Auditoria.Ver` |

---

## Plan de implementación

```
Día 1:
  □ Queries: GetAuditLogs, GetAuditLogById, GetAuditLogsPorEntidad, GetAuditLogsPorUsuario
  □ DTOs: AuditLogDto, AuditLogFiltrosQuery
  □ AuditoriaController
  □ Agregar permiso Auditoria.Ver al seeder de Auth
  □ Registrar en Program.cs

Día 2 (opcional):
  □ Join lógico con Auth.Usuarios para mostrar nombre del usuario
  □ Exportar a PDF usando IModuloPdfGenerator
```

---

**Fecha:** 2026-04-22
