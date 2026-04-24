# Event Storming — Big Picture
# Módulo: Configuración del Sistema

> **Nivel:** Big Picture + Process Level
> **Fecha:** 2026-04-22

---

## Leyenda

| Símbolo | Color | Elemento |
|---|---|---|
| 🟠 | Naranja | Evento de dominio |
| 🔵 | Azul | Comando |
| 🟡 | Amarillo | Aggregate |
| 🟣 | Morado | Política |
| 🟢 | Verde | Read Model |
| 🔴 | Rojo | Hotspot |
| 👤 | — | Actor humano |
| 🤖 | — | Sistema automático |

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| 👤 **Administrador** | Humano | Gestiona los parámetros del sistema |
| 🤖 **Seeder** | Automático | Crea parámetros por defecto al arrancar |
| 🤖 **Módulo Consumidor** | Sistema | Cualquier módulo que lee parámetros |

---

## Flujo: Seed inicial de parámetros

```
🤖 Seeder → 🔵 SeedConfiguracion
    Por cada parámetro del sistema:

    ¿Existe en BD?
    2a. SÍ → no sobreescribir (admin puede haberlo editado)
    2b. NO:
        🟡 ConfiguracionItem → Crear con valor por defecto
        🟠 ConfiguracionCreada { clave, valor, grupo }
        → Guardar en BD
```

---

## Flujo: Administrador actualiza un parámetro

```
👤 Administrador → 🔵 ActualizarConfiguracion {
    clave: "sistema.nombre",
    valor: "Mi ERP S.A."
}
    🟡 ConfiguracionItem → Verificar que existe
    🟡 ConfiguracionItem → Verificar que EsEditable = true

    3a. NO es editable:
        🟠 ConfiguracionNoEditable → Error 400

    3b. SÍ es editable:
        🟡 ConfiguracionItem → Actualizar(valor, modificadoPor)
        🟠 ConfiguracionActualizada { clave, valorAnterior, valorNuevo }
        🟣 POLÍTICA: Invalidar cache de esa clave
        → Retornar 200
```

---

## Flujo: Módulo consumidor lee un parámetro

```
🤖 Módulo Consumidor → IConfiguracionService.ObtenerStringAsync("sistema.nombre")

    IConfiguracionService:
    1. ¿Existe en cache?
    2a. SÍ (TTL vigente) → retornar valor del cache (sin consulta a BD)
    2b. NO:
        🟢 ConsultarConfiguracion { clave, tenantId }
        
        ¿Existe para el tenant actual?
        3a. SÍ → usar valor del tenant
        3b. NO → usar valor global (TenantId = null, IgnoreQueryFilters)
        3c. NO existe ninguno → retornar valorPorDefecto
        
        → Guardar en cache (TTL 10 minutos)
        → Retornar valor
```

---

## Flujo: Multi-tenant — cada empresa tiene sus parámetros

```
Tenant A configura:
    🔵 ActualizarConfiguracion { clave: "sistema.nombre", valor: "Empresa ABC" }
    → Se guarda con TenantId = tenant-A

Tenant B no ha configurado "sistema.nombre":
    IConfiguracionService.ObtenerStringAsync("sistema.nombre")
    → No encuentra para tenant-B
    → Usa valor global: "Mi Sistema" (TenantId = null)

Single-tenant:
    → Todos los parámetros tienen TenantId = null
    → Mismo comportamiento, sin complejidad adicional
```

---

## Políticas Automáticas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Seed al arrancar | Aplicación inicia | Crear parámetros si no existen |
| P2 | No sobreescribir | Parámetro ya existe en BD | Omitir en el seed |
| P3 | Invalidar cache | ConfiguracionActualizada | Eliminar entrada del cache |
| P4 | Fallback a global | Parámetro no existe para tenant | Usar valor global (TenantId = null) |
| P5 | Fallback a default | No existe en BD | Retornar valorPorDefecto del código |

---

## Eventos de Dominio

| Evento | Trigger | Datos |
|---|---|---|
| `ConfiguracionCreada` | Seed o crear nuevo | clave, valor, grupo |
| `ConfiguracionActualizada` | Actualizar valor | clave, valorAnterior, valorNuevo, modificadoPor |

---

## Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Cache se invalida en múltiples instancias? | Con Redis distribuido. Sin Redis → TTL corto (1 min) acepta eventual consistency. |
| H2 | ¿Qué pasa si se borra un parámetro del seed? | El código siempre tiene `valorPorDefecto` como fallback. |
| H3 | ¿Parámetros sensibles (contraseñas) en esta tabla? | NO. Esta tabla es para parámetros de negocio. Credenciales van en variables de entorno. |
| H4 | ¿Validación del valor según el tipo? | Al actualizar, validar que el valor sea compatible con `Tipo` (Number, Boolean, Json). |

---

**Estado:** Documentado
**Fecha:** 2026-04-22
