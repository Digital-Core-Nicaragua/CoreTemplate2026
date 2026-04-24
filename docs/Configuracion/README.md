# Configuración del Sistema — Documentación

> **Módulo:** CoreTemplate.Modules.Configuracion
> **Fecha:** 2026-04-22
> **Estado:** Pendiente de implementación

---

## ¿Por qué este módulo?

Hoy los parámetros del sistema viven en `appsettings.json`. Cambiar cualquier valor
requiere modificar el archivo y redeployar. En un ERP esto es inaceptable para:

- El nombre y datos de la empresa
- La moneda y zona horaria
- Numeración de facturas, recibos, contratos
- Días de pago de nómina
- Límites y reglas de negocio configurables

**Este módulo permite cambiar cualquier parámetro desde la UI sin redeployar.**

---

## ¿Es necesario en single-tenant?

**Sí, igual de necesario.** La diferencia es solo técnica:

| Modo | TenantId en la tabla | Comportamiento |
|---|---|---|
| Single-tenant | `null` | Un solo conjunto de parámetros para todo el sistema |
| Multi-tenant | `tenant-A`, `tenant-B`... | Cada empresa tiene sus propios parámetros |

La tabla es la misma. El código es el mismo. Solo cambia si `TenantId` tiene valor o no.

---

## Modelo de datos

### Aggregate: ConfiguracionItem

```
ConfiguracionItem (AggregateRoot, IHasTenant)
  + Id          : Guid
  + TenantId    : Guid?       null = parámetro global del sistema
  + Clave       : string      "sistema.nombre" (inmutable)
  + Valor       : string      "Mi ERP S.A."
  + Tipo        : TipoValor   String | Number | Boolean | Json
  + Descripcion : string      "Nombre de la empresa que aparece en documentos"
  + Grupo       : string      "Sistema" | "Facturacion" | "Nomina" | "RRHH"
  + EsEditable  : bool        false = solo lectura (parámetros del sistema)
  + CreadoEn    : DateTime
  + ModificadoEn: DateTime?
  + ModificadoPor: Guid?
```

### Tabla: Configuracion.Items

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | null = global |
| Clave | nvarchar(100) | Única por tenant. Ej: "sistema.nombre" |
| Valor | nvarchar(2000) | Valor como string (se convierte según Tipo) |
| Tipo | nvarchar(20) | String, Number, Boolean, Json |
| Descripcion | nvarchar(500) | Descripción para mostrar en la UI |
| Grupo | nvarchar(50) | Agrupación visual en la UI |
| EsEditable | bit | Si el admin puede modificarlo |
| CreadoEn | datetime2 | |
| ModificadoEn | datetime2? | |
| ModificadoPor | uniqueidentifier? | |

**Índice único:** `(Clave, TenantId)`

---

## Parámetros del sistema incluidos en el seed

### Grupo: Sistema

| Clave | Valor por defecto | Tipo | Descripción |
|---|---|---|---|
| `sistema.nombre` | "Mi Sistema" | String | Nombre de la empresa/sistema |
| `sistema.moneda` | "USD" | String | Moneda principal (ISO 4217) |
| `sistema.zona-horaria` | "America/Managua" | String | Zona horaria (IANA) |
| `sistema.fecha-formato` | "dd/MM/yyyy" | String | Formato de fechas en documentos |
| `sistema.logo-url` | "" | String | URL del logo (puede venir de Storage) |
| `sistema.direccion` | "" | String | Dirección de la empresa |
| `sistema.telefono` | "" | String | Teléfono de contacto |
| `sistema.email-contacto` | "" | String | Email de contacto |
| `sistema.sitio-web` | "" | String | Sitio web |

### Grupo: Facturación

| Clave | Valor por defecto | Tipo | Descripción |
|---|---|---|---|
| `facturacion.serie` | "001" | String | Serie de facturas |
| `facturacion.numero-actual` | "0" | Number | Último número usado (autoincrementa) |
| `facturacion.prefijo` | "FAC-" | String | Prefijo del número de factura |
| `facturacion.dias-vencimiento` | "30" | Number | Días de vencimiento por defecto |
| `facturacion.moneda` | "USD" | String | Moneda de facturación |
| `facturacion.impuesto-porcentaje` | "15" | Number | % de impuesto por defecto |

### Grupo: Nómina

| Clave | Valor por defecto | Tipo | Descripción |
|---|---|---|---|
| `nomina.dia-pago-quincenal` | "15" | Number | Día de pago quincenal |
| `nomina.dia-pago-mensual` | "30" | Number | Día de pago mensual |
| `nomina.moneda` | "USD" | String | Moneda de pago |
| `nomina.horas-jornada` | "8" | Number | Horas de jornada laboral |

### Grupo: RRHH

| Clave | Valor por defecto | Tipo | Descripción |
|---|---|---|---|
| `rrhh.dias-vacaciones-anuales` | "15" | Number | Días de vacaciones por año |
| `rrhh.meses-periodo-prueba` | "3" | Number | Meses de período de prueba |

---

## Contratos

```csharp
public interface IConfiguracionService
{
    // Obtener valor tipado
    Task<string> ObtenerStringAsync(string clave, string valorPorDefecto = "");
    Task<int> ObtenerIntAsync(string clave, int valorPorDefecto = 0);
    Task<bool> ObtenerBoolAsync(string clave, bool valorPorDefecto = false);
    Task<T?> ObtenerJsonAsync<T>(string clave) where T : class;

    // Actualizar
    Task ActualizarAsync(string clave, string valor, Guid modificadoPor);

    // Invalidar cache
    void InvalidarCache(string clave);
}
```

---

## Cache — importante para rendimiento

Los parámetros se leen en cada request (ej: nombre de empresa en cada PDF).
Sin cache esto genera una consulta a BD por cada lectura.

**Solución:** `IMemoryCache` con TTL de 10 minutos.

```csharp
// En ConfiguracionService
var cacheKey = $"config-{tenantId}-{clave}";
if (!_cache.TryGetValue(cacheKey, out string? valor))
{
    valor = await _repo.ObtenerAsync(clave, tenantId);
    _cache.Set(cacheKey, valor, TimeSpan.FromMinutes(10));
}
```

Al actualizar un parámetro → invalidar el cache de esa clave.

---

## Endpoints

| Método | Ruta | Descripción | Permiso |
|---|---|---|---|
| GET | `/api/configuracion` | Listar todos los parámetros (agrupados) | `Configuracion.Ver` |
| GET | `/api/configuracion/{clave}` | Obtener un parámetro por clave | `Configuracion.Ver` |
| PUT | `/api/configuracion/{clave}` | Actualizar valor de un parámetro | `Configuracion.Editar` |
| GET | `/api/configuracion/grupo/{grupo}` | Listar parámetros de un grupo | `Configuracion.Ver` |

---

## Cómo lo usa un módulo

```csharp
// En cualquier handler o servicio
public class GenerarFacturaHandler(IConfiguracionService config)
{
    public async Task Handle(GenerarFacturaCommand cmd, CancellationToken ct)
    {
        var serie = await config.ObtenerStringAsync("facturacion.serie");
        var prefijo = await config.ObtenerStringAsync("facturacion.prefijo");
        var impuesto = await config.ObtenerIntAsync("facturacion.impuesto-porcentaje");
        var nombreEmpresa = await config.ObtenerStringAsync("sistema.nombre");
        // ...
    }
}
```

---

## Relación con appsettings.json

Los parámetros de BD **complementan** a appsettings, no lo reemplazan:

| Tipo de configuración | Dónde vive | Quién lo cambia |
|---|---|---|
| Infraestructura (BD, Redis, JWT) | appsettings.json | Desarrollador / DevOps |
| Parámetros de negocio | BD (este módulo) | Administrador del sistema |
| Credenciales sensibles | Variables de entorno / Secrets | DevOps |

---

## Plan de implementación

### Fase 1 — Domain y Application (Día 1)
```
□ Aggregate ConfiguracionItem con IHasTenant
□ IConfiguracionService (contrato público)
□ IConfiguracionItemRepository
□ Commands: ActualizarConfiguracion
□ Queries: GetConfiguracion, GetConfiguracionPorGrupo, GetConfiguracionPorClave
□ DTOs: ConfiguracionItemDto, ConfiguracionGrupoDto
```

### Fase 2 — Infrastructure (Día 1-2)
```
□ ConfiguracionDbContext (schema: Configuracion)
□ ConfiguracionItemConfiguration (EF)
□ Migración: InitialCreate_Configuracion
□ ConfiguracionItemRepository con IgnoreQueryFilters para globales
□ ConfiguracionService con IMemoryCache (TTL 10 min)
□ ConfiguracionDataSeeder (todos los parámetros del seed)
□ DependencyInjection.cs
```

### Fase 3 — API (Día 2)
```
□ ConfiguracionController
□ Contratos de request/response
□ Agregar permisos al seeder de Auth: Configuracion.Ver, Configuracion.Editar
□ Registrar en Program.cs
```

### Fase 4 — Integración (Día 2-3)
```
□ Actualizar PdfTemplates para leer sistema.nombre de ConfiguracionService
□ Actualizar EmailTemplates para leer sistema.nombre de ConfiguracionService
□ Actualizar AppSettings en TemplateRenderer para usar ConfiguracionService
```

---

**Estado:** Documentado — pendiente de implementación
**Fecha:** 2026-04-22
