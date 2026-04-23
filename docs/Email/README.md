# Email — Documentación

> **Tipo:** Building Block transversal
> **Proyecto:** CoreTemplate

---

## Estructura de Documentos

| Documento | Descripción |
|---|---|
| **01-EventStorming/01-BIG-PICTURE.md** | Eventos de infraestructura, integraciones con módulos consumidores, hotspots |
| **02-Analisis-Tradicional/01-REQUERIMIENTOS-FUNCIONALES.md** | 8 RF con criterios de aceptación |
| **02-Analisis-Tradicional/02-REQUERIMIENTOS-NO-FUNCIONALES.md** | Intercambiabilidad, resiliencia, seguridad, rendimiento |
| **02-Analisis-Tradicional/03-CASOS-DE-USO.md** | CU por módulo consumidor, matriz de uso |
| **02-Analisis-Tradicional/04-MODELO-DOMINIO-Y-CONTRATOS.md** | Contratos, configuración, estructura de proyectos |

---

## Resumen

`CoreTemplate.Email` es un building block de infraestructura transversal.
Cualquier módulo inyecta `IEmailSender` y envía correos sin conocer el proveedor.

**Proveedores soportados:**
- `Mailjet` — API transaccional (producción)
- `Smtp` — servidor SMTP estándar (desarrollo / servidores propios)
- `SendGrid` — preparado para implementar

**Cambiar proveedor:** solo modificar `appsettings.json`, sin tocar código.

---

## Módulos consumidores

| Módulo | Cómo consume |
|---|---|
| EmailTemplates | Usa `IEmailSender` internamente — es el consumidor principal |
| Auth | Via eventos → EmailTemplates los maneja |
| RRHH | Via `IEmailTemplateSender` (con plantillas) |
| Nómina | Via `IEmailTemplateSender` (con plantillas + adjuntos) |
| Contabilidad | Via `IEmailTemplateSender` (con plantillas + adjuntos) |

> Los módulos de negocio NO usan `IEmailSender` directamente.
> Usan `IEmailTemplateSender` del módulo EmailTemplates para tener diseño corporativo.
> Solo EmailTemplates depende de `IEmailSender`.

---

**Estado:** Documentado — pendiente implementación
**Fecha:** 2026-04-22
