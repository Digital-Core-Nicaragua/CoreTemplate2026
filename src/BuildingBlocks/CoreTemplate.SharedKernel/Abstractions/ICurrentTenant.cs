namespace CoreTemplate.SharedKernel.Abstractions;

/// <summary>
/// Contrato para obtener el tenant de la solicitud HTTP actual.
/// <para>
/// Cuando <c>IsMultiTenant = true</c>, resuelve el TenantId desde
/// el header HTTP, subdominio o claim JWT.
/// </para>
/// <para>
/// Cuando <c>IsMultiTenant = false</c>, siempre retorna null.
/// </para>
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// ID del tenant de la solicitud actual.
    /// Null si el sistema es single-tenant o no se pudo resolver.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>Indica si el sistema está configurado como multi-tenant.</summary>
    bool EsMultiTenant { get; }

    /// <summary>Indica si el tenant fue resuelto correctamente en la solicitud actual.</summary>
    bool TenantResuelto { get; }
}
