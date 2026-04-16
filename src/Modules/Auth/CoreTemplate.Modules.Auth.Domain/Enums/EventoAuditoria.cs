namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Tipos de eventos registrados en la auditoría de seguridad.
/// </summary>
public enum EventoAuditoria
{
    /// <summary>Login exitoso.</summary>
    Login = 1,

    /// <summary>Intento de login fallido (credenciales incorrectas).</summary>
    LoginFallido = 2,

    /// <summary>Cierre de sesión.</summary>
    Logout = 3,

    /// <summary>Cambio de contraseña exitoso.</summary>
    CambioPassword = 4,

    /// <summary>Solicitud de restablecimiento de contraseña.</summary>
    RestablecimientoSolicitado = 5,

    /// <summary>Restablecimiento de contraseña completado.</summary>
    RestablecimientoCompletado = 6,

    /// <summary>Cuenta bloqueada por exceder intentos fallidos.</summary>
    CuentaBloqueada = 7,

    /// <summary>Cuenta desbloqueada por administrador o por tiempo.</summary>
    CuentaDesbloqueada = 8,

    /// <summary>2FA activado por el usuario.</summary>
    DosFactoresActivado = 9,

    /// <summary>2FA desactivado por el usuario.</summary>
    DosFactoresDesactivado = 10,

    /// <summary>AccessToken renovado con RefreshToken.</summary>
    TokenRefrescado = 11,

    /// <summary>Verificación de 2FA exitosa.</summary>
    DosFactoresVerificado = 12,

    /// <summary>Verificación de 2FA fallida.</summary>
    DosFactoresFallido = 13
}
