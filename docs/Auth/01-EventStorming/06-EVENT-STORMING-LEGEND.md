# Event Storming — Leyenda y Guía de Notación

> **Fecha:** 2026-04-15

---

## Elementos del Event Storming

| Símbolo | Color | Elemento | Formato | Descripción |
|---|---|---|---|---|
| 🟠 | Naranja | **Evento de Dominio** | Tiempo pasado | Algo que sucedió |
| 🔵 | Azul | **Comando** | Verbo infinitivo | Intención de hacer algo |
| 🟡 | Amarillo | **Aggregate** | Sustantivo | Procesa comandos, garantiza invariantes |
| 🟣 | Morado | **Política** | CUANDO→ENTONCES | Regla automática |
| 🟢 | Verde | **Read Model** | Consulta | Solo lectura |
| 🔴 | Rojo | **Hotspot** | Pregunta/Problema | Incertidumbre a resolver |
| 👤 | — | **Actor Humano** | Rol | Persona que ejecuta comandos |
| 🤖 | — | **Actor Sistema** | Servicio | Sistema que ejecuta comandos |
| ⚡ | — | **Evento Externo** | Tiempo pasado | Evento de otro contexto |

---

## Convenciones de Nomenclatura

### Eventos (tiempo pasado)
```
UsuarioRegistrado, SesionCreada, PasswordCambiado
RolAsignado, SucursalAsignada, DosFactoresActivado
```

### Comandos (verbo infinitivo)
```
RegistrarUsuario, IniciarSesion, CambiarPassword
AsignarRol, CrearSucursal, ActivarDosFactores
```

### Políticas (CUANDO → ENTONCES)
```
CUANDO IntentosFallidos >= 5 ENTONCES BloquearCuenta
CUANDO PasswordCambiado ENTONCES RevocarTodasSesiones
CUANDO LimiteSesionesAlcanzado ENTONCES CerrarMasAntigua
CUANDO PrimeraSucursalAsignada ENTONCES MarcarComoPrincipal
```

---

## Niveles del Event Storming

| Nivel | Objetivo | Resultado |
|---|---|---|
| **Big Picture** | Vista panorámica del dominio | Todos los eventos, bounded contexts, hotspots |
| **Process Level** | Detallar flujos críticos | Flujos paso a paso con actores y políticas |
| **Design Level** | Preparar para implementación | Aggregates con invariantes, repositorios |

---

**Fecha:** 2026-04-15
