namespace CoreTemplate.Modules.Auth.Domain.Enums;

/// <summary>
/// Canal desde donde se origina el acceso al sistema.
/// Cada sesión registra el canal de origen.
/// </summary>
public enum CanalAcceso
{
    Web = 1,
    Mobile = 2,
    Api = 3,
    Desktop = 4
}
