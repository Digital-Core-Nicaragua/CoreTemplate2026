using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Auth.Domain.Events;

// ─── Usuario ─────────────────────────────────────────────────────────────────

/// <summary>Se dispara cuando un nuevo usuario se registra en el sistema.</summary>
public record UsuarioRegistradoEvent(
    Guid UsuarioId,
    string Email,
    string Nombre,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un usuario es activado.</summary>
public record UsuarioActivadoEvent(
    Guid UsuarioId,
    string Email) : IDomainEvent;

/// <summary>Se dispara cuando un usuario es desactivado.</summary>
public record UsuarioDesactivadoEvent(
    Guid UsuarioId,
    string Email) : IDomainEvent;

/// <summary>Se dispara cuando una cuenta es bloqueada por intentos fallidos.</summary>
public record UsuarioBloqueadoEvent(
    Guid UsuarioId,
    string Email,
    DateTime BloqueadoHasta) : IDomainEvent;

/// <summary>Se dispara cuando una cuenta es desbloqueada.</summary>
public record UsuarioDesbloqueadoEvent(
    Guid UsuarioId,
    string Email) : IDomainEvent;

/// <summary>Se dispara cuando un usuario cambia su contraseña.</summary>
public record PasswordCambiadoEvent(
    Guid UsuarioId,
    string Email) : IDomainEvent;

/// <summary>Se dispara cuando se solicita restablecer la contraseña.</summary>
public record RestablecimientoSolicitadoEvent(
    Guid UsuarioId,
    string Email,
    string Token,
    DateTime ExpiraEn) : IDomainEvent;

/// <summary>Se dispara cuando el 2FA es activado por el usuario.</summary>
public record DosFactoresActivadoEvent(
    Guid UsuarioId,
    string Email) : IDomainEvent;

/// <summary>Se dispara cuando el 2FA es desactivado por el usuario.</summary>
public record DosFactoresDesactivadoEvent(
    Guid UsuarioId,
    string Email) : IDomainEvent;

// ─── Rol ─────────────────────────────────────────────────────────────────────

/// <summary>Se dispara cuando se crea un nuevo rol.</summary>
public record RolCreadoEvent(
    Guid RolId,
    string Nombre,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando se actualiza un rol.</summary>
public record RolActualizadoEvent(
    Guid RolId,
    string Nombre) : IDomainEvent;

/// <summary>Se dispara cuando se agrega un permiso a un rol.</summary>
public record PermisoAgregadoARolEvent(
    Guid RolId,
    Guid PermisoId) : IDomainEvent;

/// <summary>Se dispara cuando se quita un permiso de un rol.</summary>
public record PermisoQuitadoDeRolEvent(
    Guid RolId,
    Guid PermisoId) : IDomainEvent;

// ─── Sesiones ───────────────────────────────────────────────────────────────────

/// <summary>Se dispara cuando una sesión es revocada (logout o cierre remoto).</summary>
public record SesionRevocadaEvent(
    Guid SesionId,
    Guid UsuarioId) : IDomainEvent;

/// <summary>Se dispara cuando todas las sesiones de un usuario son revocadas.</summary>
public record TodasSesionesRevocadasEvent(
    Guid UsuarioId) : IDomainEvent;

// ─── Sucursales ───────────────────────────────────────────────────────────────────

/// <summary>Se dispara cuando se asigna una sucursal a un usuario.</summary>
public record SucursalAsignadaEvent(
    Guid UsuarioId,
    Guid SucursalId) : IDomainEvent;

/// <summary>Se dispara cuando se remueve una sucursal de un usuario.</summary>
public record SucursalRemovidaEvent(
    Guid UsuarioId,
    Guid SucursalId) : IDomainEvent;

// ─── Portal de Clientes ───────────────────────────────────────────────────────

/// <summary>Se dispara cuando un nuevo cliente se registra en el portal.</summary>
public record ClienteRegistradoEvent(
    Guid ClienteId,
    string Email,
    string Nombre,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un cliente verifica su email.</summary>
public record ClienteEmailVerificadoEvent(
    Guid ClienteId,
    string Email,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un cliente verifica su teléfono.</summary>
public record ClienteTelefonoVerificadoEvent(
    Guid ClienteId,
    string Email,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un cliente pasa a estado Active (acceso completo al portal).</summary>
public record ClienteActivadoEvent(
    Guid ClienteId,
    string Email,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un cliente es bloqueado (por admin o por intentos fallidos).</summary>
public record ClienteBloqueadoEvent(
    Guid ClienteId,
    string Email,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un admin reactiva un cliente bloqueado.</summary>
public record ClienteReactivadoEvent(
    Guid ClienteId,
    string Email,
    Guid? TenantId) : IDomainEvent;

/// <summary>Se dispara cuando un cliente vincula un proveedor OAuth adicional.</summary>
public record ClienteProveedorVinculadoEvent(
    Guid ClienteId,
    string Email,
    CoreTemplate.Modules.Auth.Domain.Enums.TipoProveedorOAuth Proveedor,
    Guid? TenantId) : IDomainEvent;
