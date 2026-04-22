namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Nivel de severidad de un evento de auditoría de seguridad.
/// </summary>
public enum SeveridadAuditoria
{
    /// <summary>Eventos informativos normales: Login, Logout, TokenRefrescado, UsuarioRegistrado.</summary>
    Info = 1,

    /// <summary>Eventos que requieren atención: RestablecimientoSolicitado, DosFactoresActivado, DosFactoresDesactivado.</summary>
    Media = 2,

    /// <summary>Eventos de alerta: LoginFallido, DosFactoresFallido.</summary>
    Alta = 3,

    /// <summary>Eventos críticos de seguridad: CuentaBloqueada.</summary>
    Critica = 4
}
