namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Tipo de usuario del sistema.
/// Determina el comportamiento diferenciado en autenticación y seguridad.
/// </summary>
public enum TipoUsuario
{
    /// <summary>Persona real. Aplican todas las reglas: 2FA, bloqueo, límite de sesiones.</summary>
    Humano = 1,

    /// <summary>Servicio interno o proceso automatizado. Sin 2FA, sin bloqueo, sin límite de sesiones.</summary>
    Sistema = 2,

    /// <summary>API externa o tercero. Sin 2FA, sin bloqueo, sin límite de sesiones.</summary>
    Integracion = 3
}
