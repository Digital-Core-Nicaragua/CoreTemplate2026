# Glosario — Módulo Auth

## Ubiquitous Language

Este glosario define el lenguaje ubicuo del Módulo Auth según Domain-Driven Design.

> **Fecha:** 2026-04-15

---

## A

### Access Token
Token JWT de corta duración (default: 15 minutos) que permite acceder a recursos protegidos. Contiene claims del usuario: `sub`, `email`, `name`, `roles`, `tenantId`, `tipo_usuario`, `branch_id` (si aplica).

### Accion
Aggregate que representa una operación específica del sistema en formato `Modulo.Recurso.Accion`. Solo existe cuando `UseActionCatalog = true`. Ejemplo: `Usuarios.Roles.Crear`.

### AccionAlLlegarLimiteSesiones
Enum que define qué hacer cuando un usuario alcanza el límite de sesiones: `CerrarMasAntigua` o `BloquearNuevoLogin`.

### Aggregate
Conjunto de objetos de dominio tratados como una unidad de consistencia. Ejemplos: `Usuario`, `Sesion`, `Rol`.

### AsignacionRol
Aggregate que representa la asignación de un rol a un usuario en una sucursal específica. Solo existe cuando `EnableBranches = true`. Invariante: combinación `UsuarioId + SucursalId + RolId` única.

### Auditoría
Registro inmutable de eventos de seguridad para trazabilidad. Los registros nunca se modifican ni eliminan.

### Autenticación
Proceso de verificar la identidad de un usuario mediante credenciales (email + contraseña).

### Autorización
Proceso de verificar si un usuario tiene permiso para ejecutar una acción. Se implementa con `[RequirePermission("Modulo.Recurso.Accion")]`.

---

## B

### Blacklist (Token Blacklist)
Lista de JTIs (JWT IDs) de tokens revocados que ya no son válidos aunque no hayan expirado. Backend configurable: `InMemory` o `Redis`.

### Bloqueo Temporal
Estado en el que un usuario no puede autenticarse por un período configurable (default: 15 minutos), generalmente por intentos fallidos. Solo aplica a `TipoUsuario.Humano`.

### branch_id
Claim en el JWT que indica la sucursal activa del usuario. Solo presente cuando `EnableBranches = true`.

---

## C

### CanalAcceso
Enum que indica el medio de acceso: `Web`, `Mobile`, `Api`, `Desktop`. Cada sesión registra el canal de origen.

### Claims
Información contenida en un token JWT: `sub` (userId), `email`, `name`, `roles`, `tenantId`, `tipo_usuario`, `branch_id`.

### ConfiguracionTenant
Entidad que almacena configuración específica por tenant, principalmente el límite de sesiones simultáneas. Solo relevante cuando `EnableSessionLimitsPerTenant = true`.

---

## E

### EnableBranches
Flag en `OrganizationSettings` que habilita el soporte de sucursales por usuario. Cuando `false`, los roles son globales.

### EnableTokenBlacklist
Flag en `AuthSettings` que habilita la invalidación inmediata de tokens al hacer logout o cambiar contraseña.

### EstadoUsuario
Enum del ciclo de vida del usuario: `Pendiente`, `Activo`, `Inactivo`, `Bloqueado`.

---

## I

### ICurrentBranch
Servicio de infraestructura que extrae el `branch_id` del JWT del request actual.

### ICurrentTenant
Servicio de infraestructura que extrae el `TenantId` del request actual (header o claim).

### ICurrentUser
Servicio de infraestructura que extrae el usuario autenticado del JWT del request actual.

### IntentosFallidos
Contador de intentos de login incorrectos consecutivos. Se resetea con login exitoso o desbloqueo.

### INotificacionClienteService
Contrato que define el envío de OTP por WhatsApp o SMS para el registro por teléfono. CoreTemplate define la interfaz; cada sistema implementa el proveedor (Twilio, AWS SNS, etc.).

### ISesionService
Domain service que implementa la lógica de límites de sesiones simultáneas con jerarquía Tenant → Global → Default.

### ITokenBlacklistService
Domain service para invalidar tokens antes de su expiración natural. Implementaciones: `InMemoryTokenBlacklistService`, `RedisTokenBlacklistService`.

---

## J

### JTI (JWT ID)
Identificador único de un token JWT. Se usa como clave en la blacklist.

### JWT (JSON Web Token)
Estándar para tokens de acceso. Contiene claims firmados con HMAC SHA256.

---

## L

### Límite de Sesiones
Número máximo de sesiones simultáneas por usuario. Jerarquía: Tenant > Global (`MaxSesionesSimultaneas`) > Default (5).

---

## M

### MaxSesionesSimultaneas
Configuración en `AuthSettings` que define el límite global de sesiones simultáneas por usuario.

### Multi-tenant
Modo en el que el sistema filtra automáticamente todos los datos por `TenantId`. Configurable con `IsMultiTenant`.

---

## P

### PasswordHash
Value Object que encapsula el hash BCrypt de una contraseña. Nunca se almacena ni transmite en texto plano.

### Permiso
String en formato `Modulo.Recurso.Accion` que representa una autorización. Se asigna a roles.

### Permisos Efectivos
Conjunto de permisos que un usuario puede ejercer en el contexto actual (sucursal activa + roles asignados).

---

## R

### Refresh Token
Token de larga duración (default: 7 días) usado para obtener nuevos access tokens. Se almacena como hash SHA256. Se rota en cada uso.

### RegistroAuditoria
Entidad inmutable que documenta un evento de seguridad (login, logout, cambio contraseña, etc.).

### Rol
Aggregate que agrupa permisos y se asigna a usuarios. Los roles de sistema (`EsSistema = true`) no pueden eliminarse.

### Rotación de Refresh Token
El refresh token anterior se invalida al emitir uno nuevo. Previene reutilización de tokens robados.

---

## S

### Sesion
Aggregate que representa un período de acceso autenticado. Incluye: RefreshTokenHash, Canal, Dispositivo, IP, UserAgent, UltimaActividad, ExpiraEn.

### Sucursal
Aggregate que representa una unidad organizacional. Solo existe cuando `EnableBranches = true`.

### Sucursal Activa
Sucursal en la que el usuario está trabajando actualmente. Se refleja en el claim `branch_id` del JWT.

### Sucursal Principal
Sucursal por defecto de un usuario. Se usa al generar el JWT inicial.

---

## T

### TipoRegistro
Enum del portal de clientes que indica cómo se registró un `UsuarioCliente`: `Email` (email + contraseña), `Telefono` (WhatsApp/SMS + OTP), `OAuth` (proveedor externo).

### TipoUsuario
Enum que clasifica el tipo de usuario: `Humano` (persona real), `Sistema` (servicio interno), `Integracion` (API externa). Determina comportamiento diferenciado en autenticación.

### Token Temporal 2FA
Token JWT de corta duración (5 minutos) emitido cuando el usuario tiene 2FA activo. Se usa para verificar el código TOTP antes de emitir los tokens definitivos.

### TOTP (Time-based One-Time Password)
Algoritmo para generar códigos de 6 dígitos que cambian cada 30 segundos. Compatible con Google Authenticator y Authy.

---

## U

### Ubiquitous Language
Lenguaje común compartido entre desarrolladores y expertos del dominio. Todos los términos de este glosario deben usarse consistentemente en código, documentación y conversaciones.

### UseActionCatalog
Flag en `AuthSettings` que habilita el catálogo de acciones como aggregates gestionables en lugar de strings estáticos.

### UsuarioSucursal
Entidad hija de `Usuario` que representa la relación entre un usuario y una sucursal, con flag `EsPrincipal`.

---

## V

### Value Object
Objeto inmutable definido por sus atributos, sin identidad propia. En Auth: `Email`, `PasswordHash`.

---

## Convenciones de Nomenclatura

### Eventos de Dominio
- Tiempo pasado: `UsuarioRegistrado`, `SesionRevocada`, `RolAsignado`
- Prefijo del aggregate: `Usuario*`, `Sesion*`, `Rol*`

### Comandos (Application Layer)
- Verbo imperativo + sufijo `Command`: `LoginCommand`, `LogoutCommand`, `AsignarRolCommand`

### Queries (Application Layer)
- Prefijo `Get` + sufijo `Query`: `GetMiPerfilQuery`, `GetSesionesUsuarioQuery`

### Handlers
- Sufijo `Handler`: `LoginCommandHandler`, `GetMiPerfilQueryHandler`

### Reglas de Negocio
- Código: `RN-AUTH-XXX`

### Casos de Uso
- Código: `CU-AUTH-XXX`

### Requerimientos
- Funcionales: `RF-AUTH-XXX`
- No Funcionales: `RNF-AUTH-XXX`

---

## Acrónimos

| Acrónimo | Significado |
|---|---|
| 2FA | Two-Factor Authentication |
| BCrypt | Algoritmo de hash de contraseñas |
| CQRS | Command Query Responsibility Segregation |
| DDD | Domain-Driven Design |
| DI | Dependency Injection |
| EF | Entity Framework |
| JWT | JSON Web Token |
| JTI | JWT ID (identificador único del token) |
| MediatR | Librería de mediador para CQRS |
| TOTP | Time-based One-Time Password |
| TTL | Time To Live |

---

**Fecha:** 2026-04-15
