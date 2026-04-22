namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Indica cómo se registró un <see cref="CoreTemplate.Modules.Auth.Domain.Aggregates.UsuarioCliente"/>.
/// Determina qué identificador usa para autenticarse.
/// </summary>
public enum TipoRegistro
{
    /// <summary>Registro con email + contraseña.</summary>
    Email = 1,

    /// <summary>Registro con número de teléfono + OTP (WhatsApp o SMS).</summary>
    Telefono = 2,

    /// <summary>Registro con proveedor externo (Google, Facebook).</summary>
    OAuth = 3
}
