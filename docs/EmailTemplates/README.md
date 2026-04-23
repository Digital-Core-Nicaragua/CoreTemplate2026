# EmailTemplates — Documentación

> **Tipo:** Módulo de negocio
> **Proyecto:** CoreTemplate

---

## Estructura de Documentos

| Documento | Descripción |
|---|---|
| **01-EventStorming/01-BIG-PICTURE.md** | Flujos de dominio, integración con Auth, políticas, hotspots |
| **02-Analisis-Tradicional/01-REQUERIMIENTOS-FUNCIONALES.md** | 12 RF con criterios de aceptación |
| **02-Analisis-Tradicional/02-CASOS-DE-USO.md** | 8 CU incluyendo flujos automáticos por evento |
| **02-Analisis-Tradicional/03-MODELO-DOMINIO-Y-CONTRATOS.md** | Aggregate, contratos, BD, estructura de proyectos, endpoints |

---

## Resumen

`EmailTemplates` permite gestionar plantillas de correo editables desde la UI.
El administrador puede personalizar el diseño y contenido de cada correo
sin necesidad de redeployar la aplicación.

**Cómo funciona:**
1. Las plantillas base viven como archivos `.html` en el proyecto (fallback)
2. El admin puede editarlas desde la UI → se guardan en BD
3. Al enviar, el sistema usa la versión de BD si existe, o el archivo si no
4. Las variables `{{NombreUsuario}}` se reemplazan automáticamente
5. Todos los correos se envuelven en el layout corporativo (`sistema.layout`)

**Soporte multi-tenant — plantillas por empresa:**
- Cada empresa puede tener su propio `sistema.layout` con su logo y colores
- Jerarquía: plantilla del tenant → plantilla global → archivo fallback
- `EmailTemplate` implementa `IHasTenant` — el aislamiento es automático
- Las plantillas globales (`TenantId = null`) usan `IgnoreQueryFilters()` para ser visibles desde cualquier tenant

---

## Plantillas incluidas desde el inicio

| Código | Trigger automático |
|---|---|
| `sistema.layout` | Layout base de todos los correos |
| `auth.reset-password` | Solicitud de restablecimiento de contraseña |
| `auth.cuenta-bloqueada` | Cuenta bloqueada (manual o por intentos) |
| `auth.bienvenida` | Registro de nuevo usuario |
| `auth.password-cambiado` | Cambio de contraseña exitoso |
| `auth.2fa-activado` | Activación de autenticación de dos factores |
| `auth.nueva-sesion` | Nueva sesión iniciada (configurable) |

---

## Relación con otros componentes

```
Auth (eventos)  ──► EmailTemplates (handlers) ──► IEmailTemplateSender
                                                        ├── IEmailTemplateRepository (BD)
                                                        ├── FallbackTemplateLoader (.html)
                                                        ├── ITemplateRenderer (variables + layout)
                                                        └── IEmailSender (Mailjet/SMTP/SendGrid)
```

---

**Estado:** Documentado — pendiente implementación
**Fecha:** 2026-04-22
