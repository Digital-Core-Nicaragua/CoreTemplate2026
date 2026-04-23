# Event Storming — Big Picture
# Módulo: EmailTemplates

> **Nivel:** Big Picture + Process Level
> **Fecha:** 2026-04-22
> **Bounded Context:** Email Templates (gestión de plantillas editables)

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
| ⚡ | — | Evento externo (de otro módulo) |
| 👤 | — | Actor humano |
| 🤖 | — | Sistema automático |

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| 👤 **Administrador** | Humano | Crea, edita y gestiona plantillas |
| 🤖 **Seeder** | Automático | Registra plantillas del sistema al arrancar |
| 🤖 **Módulo Auth** | Sistema | Publica eventos que disparan envíos |
| 🤖 **Módulo RRHH** | Sistema | Usa `IEmailTemplateSender` directamente |
| 🤖 **Módulo Nómina** | Sistema | Usa `IEmailTemplateSender` directamente |

---

## Flujo: Seed inicial de plantillas del sistema

```
🤖 Seeder → 🔵 SeedPlantillasSistema
    Por cada plantilla del sistema (auth.reset-password, auth.cuenta-bloqueada, etc.):

    ¿Existe en BD?
    2a. SI existe → no sobreescribir (el admin puede haberla editado)
        → Omitir

    2b. NO existe:
        🟡 EmailTemplate → Crear desde archivo .html del proyecto
        🟠 PlantillaCreada { codigo, modulo, esDeSistema: true }
        → Guardar en BD
```

---

## Flujo: Administrador edita una plantilla

```
👤 Administrador → 🔵 ActualizarPlantilla { id, asunto, cuerpoHtml, variables }
    🟡 EmailTemplate → Verificar que existe
    🟡 EmailTemplate → Verificar que no está bloqueada para edición

    3a. SI es plantilla de sistema con código protegido:
        → Puede editar asunto y cuerpo, NO puede cambiar código ni eliminar

    3b. SI es plantilla personalizada:
        → Puede editar todo excepto el código (inmutable)

    🟡 EmailTemplate → Actualizar(asunto, cuerpoHtml, variables)
    🟠 PlantillaActualizada { templateId, codigo, modificadoPor }
    → Retornar 200
```

---

## Flujo: Administrador previsualiza plantilla

```
👤 Administrador → 🔵 PreviewPlantilla { id, variables: {...} }
    🟢 ConsultarPlantilla { id }
    ITemplateRenderer → Reemplazar {{Variables}} en asunto y cuerpo
    ITemplateRenderer → Aplicar layout sistema.layout (si UsarLayout = true)
    → Retornar HTML renderizado (sin enviar correo)
    🟠 PlantillaPrevisualizadaEvent (solo para log, no persiste)
```

---

## Flujo: Envío automático por evento de Auth

```
⚡ RestablecimientoSolicitadoEvent { email, token, expiraEn } (de Auth)
    🟣 POLÍTICA: Si handler habilitado en config → procesar

    RestablecimientoSolicitadoHandler:
    🔵 EnviarConPlantilla {
        codigo: "auth.reset-password",
        para: event.Email,
        variables: { NombreUsuario, LinkReset, ExpiraEn }
    }

    IEmailTemplateSender:
    → Resolver plantilla:
        2a. ¿Existe en BD y está activa? → usar BD
        2b. ¿No existe en BD? → cargar auth.reset-password.html del proyecto
        2c. ¿No existe archivo? → error al arrancar (fail-fast en seed)

    → ITemplateRenderer.RenderizarAsync(plantilla, variables)
        → Reemplazar variables en asunto
        → Reemplazar variables en cuerpo
        → Inyectar variables globales (SistemaNombre, SistemaUrl, AnioActual)
        → Aplicar layout sistema.layout

    → IEmailSender.EnviarAsync(EmailMessage renderizado)

    3a. SI envío exitoso:
        🟠 CorreoEnviado { destinatario, codigoTemplate, proveedor }

    3b. SI envío fallido:
        🟠 CorreoFallido { destinatario, codigoTemplate, error }
        🟣 POLÍTICA: Log warning — NO revertir operación de Auth
```

---

## Flujo: Módulo consumidor envía con plantilla

```
🤖 Módulo Nómina → 🔵 EnviarConPlantilla {
    codigo: "nomina.comprobante-pago",
    para: empleado.Email,
    variables: { NombreEmpleado, Periodo, SalarioNeto, LinkComprobante },
    adjuntos: [pdfBytes]
}

IEmailTemplateSender → (mismo flujo de resolución que arriba)
    → Renderizar → Enviar via IEmailSender
    🟠 CorreoEnviado / CorreoFallido
```

---

## Flujo: Envío de prueba

```
👤 Administrador → 🔵 EnviarPrueba { templateId, destinatario, variables }
    🟢 ConsultarPlantilla { templateId }
    IEmailTemplateSender → Renderizar + Enviar
    🟠 CorreoDePruebaEnviado { templateId, destinatario, resultado }
    → Retornar resultado al admin
```

---

## Políticas Automáticas

| # | Política | Trigger | Acción |
|---|---|---|---|
| P1 | Seed al arrancar | Aplicación inicia | Crear plantillas del sistema si no existen |
| P2 | Fallback a archivo | Plantilla no en BD | Cargar .html del proyecto |
| P3 | No revertir Auth | Fallo de envío | Log warning, continuar |
| P4 | Variables globales | Cualquier renderizado | Inyectar SistemaNombre, SistemaUrl, AnioActual |
| P5 | Layout automático | `UsarLayout = true` | Envolver cuerpo en sistema.layout |
| P6 | Handler configurable | Evento de Auth | Solo procesar si habilitado en config |

---

## Eventos de Dominio del Módulo

| Evento | Trigger | Datos |
|---|---|---|
| `PlantillaCreada` | Crear plantilla | templateId, codigo, modulo |
| `PlantillaActualizada` | Editar plantilla | templateId, codigo, modificadoPor |
| `PlantillaActivada` | Activar | templateId, codigo |
| `PlantillaDesactivada` | Desactivar | templateId, codigo |

---

## Integración con eventos de Auth

```
Auth publica:                          EmailTemplates consume:
─────────────────────────────────────────────────────────────
RestablecimientoSolicitadoEvent   →   auth.reset-password
UsuarioBloqueadoEvent             →   auth.cuenta-bloqueada
PasswordCambiadoEvent             →   auth.password-cambiado
DosFactoresActivadoEvent          →   auth.2fa-activado
UsuarioRegistradoEvent            →   auth.bienvenida (configurable)
SesionCreadaEvent                 →   auth.nueva-sesion (configurable)
```

> Estos handlers se registran en `EmailTemplates.Infrastructure` — Auth no sabe nada
> de EmailTemplates. El acoplamiento es solo via eventos de dominio.

---

## Hotspots Identificados

| # | Hotspot | Resolución |
|---|---|---|
| H1 | ¿Qué pasa si el admin rompe el HTML de una plantilla? | Preview antes de guardar. Si el HTML es inválido el correo se verá mal — responsabilidad del admin. |
| H2 | ¿Cómo manejar plantillas en multi-tenant? | Jerarquía: Tenant → Global → Archivo. TenantId en el índice único. |
| H3 | ¿El layout se aplica a todos los correos? | Por defecto sí. Cada plantilla tiene `UsarLayout` para desactivarlo. |
| H4 | ¿Qué pasa si se desactiva `auth.reset-password`? | El handler verifica si la plantilla está activa. Si no → no envía (log warning). El token de reset sigue válido. |
| H5 | ¿Soporte para templates en múltiples idiomas? | No en v1. En v2: agregar `Idioma` al aggregate y resolver por preferencia del usuario. |
| H6 | ¿Historial de cambios en plantillas? | No en v1. `ModificadoPor` y `ModificadoEn` son suficientes. Historial completo en v2. |

---

**Estado:** Documentado
**Fecha:** 2026-04-22

---

## Addendum — Multi-tenant

> **Fecha:** 2026-04-22

### Flujo: Resolución de plantilla en modo multi-tenant

```
🤖 EmailTemplateSender → Resolver plantilla "auth.reset-password"

    Paso 1: Buscar en BD con TenantId = tenant-actual
    ¿Existe plantilla personalizada del tenant?
    2a. SÍ → usar plantilla del tenant (logo y diseño de la empresa)
    2b. NO → continuar

    Paso 2: Buscar en BD con TenantId = null (IgnoreQueryFilters)
    ¿Existe plantilla global del sistema?
    3a. SÍ → usar plantilla global
    3b. NO → continuar

    Paso 3: Cargar archivo auth-reset-password.html del proyecto
    ¿Existe el archivo?
    4a. SÍ → usar archivo fallback
    4b. NO → 🟠 PlantillaNoEncontrada → retornar error
```

### Flujo: Admin de tenant personaliza su layout corporativo

```
👤 Admin Tenant A → 🔵 CrearPlantilla {
    codigo: "sistema.layout",
    tenantId: tenant-A,
    cuerpoHtml: "<html>... logo Empresa ABC ...</html>"
}
🟡 EmailTemplate → Crear con TenantId = tenant-A
🟠 PlantillaCreada { codigo: "sistema.layout", tenantId: tenant-A }

Desde ese momento:
→ Todos los correos del Tenant A usan su layout corporativo
→ Los demás tenants no se ven afectados
```

### Política de aislamiento

```
🟣 POLÍTICA: QueryFilter automático de BaseDbContext
    CUANDO IsMultiTenant = true
    ENTONCES filtrar EmailTemplate por TenantId = currentTenant.TenantId
    EXCEPTO cuando se buscan plantillas globales (TenantId = null)
    → Usar IgnoreQueryFilters() explícitamente
```

---

**Fecha addendum:** 2026-04-22
