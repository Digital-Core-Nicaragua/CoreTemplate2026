namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Contrato para obtener el tenant de la solicitud HTTP actual.
/// <para>
/// Cuando <c>IsMultiTenant = true</c> en configuración, este servicio
/// resuelve el TenantId desde el header HTTP, subdominio o claim JWT.
/// </para>
/// <para>
/// Cuando <c>IsMultiTenant = false</c>, siempre retorna null y
/// <see cref="EsMultiTenant"/> retorna false.
/// </para>
/// <para>
/// Uso típico en un repositorio:
/// <code>
/// internal sealed class UsuarioRepository(AuthDbContext _db, ICurrentTenant _tenant)
///     : IUsuarioRepository
/// {
///     public Task&lt;Usuario?&gt; GetByEmailAsync(string email, CancellationToken ct) =>
///         _db.Usuarios
///             .Where(u => u.Email.Valor == email)
///             // El QueryFilter global ya filtra por TenantId automáticamente
///             .FirstOrDefaultAsync(ct);
/// }
/// </code>
/// </para>
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// ID del tenant de la solicitud actual.
    /// Null si el sistema es single-tenant o si no se pudo resolver el tenant.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>Indica si el sistema está configurado como multi-tenant.</summary>
    bool EsMultiTenant { get; }

    /// <summary>Indica si el tenant fue resuelto correctamente en la solicitud actual.</summary>
    bool TenantResuelto { get; }
}
