namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Estados posibles de un usuario en el sistema.
/// </summary>
public enum EstadoUsuario
{
    /// <summary>Registrado pero pendiente de activación.</summary>
    Pendiente = 1,

    /// <summary>Activo — puede autenticarse y operar normalmente.</summary>
    Activo = 2,

    /// <summary>Desactivado manualmente por un administrador.</summary>
    Inactivo = 3,

    /// <summary>Bloqueado automáticamente por exceder el límite de intentos fallidos.</summary>
    Bloqueado = 4
}
