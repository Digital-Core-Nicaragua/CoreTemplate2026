# Event Storming — Módulo Auth

## Estructura de Documentos

| Documento | Descripción |
|---|---|
| **01-BIG-PICTURE.md** | Vista panorámica: todos los eventos, comandos, actores y políticas |
| **02-PROCESS-LEVEL-PARTE1.md** | Procesos: Autenticación, Login, 2FA, Refresh, Logout |
| **02-PROCESS-LEVEL-PARTE2.md** | Procesos: Autorización, Permisos, Token Blacklist |
| **02-PROCESS-LEVEL-PARTE3.md** | Procesos: Sucursales, Roles por Sucursal, Límites de Sesiones |
| **03-DESIGN-LEVEL-PARTE1.md** | Design: Aggregates Usuario y Sesion |
| **03-DESIGN-LEVEL-PARTE2.md** | Design: Aggregates Rol, AsignacionRol y Accion |
| **03-DESIGN-LEVEL-PARTE3.md** | Design: Aggregates Sucursal y ConfiguracionTenant |
| **04-BOUNDED-CONTEXT-CANVAS.md** | Canvas de los 3 bounded contexts |
| **05-CONTEXT-MAPPING.md** | Relaciones entre bounded contexts |
| **06-EVENT-STORMING-LEGEND.md** | Leyenda y guía de notación |
| **07-HOTSPOTS-RESOLUTION.md** | 8 hotspots identificados y resueltos |

---

## Leyenda de Colores Event Storming

| Símbolo | Color | Elemento | Descripción |
|---|---|---|---|
| 🟠 | Naranja | **Evento de Dominio** | Algo que sucedió en el dominio |
| 🔵 | Azul | **Comando** | Intención de hacer algo |
| 🟡 | Amarillo | **Aggregate** | Entidad que procesa comandos |
| 🟣 | Morado | **Política** | Regla automática CUANDO→ENTONCES |
| 🟢 | Verde | **Read Model** | Consulta de información |
| 🔴 | Rojo | **Hotspot** | Problema o incertidumbre |
| 👤 | — | **Actor** | Usuario o sistema que ejecuta comandos |
| ⚡ | — | **Evento Externo** | Evento de otro bounded context |

---

## Bounded Contexts del Módulo Auth

```
┌─────────────────────────────────────────────────────────────┐
│                    MÓDULO AUTH                              │
├─────────────────┬───────────────────┬───────────────────────┤
│      IAM        │   Authorization   │    Configuration      │
│                 │                   │                       │
│ - Usuario       │ - Rol             │ - ConfiguracionTenant │
│ - Sesion        │ - AsignacionRol   │                       │
│                 │ - Accion          │                       │
│                 │ - Sucursal*       │                       │
└─────────────────┴───────────────────┴───────────────────────┘
* Sucursal pertenece a Organization, relacionado con Authorization
```

---

## Convenciones de Nomenclatura

### Eventos (tiempo pasado)
```
UsuarioRegistrado, SesionCreada, RolAsignado, SucursalAsignada
```

### Comandos (verbo infinitivo)
```
RegistrarUsuario, IniciarSesion, AsignarRol, CerrarSesion
```

### Políticas (CUANDO → ENTONCES)
```
CUANDO IntentosFallidos >= 5 ENTONCES BloquearCuenta
CUANDO PasswordCambiado ENTONCES RevocarTodasSesiones
CUANDO LimiteSesionesAlcanzado ENTONCES CerrarMasAntigua
```

---

**Estado:** ✅ Completo  
**Fecha:** 2026-04-15
