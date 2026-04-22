# Event Storming — Portal de Clientes

> **Fecha:** Pendiente de implementación
> **Estado:** Diseño completo — pendiente de desarrollo
> **Nivel:** Design Level

---

## Flujo 1: Registro de Cliente (email + contraseña)

```
[Admin]                    [Cliente]                    [Sistema]
   |                           |                            |
   | Habilita registro         |                            |
   |-------------------------->|                            |
   |                           |                            |
   |                    POST /api/portal/registro           |
   |                           |--------------------------->|
   |                           |                     Valida email único
   |                           |                     Valida contraseña
   |                           |                     Crea UsuarioCliente
   |                           |                     Estado: Registered
   |                           |<---------------------------|
   |                           |   ClienteRegistradoEvent   |
   |                           |                            |
   |              (Si RequireEmailVerification = true)      |
   |                           |                     Genera token email
   |                           |                     Envía email verificación
   |                           |                            |
   |              (Si RequireEmailVerification = false)     |
   |                           |                     Estado: Active
```

**Eventos de dominio:**
- 🟧 `ClienteRegistradoEvent`
- 🟧 `ClienteActivadoEvent` (si no requiere verificación)

---

## Flujo 2: Verificación de Email

```
[Cliente]                                          [Sistema]
   |                                                   |
   | Recibe email con token                            |
   |                                                   |
   | POST /api/portal/verificar-email { token }        |
   |-------------------------------------------------->|
   |                                            Valida token
   |                                            Valida expiración (24h)
   |                                            EmailVerificado = true
   |                                            Estado: Verified
   |<--------------------------------------------------|
   |                                    ClienteEmailVerificadoEvent
   |                                                   |
   |         (Si RequirePhoneVerification = false)     |
   |                                            Estado: Active
   |                                    ClienteActivadoEvent
```

**Eventos de dominio:**
- 🟧 `ClienteEmailVerificadoEvent`
- 🟧 `ClienteActivadoEvent` (si no requiere verificación de teléfono)

---

## Flujo 3: Verificación de Teléfono

```
[Cliente]                                          [Sistema]
   |                                                   |
   | POST /api/portal/verificar-telefono { codigo }    |
   |-------------------------------------------------->|
   |                                            Valida código (6 dígitos)
   |                                            Valida expiración (10min)
   |                                            TelefonoVerificado = true
   |                                            Estado: Active
   |<--------------------------------------------------|
   |                               ClienteTelefonoVerificadoEvent
   |                               ClienteActivadoEvent
```

**Eventos de dominio:**
- 🟧 `ClienteTelefonoVerificadoEvent`
- 🟧 `ClienteActivadoEvent`

---

## Flujo 4: Login con Email y Contraseña

```
[Cliente]                                          [Sistema]
   |                                                   |
   | POST /api/portal/login { email, password }        |
   |-------------------------------------------------->|
   |                                            Busca UsuarioCliente por email+tenant
   |                                            Verifica contraseña
   |                                            Verifica estado (Verified o Active)
   |                                            Verifica bloqueo
   |                                            Crea Sesion
   |                                            Genera JWT (claims: tipo=cliente)
   |<--------------------------------------------------|
   |   { accessToken, refreshToken, expiraEn }         |
```

**Hotspots:**
- 🔴 ¿Qué claims específicos lleva el JWT del cliente vs usuario interno?
- 🔴 ¿El cliente con estado `Verified` tiene acceso completo o restringido?

---

## Flujo 5: Login con OAuth (Google / Facebook)

```
[Cliente]                    [Proveedor OAuth]           [Sistema]
   |                               |                         |
   | Clic "Continuar con Google"   |                         |
   |------------------------------>|                         |
   |   idToken                     |                         |
   |<------------------------------|                         |
   |                               |                         |
   | POST /api/portal/login/google { idToken }               |
   |-------------------------------------------------------->|
   |                                                  Valida idToken con Google
   |                                                  Extrae email + nombre
   |                                                  Busca UsuarioCliente por email
   |                                                  Si no existe → Crea (estado: Active)
   |                                                  Si existe → Vincula proveedor
   |                                                  Genera JWT propio
   |<--------------------------------------------------------|
   |   { accessToken, refreshToken, expiraEn }               |

```

**Eventos de dominio:**
- 🟧 `ClienteRegistradoEvent` (si es nuevo)
- 🟧 `ClienteActivadoEvent` (si es nuevo)
- 🟧 `ClienteProveedorVinculadoEvent` (si ya existía)

---

## Flujo 6: Bloqueo por Admin

```
[Admin]                                            [Sistema]
   |                                                   |
   | POST /api/portal/clientes/{id}/bloquear           |
   |-------------------------------------------------->|
   |                                            Estado: Blocked
   |                                            Revoca todas las sesiones
   |                                            Agrega tokens a blacklist
   |<--------------------------------------------------|
   |                                    ClienteBloqueadoEvent
```

**Eventos de dominio:**
- 🟧 `ClienteBloqueadoEvent`

---

## Flujo 7: Habilitación de Registro por Tenant

```
[Admin]                                            [Sistema]
   |                                                   |
   | PUT /api/portal/configuracion/registro            |
   |    { habilitado: true }                           |
   |-------------------------------------------------->|
   |                                            RegistroHabilitado = true
   |                                            en ConfiguracionPortalTenant
   |<--------------------------------------------------|
```

---

## Flujo 8: Registro por Teléfono (WhatsApp / SMS)

```
[Cliente]                                          [Sistema]                    [INotificacionClienteService]
   |                                                   |                                    |
   | POST /api/portal/registro/telefono                |                                    |
   |    { telefono: "+521234567890", nombre, apellido } |                                    |
   |-------------------------------------------------->|                                    |
   |                                            Valida formato E.164                       |
   |                                            Valida teléfono único por tenant           |
   |                                            Crea UsuarioCliente (TipoRegistro=Telefono)|
   |                                            Estado: Registered                         |
   |                                            Genera OTP 6 dígitos (expira 10min)        |
   |                                            ----------------------------------------->|
   |                                            |                          Envía OTP por WhatsApp/SMS
   |<--------------------------------------------------|
   |   { mensaje: "Código enviado" }                   |
   |                                                   |
   | POST /api/portal/verificar-telefono               |
   |    { telefono, codigo }                           |
   |-------------------------------------------------->|
   |                                            Valida OTP
   |                                            Valida expiración
   |                                            TelefonoVerificado = true
   |                                            Estado: Active
   |                                            Genera JWT
   |<--------------------------------------------------|
   |   { accessToken, refreshToken }                   |
```

**Eventos de dominio:**
- 🟧 `ClienteRegistradoEvent`
- 🟧 `ClienteTelefonoVerificadoEvent`
- 🟧 `ClienteActivadoEvent`

---

## Bounded Context Canvas

### Nombre
Portal de Clientes

### Propósito
Gestionar el ciclo de vida de usuarios clientes externos que acceden al portal del sistema.

### Comandos entrantes
| Comando | Actor |
|---|---|
| `RegistrarClienteCommand` | Cliente externo |
| `RegistrarClientePorTelefonoCommand` | Cliente externo |
| `VerificarEmailClienteCommand` | Cliente externo |
| `VerificarTelefonoClienteCommand` | Cliente externo |
| `LoginClienteCommand` | Cliente externo |
| `LoginClienteGoogleCommand` | Cliente externo |
| `LoginClienteFacebookCommand` | Cliente externo |
| `BloquearClienteCommand` | Admin |
| `ReactivarClienteCommand` | Admin |
| `HabilitarRegistroCommand` | Admin |

### Eventos producidos
| Evento | Consumidores potenciales |
|---|---|
| `ClienteRegistradoEvent` | Servicio de email (notificación) |
| `ClienteActivadoEvent` | Módulos de negocio del sistema |
| `ClienteBloqueadoEvent` | Auditoría |

### Dependencias
| Dependencia | Motivo |
|---|---|
| `ITokenBlacklistService` | Invalidar tokens al bloquear o cerrar sesión |
| `IPasswordService` | Hash y validación de contraseñas |
| `IJwtService` | Generación de JWT para clientes |
| `IProveedorOAuthService` | Validación de tokens externos (Google, Facebook) |
| `INotificacionClienteService` | Envío de OTP por WhatsApp/SMS (registro por teléfono) |
| `ConfiguracionTenant` | Verificar si el registro está habilitado |

### Lo que NO pertenece a este contexto
- Lógica de negocio del portal (pedidos, historial, etc.)
- Vinculación con entidades de negocio (`Associated`) — responsabilidad del sistema que usa la plantilla
- Configuración de apps OAuth en proveedores externos

---

*Ver `docs/PLAN-PORTAL-CLIENTES.md` para el plan completo.*
*Ver `docs/Auth/02-Analisis-Tradicional/14-REQUERIMIENTOS-PORTAL-CLIENTES.md` para los RF.*
*Ver `docs/Auth/02-Analisis-Tradicional/15-MODELO-DOMINIO-PORTAL-CLIENTES.md` para el modelo de dominio.*
