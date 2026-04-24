# Configuración del Sistema — Casos de Uso y Modelo de Dominio

> **Fecha:** 2026-04-22

---

## Casos de Uso

### CU-CFG-001: Admin configura el nombre de la empresa

**Actor:** Administrador
```
GET /api/configuracion/grupo/Sistema
→ Ve todos los parámetros del grupo Sistema

PUT /api/configuracion/sistema.nombre
{ "valor": "Empresa ABC S.A." }
→ Actualiza el nombre
→ Cache invalidado
→ Próximo PDF generado usa "Empresa ABC S.A."
```

### CU-CFG-002: Admin configura la serie de facturación

```
PUT /api/configuracion/facturacion.serie
{ "valor": "A001" }

PUT /api/configuracion/facturacion.prefijo
{ "valor": "FACT-" }
→ Próxima factura generada: "FACT-A001-0001"
```

### CU-CFG-003: Módulo Nómina lee el día de pago

```csharp
// En GenerarNominaHandler
var diaPago = await config.ObtenerIntAsync("nomina.dia-pago-mensual", 30);
// → 30 (valor por defecto si no está configurado)
// → 15 (si el admin lo cambió)
```

### CU-CFG-004: Tenant A tiene su propia configuración

```
Tenant A: sistema.nombre = "Empresa ABC"
Tenant B: no tiene configuración propia

Tenant A genera PDF → "Empresa ABC"
Tenant B genera PDF → "Mi Sistema" (valor global del seed)
```

---

## Aggregate: ConfiguracionItem

```
ConfiguracionItem (AggregateRoot, IHasTenant)
  + Id            : Guid
  + TenantId      : Guid?       null = parámetro global
  + Clave         : string      "sistema.nombre" (inmutable)
  + Valor         : string      "Mi Sistema"
  + Tipo          : TipoValor   String | Number | Boolean | Json
  + Descripcion   : string      "Nombre de la empresa en documentos"
  + Grupo         : string      "Sistema" | "Facturacion" | "Nomina" | "RRHH"
  + EsEditable    : bool        true = admin puede modificarlo
  + CreadoEn      : DateTime
  + ModificadoEn  : DateTime?
  + ModificadoPor : Guid?

Métodos:
  + Crear(...)           : Result<ConfiguracionItem>
  + Actualizar(valor, modificadoPor) : Result
```

---

## Contrato público

```csharp
public interface IConfiguracionService
{
    Task<string>  ObtenerStringAsync(string clave, string valorPorDefecto = "");
    Task<int>     ObtenerIntAsync(string clave, int valorPorDefecto = 0);
    Task<bool>    ObtenerBoolAsync(string clave, bool valorPorDefecto = false);
    Task<T?>      ObtenerJsonAsync<T>(string clave) where T : class;
    Task          ActualizarAsync(string clave, string valor, Guid modificadoPor);
    void          InvalidarCache(string clave);
}
```

---

## Modelo de datos

### Tabla: Configuracion.Items

| Campo | Tipo | Descripción |
|---|---|---|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier? | null = global |
| Clave | nvarchar(100) | Única por tenant |
| Valor | nvarchar(2000) | Valor como string |
| Tipo | nvarchar(20) | String, Number, Boolean, Json |
| Descripcion | nvarchar(500) | Para mostrar en la UI |
| Grupo | nvarchar(50) | Agrupación visual |
| EsEditable | bit | Si el admin puede modificarlo |
| CreadoEn | datetime2 | |
| ModificadoEn | datetime2? | |
| ModificadoPor | uniqueidentifier? | |

**Índice único:** `(Clave, TenantId)`

---

## Estructura de proyectos

```
src/Modules/Configuracion/
  CoreTemplate.Modules.Configuracion.Domain/
    Aggregates/
      ConfiguracionItem.cs
    Enums/
      TipoValor.cs
    Events/
      ConfiguracionEvents.cs
    Repositories/
      IConfiguracionItemRepository.cs

  CoreTemplate.Modules.Configuracion.Application/
    Abstractions/
      IConfiguracionService.cs       ← contrato público para módulos consumidores
    Commands/
      ActualizarConfiguracionCommand.cs
    Queries/
      GetConfiguracionQuery.cs
      GetConfiguracionPorGrupoQuery.cs
      GetConfiguracionPorClaveQuery.cs
    DTOs/
      ConfiguracionItemDto.cs
      ConfiguracionGrupoDto.cs

  CoreTemplate.Modules.Configuracion.Infrastructure/
    Persistence/
      ConfiguracionDbContext.cs      schema: Configuracion
      Configurations/
        ConfiguracionItemConfiguration.cs
      ConfiguracionDataSeeder.cs
    Repositories/
      ConfiguracionItemRepository.cs
    Services/
      ConfiguracionService.cs        implementa IConfiguracionService con IMemoryCache
    DependencyInjection.cs

  CoreTemplate.Modules.Configuracion.Api/
    Controllers/
      ConfiguracionController.cs
```

---

## Endpoints

| Método | Ruta | Descripción | Permiso |
|---|---|---|---|
| GET | `/api/configuracion` | Listar todos (agrupados) | `Configuracion.Ver` |
| GET | `/api/configuracion/{clave}` | Obtener por clave | `Configuracion.Ver` |
| GET | `/api/configuracion/grupo/{grupo}` | Listar por grupo | `Configuracion.Ver` |
| PUT | `/api/configuracion/{clave}` | Actualizar valor | `Configuracion.Editar` |

---

**Fecha:** 2026-04-22
