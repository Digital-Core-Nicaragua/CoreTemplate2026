# EmailTemplates — Casos de Uso

> **Módulo:** CoreTemplate.Modules.EmailTemplates
> **Fecha:** 2026-04-22

---

## Actores

| Actor | Tipo | Descripción |
|---|---|---|
| **Administrador** | Humano | Gestiona y personaliza las plantillas |
| **Módulo Consumidor** | Sistema | Auth, RRHH, Nómina, Contabilidad — usan `IEmailTemplateSender` |
| **Sistema (Seeder)** | Automático | Registra plantillas base al arrancar |
| **Evento de Dominio** | Automático | Dispara envíos automáticos (ej: `RestablecimientoSolicitadoEvent`) |

---

## CU-ET-001: Editar plantilla de correo

**Actor:** Administrador
**Permiso requerido:** `EmailTemplates.Editar`

**Flujo principal:**
1. Admin navega a `/admin/email-templates`
2. Selecciona la plantilla `auth.reset-password`
3. Edita el asunto: `"Restablece tu contraseña en {{SistemaNombre}}"`
4. Edita el cuerpo HTML con el nuevo diseño corporativo
5. El sistema valida que las variables declaradas estén en el HTML
6. Guarda → `PlantillaActualizada`
7. Desde ese momento todos los correos de reset usan el nuevo diseño

**Flujo alternativo — plantilla del sistema:**
- Si `EsDeSistema = true` → puede editar asunto y cuerpo, pero NO puede eliminar ni cambiar el código

---

## CU-ET-002: Previsualizar plantilla

**Actor:** Administrador
**Permiso requerido:** `EmailTemplates.Ver`

**Flujo principal:**
1. Admin abre la plantilla `auth.reset-password`
2. Hace clic en "Vista previa"
3. El sistema solicita valores de ejemplo para las variables:
   ```json
   {
     "NombreUsuario": "Juan Pérez",
     "LinkReset": "https://misistema.com/reset?token=ABC123",
     "ExpiraEn": "1 hora"
   }
   ```
4. El sistema renderiza el HTML con las variables reemplazadas y el layout aplicado
5. Muestra el resultado en el navegador (iframe o nueva pestaña)
6. No se envía ningún correo

---

## CU-ET-003: Enviar correo de prueba

**Actor:** Administrador
**Permiso requerido:** `EmailTemplates.EnviarPrueba`

**Flujo principal:**
1. Admin abre la plantilla `nomina.comprobante-pago`
2. Hace clic en "Enviar prueba"
3. Ingresa: destinatario `admin@misistema.com` y variables de ejemplo
4. El sistema envía el correo real usando el proveedor configurado
5. Retorna confirmación con el messageId del proveedor

**Flujo alternativo — fallo de envío:**
- El proveedor retorna error
- El sistema muestra el mensaje de error al admin
- Registra en log como "envío de prueba fallido"

---

## CU-ET-004: Crear plantilla personalizada

**Actor:** Administrador
**Permiso requerido:** `EmailTemplates.Gestionar`

**Flujo principal:**
1. Admin hace clic en "Nueva plantilla"
2. Ingresa:
   - Código: `rrhh.notificacion-candidato` (único, inmutable)
   - Nombre: "Notificación a candidato"
   - Módulo: "RRHH"
   - Asunto: `"Actualización sobre tu proceso en {{SistemaNombre}}"`
   - Cuerpo HTML: diseño personalizado
   - Variables disponibles: `NombreCandidato`, `EstadoProceso`, `ProximoPaso`
3. Guarda → `PlantillaCreada`
4. El módulo RRHH puede usar esta plantilla con `IEmailTemplateSender`

---

## CU-ET-005: Envío automático por evento de dominio

**Actor:** Evento de Dominio (automático)
**Precondición:** Handler registrado en DI, plantilla activa

**Flujo — reset de contraseña:**
```
1. Auth handler ejecuta SolicitarRestablecimientoCommand
2. Usuario.SolicitarRestablecimiento() → genera token
3. Publica RestablecimientoSolicitadoEvent { usuarioId, email, token, expiraEn }
4. EmailTemplates.RestablecimientoSolicitadoHandler recibe el evento
5. Llama IEmailTemplateSender.EnviarAsync(
       codigo: "auth.reset-password",
       para: event.Email,
       variables: {
           NombreUsuario: usuario.Nombre,
           LinkReset: $"{baseUrl}/reset?token={event.Token}",
           ExpiraEn: "1 hora"
       }
   )
6. IEmailTemplateSender resuelve la plantilla (BD → fallback)
7. Renderiza: reemplaza variables + aplica layout
8. Delega a IEmailSender → Mailjet/SMTP/SendGrid
9. Si falla → log warning, NO revertir el token de Auth
```

---

## CU-ET-006: Módulo consumidor envía correo con plantilla

**Actor:** Módulo Consumidor (ej: Nómina)
**Precondición:** Plantilla `nomina.comprobante-pago` activa

**Flujo:**
```
1. NominaHandler genera PDF del comprobante
2. Sube PDF via IStorageService → obtiene URL
3. Llama IEmailTemplateSender.EnviarAsync(
       codigo: "nomina.comprobante-pago",
       para: empleado.Email,
       variables: {
           NombreEmpleado: "Juan Pérez",
           Periodo: "Enero 2025",
           SalarioNeto: "$2,500.00",
           LinkComprobante: url
       },
       adjuntos: [pdfBytes]
   )
4. Correo enviado con diseño corporativo y PDF adjunto
```

---

## CU-ET-007: Activar / desactivar plantilla

**Actor:** Administrador
**Permiso requerido:** `EmailTemplates.Gestionar`

**Flujo — desactivar:**
1. Admin desactiva la plantilla `auth.nueva-sesion`
2. Desde ese momento, cuando se dispara `SesionCreadaEvent`, el handler verifica
   que la plantilla está inactiva → no envía correo (sin error)
3. El sistema usa el fallback de archivo si existe, o simplemente omite el envío

**Flujo — activar:**
1. Admin activa la plantilla `rrhh.notificacion-candidato`
2. Desde ese momento los envíos usan esta plantilla

---

## CU-ET-008: Editar layout base

**Actor:** Administrador
**Permiso requerido:** `EmailTemplates.Gestionar`

**Flujo:**
1. Admin edita la plantilla especial `sistema.layout`
2. Cambia el logo: `<img src="{{SistemaLogoUrl}}" />`
3. Cambia colores corporativos en el CSS inline
4. Guarda → todos los correos futuros usan el nuevo layout
5. Puede previsualizar con cualquier plantilla de contenido

---

## Matriz de plantillas por módulo

| Módulo | Código | Trigger |
|---|---|---|
| Sistema | `sistema.layout` | Layout base de todos los correos |
| Auth | `auth.reset-password` | `RestablecimientoSolicitadoEvent` |
| Auth | `auth.cuenta-bloqueada` | `UsuarioBloqueadoEvent` |
| Auth | `auth.bienvenida` | `UsuarioRegistradoEvent` |
| Auth | `auth.password-cambiado` | `PasswordCambiadoEvent` |
| Auth | `auth.2fa-activado` | `DosFactoresActivadoEvent` |
| Auth | `auth.nueva-sesion` | `SesionCreadaEvent` (configurable) |
| RRHH | `rrhh.notificacion-candidato` | Manual desde módulo RRHH |
| Nómina | `nomina.comprobante-pago` | `NominaCalculadaEvent` |
| Contabilidad | `contabilidad.factura-emitida` | `FacturaEmitidaEvent` |

> Las plantillas de RRHH, Nómina y Contabilidad se crean cuando se implementen esos módulos.
> El módulo EmailTemplates ya estará listo para recibirlas.

---

**Fecha:** 2026-04-22
