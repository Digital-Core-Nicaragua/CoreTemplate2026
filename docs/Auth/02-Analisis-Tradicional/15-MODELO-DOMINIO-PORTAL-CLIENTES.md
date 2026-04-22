# Modelo de Dominio — Portal de Clientes

> **Fecha:** Pendiente de implementación
> **Estado:** Diseño completo — pendiente de desarrollo

---

## Aggregate: UsuarioCliente

Aggregate root que representa a un cliente externo con acceso al portal.
**Tabla**: `Auth.UsuariosCliente`
**Separado de**: `Auth.Usuarios` — no comparten tabla ni aggregate.

### Propiedades

| Propiedad | Tipo | Descripción |
|---|---|---|
| `Id` | `Guid` | Identificador único |
| `TenantId` | `Guid?` | Tenant al que pertenece (multi-tenant) |
| `Email` | `Email?` (VO) | Email único por tenant — **nullable** cuando el registro es por teléfono |
| `Telefono` | `string?` | Teléfono en formato E.164 — **nullable** cuando el registro es por email |
| `TipoRegistro` | `TipoRegistro` | Cómo se registró: `Email`, `Telefono`, `OAuth` |
| `PasswordHash` | `string?` | Nullable — clientes OAuth o por teléfono no tienen contraseña local |
| `Nombre` | `string` | Nombre del cliente |
| `Apellido` | `string` | Apellido del cliente |
| `Estado` | `EstadoUsuarioCliente` | Ciclo de vida del cliente |
| `EmailVerificado` | `bool` | Si el email fue verificado |
| `TelefonoVerificado` | `bool` | Si el teléfono fue verificado |
| `TokenVerificacionEmail` | `string?` | Token de un solo uso, expira en 24h |
| `TokenVerificacionTelefono` | `string?` | Código 6 dígitos, expira en 10min |
| `TokenExpiraEn` | `DateTime?` | Expiración del token activo |
| `Proveedores` | `List<ProveedorOAuth>` | Proveedores OAuth vinculados |
| `IntentosLoginFallidos` | `int` | Contador de intentos fallidos |
| `BloqueadoHasta` | `DateTime?` | Fecha de desbloqueo automático |
| `CreadoEn` | `DateTime` | Fecha de registro |
| `ActualizadoEn` | `DateTime?` | Última modificación |

### Métodos del Aggregate

| Método | Descripción |
|---|---|
| `Crear(email, passwordHash, nombre, apellido, tenantId)` | Factory method — estado inicial `Registered` |
| `CrearPorTelefono(telefono, nombre, apellido, tenantId)` | Factory method por teléfono — sin email, sin contraseña |
| `CrearDesdeOAuth(email, nombre, apellido, proveedor, externalId, tenantId)` | Factory method OAuth — estado inicial `Active` |
| `GenerarTokenVerificacionEmail()` | Genera token UUID, expira en 24h |
| `VerificarEmail(token)` | Valida token, marca email verificado, avanza estado |
| `GenerarCodigoVerificacionTelefono()` | Genera código 6 dígitos, expira en 10min |
| `VerificarTelefono(codigo)` | Valida código, marca teléfono verificado, avanza estado |
| `VincularProveedor(proveedor, externalId)` | Agrega proveedor OAuth al cliente |
| `DesvinularProveedor(proveedor)` | Quita proveedor OAuth |
| `CambiarPassword(nuevoHash)` | Actualiza hash de contraseña |
| `RegistrarLoginFallido()` | Incrementa contador, bloquea si supera límite |
| `RegistrarLoginExitoso()` | Resetea contador de intentos |
| `Bloquear()` | Estado → `Blocked` |
| `Reactivar()` | Estado → `Active` o `Verified` según verificaciones |

### Invariantes

- Email único por tenant (cuando presente)
- Teléfono único por tenant (cuando presente)
- Al menos email o teléfono debe estar presente
- No puede hacer login si estado es `Registered` o `Blocked`
- No puede verificar teléfono sin haber verificado email primero (si ambos requeridos)
- No puede desvincular el único proveedor si no tiene contraseña local
- `PasswordHash` nullable solo si tiene al menos un proveedor OAuth vinculado o se registró por teléfono

---

## Enum: EstadoUsuarioCliente

```csharp
public enum EstadoUsuarioCliente
{
    Registered = 1,  // Registrado, email sin verificar
    Verified   = 2,  // Email (y teléfono si aplica) verificado
    Active     = 3,  // Activo — acceso completo al portal
    Blocked    = 4   // Bloqueado por admin
}
```

### Transiciones válidas

```
Registered → Verified   (al verificar email, si RequirePhoneVerification = false)
Registered → Active     (si RequireEmailVerification = false)
Verified   → Active     (al verificar teléfono, si RequirePhoneVerification = true)
Verified   → Blocked    (admin bloquea)
Active     → Blocked    (admin bloquea)
Blocked    → Active     (admin reactiva, si tenía email y teléfono verificados)
Blocked    → Verified   (admin reactiva, si solo tenía email verificado)
```

### Acceso por estado

| Estado | Login | Portal | Gestión sesiones |
|---|---|---|---|
| `Registered` | ❌ | ❌ | ❌ |
| `Verified` | ✅ | ✅ Básico | Según config |
| `Active` | ✅ | ✅ Completo | Según config |
| `Blocked` | ❌ | ❌ | ❌ |

---

## Value Object: ProveedorOAuth

```csharp
public sealed class ProveedorOAuth
{
    public TipoProveedorOAuth Proveedor { get; }  // Google, Facebook
    public string ExternalId { get; }             // ID en el proveedor externo
    public string Email { get; }                  // Email retornado por el proveedor
    public DateTime VinculadoEn { get; }
}

public enum TipoProveedorOAuth
{
    Google   = 1,
    Facebook = 2
}
```

---

## Enum: TipoRegistro

```csharp
public enum TipoRegistro
{
    Email    = 1,  // Registro con email + contraseña
    Telefono = 2,  // Registro con teléfono + OTP (WhatsApp/SMS)
    OAuth    = 3   // Registro con proveedor externo (Google, Facebook)
}
```

---

## Repositorio: IUsuarioClienteRepository

Extiende la configuración por tenant para el portal de clientes.
Se agrega a `ConfiguracionTenant` existente o como entidad separada.

| Propiedad | Tipo | Descripción |
|---|---|---|
| `TenantId` | `Guid` | Tenant al que aplica |
| `RegistroHabilitado` | `bool` | Si los clientes pueden registrarse |
| `FechaHabilitacion` | `DateTime?` | Cuándo se habilitó el registro |
| `HabilitadoPor` | `Guid?` | Admin que lo habilitó |

---

## Repositorio: IUsuarioClienteRepository

```csharp
public interface IUsuarioClienteRepository
{
    Task<UsuarioCliente?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UsuarioCliente?> GetByEmailAsync(string email, Guid? tenantId, CancellationToken ct = default);
    Task<UsuarioCliente?> GetByTelefonoAsync(string telefono, Guid? tenantId, CancellationToken ct = default);
    Task<UsuarioCliente?> GetByExternalIdAsync(TipoProveedorOAuth proveedor, string externalId, Guid? tenantId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? tenantId, CancellationToken ct = default);
    Task<bool> ExistsByTelefonoAsync(string telefono, Guid? tenantId, CancellationToken ct = default);
    Task<PagedResult<UsuarioCliente>> GetPagedAsync(Guid? tenantId, EstadoUsuarioCliente? estado, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(UsuarioCliente cliente, CancellationToken ct = default);
    Task UpdateAsync(UsuarioCliente cliente, CancellationToken ct = default);
}
```

---

## Eventos de Dominio

| Evento | Cuándo se dispara |
|---|---|
| `ClienteRegistradoEvent` | Al crear un nuevo `UsuarioCliente` |
| `ClienteEmailVerificadoEvent` | Al verificar el email |
| `ClienteTelefonoVerificadoEvent` | Al verificar el teléfono |
| `ClienteActivadoEvent` | Al pasar a estado `Active` |
| `ClienteBloqueadoEvent` | Al bloquear un cliente |
| `ClienteReactivadoEvent` | Al reactivar un cliente bloqueado |
| `ClienteProveedorVinculadoEvent` | Al vincular proveedor OAuth |

---

*Ver `docs/PLAN-PORTAL-CLIENTES.md` para el plan completo.*
*Ver `docs/Auth/02-Analisis-Tradicional/14-REQUERIMIENTOS-PORTAL-CLIENTES.md` para los RF.*
