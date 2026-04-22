namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Proveedor de autenticación OAuth externo.
/// <para>
/// Se usa para identificar con qué proveedor se registró o vinculó
/// un <see cref="CoreTemplate.Modules.Auth.Domain.Aggregates.UsuarioCliente"/>.
/// </para>
/// </summary>
public enum TipoProveedorOAuth
{
    /// <summary>Autenticación local con email y contraseña.</summary>
    Local = 1,

    /// <summary>Google OAuth 2.0 — valida idToken con la API de Google.</summary>
    Google = 2,

    /// <summary>Facebook OAuth — valida accessToken con la Graph API de Facebook.</summary>
    Facebook = 3
}
