namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de un usuario cliente del portal.
/// <para>
/// Transiciones válidas:
/// <code>
/// Registered → Verified  (al verificar email, si RequirePhoneVerification = false)
/// Registered → Active    (si RequireEmailVerification = false)
/// Verified   → Active    (al verificar teléfono, si RequirePhoneVerification = true)
/// Verified   → Blocked   (admin bloquea)
/// Active     → Blocked   (admin bloquea)
/// Blocked    → Active    (admin reactiva con email y teléfono verificados)
/// Blocked    → Verified  (admin reactiva con solo email verificado)
/// </code>
/// </para>
/// </summary>
public enum EstadoUsuarioCliente
{
    /// <summary>Registrado — email sin verificar. No puede hacer login.</summary>
    Registered = 1,

    /// <summary>Email verificado. Puede hacer login con acceso básico.</summary>
    Verified = 2,
    /// <summary>Asociado a cliente ERP</summary>
    //Associated = 3,

    /// <summary>Activo — todas las verificaciones completadas. Acceso completo al portal.</summary>
    Active = 3,

    /// <summary>Bloqueado por admin. No puede hacer login.</summary>
    Blocked = 4
}
