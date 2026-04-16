namespace CoreTemplate.Modules.Auth.Application.Constants;

/// <summary>
/// Mensajes de error del módulo Auth.
/// </summary>
internal static class AuthErrorMessages
{
    // ─── Usuario ──────────────────────────────────────────────────────────────
    public const string UsuarioNoEncontrado = "El usuario no fue encontrado.";
    public const string EmailYaRegistrado = "El email ya está registrado.";
    public const string CredencialesInvalidas = "Las credenciales son inválidas.";
    public const string CuentaInactiva = "La cuenta está inactiva. Contacte al administrador.";
    public const string CuentaBloqueada = "La cuenta está bloqueada temporalmente por intentos fallidos.";
    public const string PasswordActualIncorrecto = "La contraseña actual es incorrecta.";
    public const string PasswordsNoCoinciden = "La nueva contraseña y la confirmación no coinciden.";
    public const string UsuarioYaActivo = "El usuario ya está activo.";
    public const string UsuarioYaInactivo = "El usuario ya está inactivo.";
    public const string UsuarioNoBloqueado = "El usuario no está bloqueado.";
    public const string SuperAdminNoPuedeDesactivarse = "El usuario SuperAdmin no puede ser desactivado.";

    // ─── Tokens ───────────────────────────────────────────────────────────────
    public const string RefreshTokenInvalido = "El refresh token es inválido o ha expirado.";
    public const string TokenRestablecimientoInvalido = "El token de restablecimiento es inválido o ha expirado.";

    // ─── Sesiones ─────────────────────────────────────────────────────────────
    public const string SesionNoEncontrada = "La sesión no fue encontrada.";
    public const string LimiteSesionesAlcanzado = "Se alcanzó el límite de sesiones simultáneas permitidas.";

    // ─── 2FA ──────────────────────────────────────────────────────────────────
    public const string DosFactoresNoActivo = "El 2FA no está activo para este usuario.";
    public const string DosFactoresYaActivo = "El 2FA ya está activo para este usuario.";
    public const string CodigoTotpInvalido = "El código de verificación es inválido.";
    public const string CodigoRecuperacionInvalido = "El código de recuperación es inválido o ya fue usado.";
    public const string DosFactoresNoHabilitado = "El 2FA no está habilitado en este sistema.";

    // ─── Roles ────────────────────────────────────────────────────────────────
    public const string RolNoEncontrado = "El rol no fue encontrado.";
    public const string RolNombreYaExiste = "Ya existe un rol con ese nombre.";
    public const string RolEsSistema = "Los roles del sistema no pueden ser eliminados.";
    public const string RolTieneUsuarios = "No se puede eliminar un rol que tiene usuarios asignados.";
    public const string UsuarioYaTieneRol = "El usuario ya tiene asignado este rol.";
    public const string UsuarioNoTieneRol = "El usuario no tiene asignado este rol.";
    public const string UsuarioDebeUnaRol = "El usuario debe tener al menos un rol.";

    // ─── Permisos ─────────────────────────────────────────────────────────────
    public const string PermisoNoEncontrado = "El permiso no fue encontrado.";
}
