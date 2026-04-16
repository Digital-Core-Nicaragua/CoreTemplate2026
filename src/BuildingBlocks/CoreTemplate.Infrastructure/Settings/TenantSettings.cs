namespace CoreTemplate.Infrastructure.Settings;

/// <summary>
/// Configuración del comportamiento multi-tenant del sistema.
/// Se lee desde la sección <c>TenantSettings</c> en <c>appsettings.json</c>.
/// <para>
/// Ejemplo de configuración:
/// <code>
/// {
///   "TenantSettings": {
///     "IsMultiTenant": true,
///     "TenantResolutionStrategy": "Header"
///   }
/// }
/// </code>
/// </para>
/// </summary>
public sealed class TenantSettings
{
    /// <summary>
    /// Nombre de la sección en appsettings.json.
    /// </summary>
    public const string SectionName = "TenantSettings";

    /// <summary>
    /// Indica si el sistema opera en modo multi-tenant.
    /// <list type="bullet">
    ///   <item><c>true</c> — Se filtra por TenantId en todas las entidades.</item>
    ///   <item><c>false</c> — Sistema single-tenant, TenantId se ignora.</item>
    /// </list>
    /// </summary>
    public bool IsMultiTenant { get; init; } = false;

    /// <summary>
    /// Estrategia de resolución del tenant.
    /// Valores válidos: <c>Header</c>, <c>Claim</c>, <c>Subdomain</c>.
    /// Default: <c>Header</c>.
    /// </summary>
    public string TenantResolutionStrategy { get; init; } = "Header";

    /// <summary>
    /// Cuando es true, cada tenant puede tener su propio límite de sesiones simultáneas.
    /// Requiere IsMultiTenant = true para tener efecto.
    /// </summary>
    public bool EnableSessionLimitsPerTenant { get; init; } = false;
}
