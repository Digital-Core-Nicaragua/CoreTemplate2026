namespace CoreTemplate.Infrastructure.Persistence;

/// <summary>
/// Interfaz que marca una entidad como perteneciente a un tenant.
/// <para>
/// Todas las entidades que implementen esta interfaz recibirán
/// automáticamente un <c>QueryFilter</c> global en el <c>BaseDbContext</c>
/// que filtra por el <see cref="TenantId"/> de la solicitud actual
/// cuando el sistema está en modo multi-tenant.
/// </para>
/// <para>
/// Uso en una entidad:
/// <code>
/// public class Usuario : AggregateRoot&lt;Guid&gt;, IHasTenant
/// {
///     public Guid? TenantId { get; private set; }
///     // ...
/// }
/// </code>
/// </para>
/// </summary>
public interface IHasTenant
{
    /// <summary>
    /// ID del tenant al que pertenece esta entidad.
    /// Null cuando el sistema opera en modo single-tenant.
    /// </summary>
    Guid? TenantId { get; }
}
