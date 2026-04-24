# Configuración del Sistema — Requerimientos Funcionales

> **Fecha:** 2026-04-22
> **Total:** 9 RF

---

## Contexto

Este módulo permite gestionar parámetros de negocio del sistema desde la UI,
sin necesidad de modificar `appsettings.json` ni redeployar.

**Separación clara de responsabilidades:**

| Tipo de configuración | Dónde vive | Quién lo cambia |
|---|---|---|
| Infraestructura (BD, Redis, JWT, puertos) | appsettings.json | Desarrollador / DevOps |
| Parámetros de negocio (nombre empresa, moneda, series) | BD — este módulo | Administrador del sistema |
| Credenciales sensibles | Variables de entorno / Secrets | DevOps |

---

## RF-CFG-001: Leer parámetro de configuración
**Prioridad:** Crítica

### Descripción
Cualquier módulo puede leer un parámetro por su clave.

### Criterios de Aceptación
- Contrato: `IConfiguracionService.ObtenerStringAsync(clave, valorPorDefecto)`
- Jerarquía: valor del tenant → valor global → valorPorDefecto del código
- Resultado cacheado en `IMemoryCache` con TTL de 10 minutos
- Si la clave no existe en BD → retorna `valorPorDefecto` (nunca lanza excepción)
- Métodos tipados: `ObtenerStringAsync`, `ObtenerIntAsync`, `ObtenerBoolAsync`, `ObtenerJsonAsync<T>`

---

## RF-CFG-002: Actualizar parámetro
**Prioridad:** Crítica

### Criterios de Aceptación
- `PUT /api/configuracion/{clave}`
- Solo parámetros con `EsEditable = true` pueden modificarse
- Valida que el valor sea compatible con el `Tipo` del parámetro
- Al actualizar → invalida el cache de esa clave
- Registra `ModificadoPor` y `ModificadoEn`
- Permiso requerido: `Configuracion.Editar`

---

## RF-CFG-003: Listar parámetros
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/configuracion` → lista todos los parámetros agrupados por `Grupo`
- `GET /api/configuracion/grupo/{grupo}` → lista parámetros de un grupo específico
- Retorna: clave, valor actual, tipo, descripción, grupo, esEditable
- Permiso requerido: `Configuracion.Ver`

---

## RF-CFG-004: Obtener parámetro por clave
**Prioridad:** Alta

### Criterios de Aceptación
- `GET /api/configuracion/{clave}`
- Si no existe → 404
- Permiso requerido: `Configuracion.Ver`

---

## RF-CFG-005: Seed inicial de parámetros
**Prioridad:** Crítica

### Descripción
Al arrancar, el seeder crea los parámetros del sistema si no existen.

### Parámetros incluidos:

**Grupo: Sistema**
| Clave | Valor por defecto | Tipo |
|---|---|---|
| `sistema.nombre` | "Mi Sistema" | String |
| `sistema.moneda` | "USD" | String |
| `sistema.zona-horaria` | "America/Managua" | String |
| `sistema.fecha-formato` | "dd/MM/yyyy" | String |
| `sistema.logo-url` | "" | String |
| `sistema.direccion` | "" | String |
| `sistema.telefono` | "" | String |
| `sistema.email-contacto` | "" | String |
| `sistema.sitio-web` | "" | String |

**Grupo: Facturación**
| Clave | Valor por defecto | Tipo |
|---|---|---|
| `facturacion.serie` | "001" | String |
| `facturacion.numero-actual` | "0" | Number |
| `facturacion.prefijo` | "FAC-" | String |
| `facturacion.dias-vencimiento` | "30" | Number |
| `facturacion.impuesto-porcentaje` | "15" | Number |

**Grupo: Nómina**
| Clave | Valor por defecto | Tipo |
|---|---|---|
| `nomina.dia-pago-quincenal` | "15" | Number |
| `nomina.dia-pago-mensual` | "30" | Number |
| `nomina.horas-jornada` | "8" | Number |

**Grupo: RRHH**
| Clave | Valor por defecto | Tipo |
|---|---|---|
| `rrhh.dias-vacaciones-anuales` | "15" | Number |
| `rrhh.meses-periodo-prueba` | "3" | Number |

### Criterios de Aceptación
- Si el parámetro ya existe → no sobreescribir
- Parámetros del sistema tienen `EsEditable = true` (el admin puede cambiarlos)
- El seeder usa `IgnoreQueryFilters()` para no ser bloqueado por el QueryFilter de tenant

---

## RF-CFG-006: Multi-tenant
**Prioridad:** Alta

### Criterios de Aceptación
- `ConfiguracionItem` implementa `IHasTenant`
- Jerarquía de resolución: tenant actual → global (TenantId = null) → valorPorDefecto
- En single-tenant: todos los parámetros tienen `TenantId = null`
- En multi-tenant: cada empresa puede tener sus propios valores

---

## RF-CFG-007: Tipos de valor
**Prioridad:** Media

| Tipo | Descripción | Validación |
|---|---|---|
| `String` | Texto libre | Longitud máxima 2000 |
| `Number` | Número entero o decimal | Parseable como decimal |
| `Boolean` | true / false | "true" o "false" |
| `Json` | Objeto JSON | JSON válido |

---

## RF-CFG-008: Permisos del módulo
**Prioridad:** Alta

| Código | Descripción |
|---|---|
| `Configuracion.Ver` | Ver y listar parámetros |
| `Configuracion.Editar` | Actualizar valores de parámetros |

---

## RF-CFG-009: Integración con otros módulos
**Prioridad:** Alta

### Descripción
Los módulos que hoy leen de `AppSettings` deben migrar a `IConfiguracionService`.

### Módulos a actualizar:
- `TemplateRenderer` (EmailTemplates) → leer `sistema.nombre` de `IConfiguracionService`
- `ModuloPdfGenerator` (PdfTemplates) → leer `sistema.nombre` de `IConfiguracionService`
- Módulos de Nómina → leer `nomina.dia-pago` de `IConfiguracionService`
- Módulos de Facturación → leer `facturacion.serie` y `facturacion.prefijo`

---

## Resumen

| Prioridad | Cantidad |
|---|---|
| Crítica | 3 |
| Alta | 5 |
| Media | 1 |
| **Total** | **9** |

---

**Fecha:** 2026-04-22
