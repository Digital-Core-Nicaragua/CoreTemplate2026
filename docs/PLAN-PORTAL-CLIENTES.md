# Plan — Portal de Clientes (Configurable)

> **Estado**: 📋 Diseño completo — Pendiente de implementación
> **Prioridad**: Por definir
> **Depende de**: Fases 0-24 completadas

---

## Referencia: enfoque de LunaERP

LunaERP implementa dos piezas separadas:

**1. `UserType` enum** — incluye `Customer` como tipo en la tabla principal de usuarios.

**2. `CustomerUserStatus` enum** — ciclo de vida propio del usuario cliente:
```
Registered → Verified → Associated → (Blocked)
```
- `Registered`: se registró, email sin verificar
- `Verified`: verificó email y teléfono
- `Associated`: un admin lo vinculó a un cliente del ERP
- `Blocked`: bloqueado

**3. Login** — el cliente usa el **mismo endpoint** `POST /auth/login` que los usuarios internos.
El `LoginCommandHandler` detecta `UserType = Customer` y aplica sus propias reglas
(sin sucursal obligatoria, sin permisos de roles, claims diferenciados en el JWT).

**4. `CustomerUsersController`** — controller separado solo para el ciclo de vida del cliente:
```
POST /customer-users/register
POST /customer-users/{id}/verify-email
POST /customer-users/{id}/verify-phone
POST /customer-users/{id}/associate      ← admin vincula al cliente ERP
POST /customer-users/{id}/block
POST /customer-users/{id}/reactivate
```

**Conclusión del enfoque LunaERP**:
- Login compartido — un solo endpoint para todos los tipos de usuario
- Registro y ciclo de vida separados en controller propio
- El estado `Associated` lo hace específico de un ERP (existe entidad `Cliente` en el dominio)
- Para CoreTemplate como plantilla genérica, la Opción C (módulo separado) sigue siendo
  la más adecuada, pero el **login compartido** es un patrón válido a reutilizar.

---

## Dependencia crítica: el estado Associated

> ⚠️ **Leer antes de implementar**

En LunaERP existe el estado `Associated` en el ciclo de vida del cliente:

```
Registered → Verified → Associated → (Blocked)
```

Este estado representa el momento en que un admin vincula manualmente
el usuario del portal (`UsuarioCliente`) con la entidad de negocio (`Cliente`):

```
UsuarioCliente (Security)          Cliente (Dominio de negocio)
─────────────────────────          ────────────────────────────
juan@gmail.com          ────────►  "Empresa ABC S.A."
password hash                      RFC, dirección, crédito
estado: Associated                 pedidos, facturas, historial
```

Sin esta vinculación, el usuario puede hacer login pero no puede
acceder a los datos de negocio de su empresa (pedidos, facturas, etc.).

### Qué implica para CoreTemplate

Si se implementa el portal de clientes en CoreTemplate, **el estado `Associated`
no puede incluirse en la plantilla** porque:

- Requiere que exista una entidad `Cliente` en el dominio del negocio
- Esa entidad es específica de cada sistema (ERP, e-commerce, CRM, etc.)
- CoreTemplate no tiene ni puede tener esa entidad — es lógica de negocio

### Lo que sí incluiría CoreTemplate

```
Registered → Verified → Active → (Blocked)
```

- `Registered`: se registró, email sin verificar
- `Verified`: verificó email, puede hacer login y acceder al portal
- `Active`: estado operativo normal (equivalente a `Verified` confirmado)
- `Blocked`: bloqueado por admin

### Lo que cada sistema agrega encima

Cada sistema que use CoreTemplate como base define su propio estado de vinculación
según su dominio:

| Sistema | Estado equivalente a Associated |
|---|---|
| ERP | `Associated` → vinculado a entidad Cliente del ERP |
| E-commerce | `ProfileComplete` → completó datos de envío y pago |
| CRM | `Onboarded` → asignado a un ejecutivo de cuenta |
| SaaS | `Subscribed` → tiene plan activo |

Esto se implementa en el módulo de negocio correspondiente, no en CoreTemplate.

---

## Contexto

CoreTemplate actualmente contempla solo usuarios internos del sistema (`Humano`, `Sistema`, `Integracion`).
No existe soporte para usuarios clientes externos que accedan a un portal propio.

Esta feature se agregaría como flag configurable siguiendo el mismo patrón que `EnableBranches`:

```json
{
  "CustomerPortalSettings": {
    "EnableCustomerPortal": false
  }
}
```

Cuando está **desactivado** (default): comportamiento actual sin cambios.
Cuando está **activado**: habilita registro, login y gestión de clientes con flujo propio.

---

## Preguntas a responder antes de implementar

- [x] ¿Los clientes comparten la misma tabla `Usuarios` o tienen tabla propia?
      **Respuesta: Tabla propia** — `UsuarioCliente` aggregate independiente, tabla `Auth.UsuariosCliente`.
- [x] ¿Conviene seguir el enfoque de LunaERP (agregar `Cliente` al enum) o aggregate separado?
      **Respuesta: Aggregate separado en el mismo módulo Auth**, igual que LunaERP.
- [x] ¿La verificación de email es obligatoria o configurable?
      **Respuesta: Configurable** — flag `RequireEmailVerification` en `CustomerPortalSettings`.
- [x] ¿El registro de clientes es libre o requiere habilitación?
      **Respuesta: El admin debe habilitarlo** — flag `RegistroHabilitado` por tenant.
      Si está deshabilitado, el endpoint de registro retorna 403.
- [x] ¿Se requiere verificación de teléfono?
      **Respuesta: Configurable** — flag `RequirePhoneVerification` en `CustomerPortalSettings`.
- [x] ¿Los clientes tienen roles y permisos propios o acceso plano?
      **Respuesta: El acceso lo controla el estado del ciclo de vida**, igual que LunaERP.
      LunaERP no tiene roles para clientes — el estado (`Registered`, `Verified`, `Active`, `Blocked`)
      determina qué puede hacer el cliente. Ver sección "Permisos por estado" más abajo.
- [x] ¿Los clientes son por tenant o globales?
      **Respuesta: Por tenant** — cada tenant tiene sus propios clientes.
      `UsuarioCliente` incluye `TenantId` y el filtro multi-tenant aplica igual que en `Usuario`.
- [x] ¿Se necesita un JWT diferente (claims distintos) para clientes vs usuarios internos?
      **Respuesta: Sí** — endpoint separado `POST /api/portal/login`.
- [x] ¿Los clientes pueden ver/cerrar sus propias sesiones?
      **Respuesta: Sí, configurable** — flag `EnableSessionManagement` en `CustomerPortalSettings`.

---

## Permisos por estado

LunaERP no usa roles para clientes — el **estado del ciclo de vida** controla el acceso.
CoreSTemplate adopta el mismo patrón:

| Estado | Puede hacer login | Puede acceder al portal | Puede gestionar sesiones |
|---|---|---|---|
| `Registered` | ❌ No (email sin verificar) | ❌ No | ❌ No |
| `Verified` | ✅ Sí | ✅ Sí (acceso básico) | Según config |
| `Active` | ✅ Sí | ✅ Sí (acceso completo) | Según config |
| `Blocked` | ❌ No | ❌ No | ❌ No |

El claim `estado_cliente` se incluye en el JWT para que el sistema
pueda tomar decisiones de acceso sin consultar la BD en cada request.

---

## Opciones de diseño

### Opción A — Cliente como TipoUsuario adicional
Agregar `Cliente = 4` al enum `TipoUsuario` existente.

**Ventajas**: mínimo cambio, reutiliza toda la infraestructura actual.
**Desventajas**: mezcla usuarios internos y clientes en la misma tabla, complica queries y permisos.

> **Descartada** — LunaERP tiene `Customer` en el enum `UserType` pero en la práctica
> `CustomerUser` es un aggregate con tabla propia. El enum no es el mecanismo real de separación.

### Opción B — Aggregate separado `UsuarioCliente` en el módulo Auth
Nuevo aggregate con tabla propia dentro del mismo módulo Auth.

**Ventajas**: separación limpia, cada tipo evoluciona independiente, reutiliza DbContext y migraciones existentes.
**Desventajas**: crece el módulo Auth, puede volverse grande.

> **Recomendada** — es exactamente lo que hace LunaERP: `CustomerUser` vive en `SecurityDbContext`
> con tabla propia `CustomerUsers`, no en la tabla `Users`.

### Opción C — Módulo separado `CustomerPortal`
Módulo nuevo (`CoreTemplate.Modules.CustomerPortal.*`) con su propia capa Domain/Application/Infrastructure/Api.

**Ventajas**: aislamiento total, patrón consistente con la arquitectura modular existente.
**Desventajas**: mayor esfuerzo, duplica lógica de JWT y sesiones que ya existe en Auth.

> **Descartada por ahora** — la Opción B es suficiente y más pragmática.

---

## Alcance definido

### Funcionalidades base (siempre activas cuando `EnableCustomerPortal = true`)
- Aggregate `UsuarioCliente` con tabla propia `Auth.UsuariosCliente`
- Ciclo de vida: `Registered → Verified → Active → Blocked`
- Registro de cliente (email + contraseña) — solo si `RegistroHabilitado = true` por tenant
- Login separado `POST /api/portal/login` con JWT propio
- Refresh token y logout
- Cambio y restablecimiento de contraseña
- Bloqueo automático por intentos fallidos
- Clientes por tenant — filtro multi-tenant automático
- Admin puede activar/bloquear/reactivar clientes
- Admin habilita/deshabilita el registro por tenant

### Funcionalidades configurables
| Flag | Default | Descripción |
|---|---|---|
| `EnableCustomerPortal` | `false` | Activa todo el portal |
| `RequireEmailVerification` | `true` | Verificación de email al registrarse |
| `RequirePhoneVerification` | `false` | Verificación de teléfono |
| `EnableSessionManagement` | `false` | Cliente puede ver/cerrar sus sesiones |
| `OAuth.Google.Enabled` | `false` | Login con Google |
| `OAuth.Facebook.Enabled` | `false` | Login con Facebook |
| `RegistroPorTelefono.Enabled` | `false` | Habilita registro e identificación por número de teléfono (WhatsApp/SMS) |

### OAuth (configurable por proveedor)
- Login con Google y Facebook además de email/contraseña
- Contrato `IProveedorOAuthService` extensible para agregar más proveedores
- `UsuarioCliente` soporta múltiples proveedores vinculados al mismo email
- `PasswordHash` nullable — clientes OAuth no tienen contraseña local

### Lo que NO incluye
- Estado `Associated` — específico del dominio de negocio de cada sistema
- Configuración de apps en Google Console / Facebook Developers
- Flujo OAuth redirect en frontend
- Roles y permisos para clientes — el estado controla el acceso
- Lógica de negocio del portal (pedidos, historial, etc.)
- Frontend

---

## Impacto en la arquitectura actual

| Componente | Cambio |
|---|---|
| `TipoUsuario` enum | Sin cambio |
| Módulo Auth — Domain | Nuevo aggregate `UsuarioCliente` + `EstadoUsuarioCliente` enum + `IUsuarioClienteRepository` |
| Módulo Auth — Application | Nuevos commands: `RegistrarClienteCommand`, `VerificarEmailClienteCommand`, `LoginClienteCommand`, `LoginClienteGoogleCommand`, `LoginClienteFacebookCommand`, `BloquearClienteCommand`, `ReactivarClienteCommand` |
| Módulo Auth — Application | Nuevas abstracciones: `IProveedorOAuthService`, `CustomerPortalSettings` |
| Módulo Auth — Infrastructure | `AuthDbContext` agrega `DbSet<UsuarioCliente>`, nueva tabla `Auth.UsuariosCliente` |
| Módulo Auth — Infrastructure | `GoogleOAuthService`, `FacebookOAuthService` implementando `IProveedorOAuthService` |
| Módulo Auth — Api | Nuevo `PortalClientesController` con endpoints `/api/portal/*` |
| `LoginCommandHandler` | Sin cambio — no se toca |
| `Program.cs` | Registro condicional según `EnableCustomerPortal` |
| `appsettings.json` | Nueva sección `CustomerPortalSettings` |
| Base de datos | Nueva tabla `Auth.UsuariosCliente`, nueva migración `Add_UsuariosCliente` |

---

## Decisión: Login separado

**Resolución**: endpoint separado para clientes.

```
POST /api/auth/login       → usuarios internos (admin, empleados, integraciones)
POST /api/portal/login     → clientes externos
```

**Razones**:
- El portal de clientes es pública en internet — el login interno puede restringirse por IP o red interna
- Sin condicionales en el `LoginCommandHandler` existente — no se toca
- `LoginClienteCommandHandler` independiente con sus propias reglas:
  sin sucursales obligatorias, sin 2FA obligatorio, sin límite de sesiones por tipo
- Si el portal se desactiva (`EnableCustomerPortal = false`), el endpoint desaparece completamente

**Referencia**: LunaERP usa el mismo endpoint — CoreTemplate elige separado
por ser una plantilla genérica donde el portal puede ser público.

---

## Decisión: Autenticación externa para clientes (OAuth)

Los clientes podrán autenticarse con proveedores externos (Google, Facebook, etc.)
además del login tradicional con email y contraseña.

**Flujo con proveedor externo**:
```
1. Cliente hace clic en "Continuar con Google"
2. Redirige al proveedor OAuth
3. Proveedor retorna con token/code
4. POST /api/portal/login/google  { idToken: "..." }
5. Handler valida el token con Google
6. Si el email ya existe en UsuariosCliente → genera JWT propio
7. Si no existe → crea UsuarioCliente automáticamente (email ya verificado)
8. Retorna JWT de CoreTemplate (igual que login normal)
```

**Endpoints contemplados**:
```
POST /api/portal/login              → email + contraseña
POST /api/portal/login/google       → Google OAuth
POST /api/portal/login/facebook     → Facebook OAuth
```

**Diseño**:
- Cada proveedor es un command independiente: `LoginClienteGoogleCommand`, `LoginClienteFacebookCommand`
- Contrato común `IProveedorOAuthService` — cada proveedor implementa la validación del token externo
- El JWT que se genera siempre es el propio de CoreTemplate — el token de Google/Facebook
  nunca sale del handler
- `UsuarioCliente` agrega campo `ProveedorExterno` (Google, Facebook, Local) y `ExternalId`
- Un cliente puede tener múltiples proveedores vinculados al mismo email

**Lo que NO incluye CoreTemplate**:
- Configuración de las apps en Google Console / Facebook Developers — cada sistema la hace
- Flujo de redirect OAuth en el frontend — es responsabilidad del cliente web/mobile
- CoreTemplate solo valida el `idToken` o `accessToken` que el frontend ya obtuvo

**Configuración**:
```json
{
  "CustomerPortalSettings": {
    "EnableCustomerPortal": false,
    "RequireEmailVerification": true,
    "OAuth": {
      "Google": {
        "Enabled": false,
        "ClientId": ""
      },
      "Facebook": {
        "Enabled": false,
        "AppId": "",
        "AppSecret": ""
      }
    }
  }
}
```

**Impacto en `UsuarioCliente`**:

| Campo nuevo | Descripción |
|---|---|
| `ProveedorOrigen` | `Local`, `Google`, `Facebook` |
| `ExternalId` | ID del usuario en el proveedor externo |
| `PasswordHash` | Nullable — clientes OAuth no tienen contraseña local |

---

## Referencias

- `docs/ALCANCE.md` — sección 3: "Qué NO incluye la plantilla"
- `docs/Auth/02-Analisis-Tradicional/07-REQUERIMIENTOS-FUNCIONALES.md` — RF de usuarios internos
- Patrón a seguir: `docs/Auth/03-Implementacion/01-ESTRUCTURA-PROYECTOS.md`

---

## Registro por Teléfono / WhatsApp (Fase 25)

> **Estado**: 📋 Diseño completo — Pendiente de implementación
> **Depende de**: Portal de Clientes implementado (aggregate `UsuarioCliente` existente)

### Motivación

El registro por WhatsApp permite que clientes se identifiquen con su número de teléfono en lugar de (o además de) un email. Es especialmente útil en mercados donde WhatsApp es el canal de comunicación principal.

### Regla de negocio central

Un cliente debe tener **al menos uno** de los dos identificadores:
- Email (registro tradicional)
- Teléfono (registro por WhatsApp/SMS)

Ambos son opcionales individualmente, pero no pueden estar ambos ausentes:

```
Email presente, Teléfono ausente   → ✅ válido (registro tradicional)
Email ausente,  Teléfono presente  → ✅ válido (registro por WhatsApp)
Email presente, Teléfono presente  → ✅ válido (ambos)
Email ausente,  Teléfono ausente   → ❌ inválido
```

### Flujo de registro por WhatsApp

```
1. Cliente ingresa su número de teléfono
2. POST /api/portal/registro  { telefono: "+521234567890" }
3. Sistema genera OTP de 6 dígitos, expira en 10 minutos
4. Sistema envía OTP por WhatsApp (vía INotificacionClienteService)
5. POST /api/portal/verificar-telefono  { telefono, codigo }
6. Sistema verifica OTP → cliente queda en estado Active
7. Sistema retorna JWT
```

### Cambios en el aggregate `UsuarioCliente`

| Cambio | Detalle |
|---|---|
| `Email` | Pasa a ser nullable (`Email?`) |
| `Telefono` | Pasa a ser identificador único (indexado) |
| `TipoRegistro` | Nuevo enum: `Email`, `Telefono`, `OAuth` |
| Validación en factory | Al menos email o teléfono requerido |
| `Crear()` | Acepta email nullable |
| `CrearPorTelefono()` | Nuevo factory method sin email |

### Nuevo Value Object `Telefono`

Validación de formato E.164 (`+521234567890`):
- Debe comenzar con `+`
- Solo dígitos después del `+`
- Entre 7 y 15 dígitos

### Nuevo enum `TipoRegistro`

```csharp
public enum TipoRegistro
{
    Email = 1,     // Registro con email + contraseña
    Telefono = 2,  // Registro con teléfono + OTP
    OAuth = 3      // Registro con proveedor externo (Google, Facebook)
}
```

### Cambios en la configuración

```json
{
  "CustomerPortalSettings": {
    "RegistroPorTelefono": {
      "Enabled": false,
      "Proveedor": "WhatsApp",
      "OtpExpirationMinutes": 10
    }
  }
}
```

### Cambios en la base de datos

| Cambio | Detalle |
|---|---|
| Columna `Email` | Pasa a nullable |
| Índice `IX_UsuariosCliente_Email_TenantId` | Agrega filtro `WHERE Email IS NOT NULL` |
| Nuevo índice `IX_UsuariosCliente_Telefono_TenantId` | Único, filtrado `WHERE Telefono IS NOT NULL` |
| Nueva columna `TipoRegistro` | `int NOT NULL` |

### Nuevo contrato `INotificacionClienteService`

```csharp
public interface INotificacionClienteService
{
    Task EnviarOtpWhatsAppAsync(string telefono, string codigo, CancellationToken ct);
    Task EnviarOtpSmsAsync(string telefono, string codigo, CancellationToken ct);
}
```

CoreTemplate define el contrato. Cada sistema implementa el proveedor (Twilio, AWS SNS, etc.).

### Archivos a crear/modificar

| Archivo | Cambio |
|---|---|
| `Domain/ValueObjects/Telefono.cs` | NUEVO — Value Object con validación E.164 |
| `Domain/Enums/TipoRegistro.cs` | NUEVO — enum Email/Telefono/OAuth |
| `Domain/Aggregates/UsuarioCliente.cs` | Email nullable, nuevo factory `CrearPorTelefono()`, validación al menos uno |
| `Application/Abstractions/INotificacionClienteService.cs` | NUEVO — contrato de notificaciones |
| `Application/Abstractions/CustomerPortalSettings.cs` | Agregar `RegistroPorTelefono` settings |
| `Application/Commands/RegistrarClienteCommand` | Aceptar email nullable, teléfono opcional |
| `Application/Commands/RegistrarClientePorTelefonoCommand` | NUEVO |
| `Infrastructure/Configurations/UsuarioClienteConfiguration.cs` | Email nullable, índice filtrado en Email, nuevo índice en Teléfono |
| `Infrastructure/Migrations/` | Nueva migración `Add_RegistroPorTelefono` |
| `Api/Controllers/PortalClientesController.cs` | Nuevo endpoint `POST /registro/telefono`, `POST /verificar-telefono` |

*Documento actualizado para registrar la idea. Revisar preguntas pendientes antes de decidir implementación.*
