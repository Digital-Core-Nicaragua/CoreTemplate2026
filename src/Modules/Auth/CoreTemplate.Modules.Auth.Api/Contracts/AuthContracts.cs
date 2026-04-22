using CoreTemplate.Modules.Auth.Domain.Enums;

namespace CoreTemplate.Modules.Auth.Api.Contracts;

/// <summary>Request para login con email y contraseña.</summary>
public sealed record LoginRequest(
    string Email,
    string Password,
    CanalAcceso Canal = CanalAcceso.Web,
    string Dispositivo = "");

/// <summary>Request para registrar un nuevo usuario.</summary>
public sealed record RegistrarUsuarioRequest(
    string Email,
    string Nombre,
    string Password,
    string ConfirmPassword,
    TipoUsuario TipoUsuario = TipoUsuario.Humano);

/// <summary>Request para renovar el AccessToken usando un RefreshToken.</summary>
public sealed record RefreshTokenRequest(
    string RefreshToken);

/// <summary>Request para cerrar sesión.</summary>
public sealed record LogoutRequest(
    string RefreshToken);

/// <summary>Request para cambiar la contraseña del usuario autenticado.</summary>
public sealed record CambiarPasswordRequest(
    string PasswordActual,
    string NuevoPassword,
    string ConfirmPassword);

/// <summary>Request para solicitar el restablecimiento de contraseña por email.</summary>
public sealed record SolicitarRestablecimientoRequest(
    string Email);

/// <summary>Request para restablecer la contraseña con el token recibido por email.</summary>
public sealed record RestablecerPasswordRequest(
    string Token,
    string NuevoPassword,
    string ConfirmPassword);

/// <summary>Request para verificar el código TOTP en el flujo de login con 2FA.</summary>
public sealed record Verificar2FARequest(
    string TokenTemporal,
    string Codigo);

/// <summary>Request para confirmar la activación del 2FA con el primer código TOTP.</summary>
public sealed record Confirmar2FARequest(
    string Codigo);

/// <summary>Request para desactivar el 2FA con el código TOTP actual.</summary>
public sealed record Desactivar2FARequest(
    string Codigo);

/// <summary>Request para crear un nuevo rol.</summary>
public sealed record CrearRolRequest(
    string Nombre,
    string Descripcion,
    IReadOnlyList<Guid> PermisoIds);

/// <summary>Request para actualizar un rol existente.</summary>
public sealed record ActualizarRolRequest(
    string Nombre,
    string Descripcion,
    IReadOnlyList<Guid> PermisoIds);

/// <summary>Request para asignar un rol a un usuario.</summary>
public sealed record AsignarRolRequest(
    Guid RolId);

/// <summary>Request para crear una sucursal.</summary>
public sealed record CrearSucursalRequest(
    string Codigo,
    string Nombre);

/// <summary>Request para asignar una sucursal a un usuario.</summary>
public sealed record AsignarSucursalRequest(
    Guid SucursalId);

/// <summary>Request para cambiar la sucursal activa del usuario autenticado.</summary>
public sealed record CambiarSucursalActivaRequest(
    Guid SucursalId);

/// <summary>Request para crear una acción en el catálogo.</summary>
public sealed record CrearAccionRequest(
    string Codigo,
    string Nombre,
    string Modulo,
    string? Descripcion = null);

/// <summary>Request para configurar el límite de sesiones de un tenant.</summary>
public sealed record ConfigurarLimiteSesionesRequest(
    int? MaxSesionesSimultaneas);

// ─── Portal de Clientes ───────────────────────────────────────────────────────

/// <summary>Request para registrar un nuevo cliente en el portal.</summary>
public sealed record RegistrarClienteRequest(
    string Email,
    string Password,
    string Nombre,
    string Apellido,
    string? Telefono = null);

/// <summary>Request para login de cliente con email y contraseña.</summary>
public sealed record LoginClienteRequest(
    string Email,
    string Password);

/// <summary>Request para login de cliente mediante token OAuth externo.</summary>
public sealed record LoginClienteOAuthRequest(
    Domain.Enums.TipoProveedorOAuth Proveedor,
    string Token);

/// <summary>Request para verificar el email del cliente.</summary>
public sealed record VerificarEmailClienteRequest(
    string Token);

/// <summary>Request para verificar el teléfono del cliente.</summary>
public sealed record VerificarTelefonoClienteRequest(
    string Codigo);

/// <summary>Request para cambiar la contraseña del cliente autenticado.</summary>
public sealed record CambiarPasswordClienteRequest(
    string PasswordActual,
    string NuevoPassword);

/// <summary>Request para renovar el AccessToken del cliente.</summary>
public sealed record RefreshTokenClienteRequest(
    string RefreshToken);

/// <summary>Request para cerrar la sesión del cliente.</summary>
public sealed record LogoutClienteRequest(
    string RefreshToken);

/// <summary>Request para solicitar restablecimiento de contraseña del cliente.</summary>
public sealed record SolicitarRestablecimientoClienteRequest(
    string Email);

/// <summary>Request para restablecer la contraseña del cliente con el token recibido por email.</summary>
public sealed record RestablecerPasswordClienteRequest(
    string Token,
    string NuevoPassword);

// --- Portal Clientes: Telefono ---

/// <summary>Request para registrar un cliente por numero de telefono.</summary>
public sealed record RegistrarClientePorTelefonoRequest(
    string Telefono,
    string Nombre,
    string Apellido);

/// <summary>Request para solicitar OTP de login por telefono.</summary>
public sealed record SolicitarOtpTelefonoRequest(
    string Telefono);

/// <summary>Request para verificar OTP recibido por WhatsApp/SMS.</summary>
public sealed record VerificarOtpTelefonoRequest(
    string Telefono,
    string Codigo);
