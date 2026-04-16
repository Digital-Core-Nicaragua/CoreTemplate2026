namespace CoreTemplate.SharedKernel.Domain;

/// <summary>
/// Clase base para todos los Aggregate Roots del dominio.
/// <para>
/// Un Aggregate Root es la entidad principal de un agregado DDD.
/// Es el único punto de entrada para modificar el estado del agregado
/// y el responsable de garantizar sus invariantes de negocio.
/// </para>
/// <para>
/// Extiende <see cref="Entity{TId}"/> y agrega la capacidad de registrar
/// eventos de dominio que serán publicados después de que la transacción
/// se complete exitosamente.
/// </para>
/// <para>
/// Uso típico:
/// <code>
/// public class Usuario : AggregateRoot&lt;Guid&gt;
/// {
///     public static Result&lt;Usuario&gt; Crear(string email, string nombre)
///     {
///         var usuario = new Usuario { Id = Guid.NewGuid(), Email = email };
///         usuario.RaiseDomainEvent(new UsuarioRegistradoEvent(usuario.Id, email));
///         return Result&lt;Usuario&gt;.Success(usuario, "Usuario creado correctamente.");
///     }
/// }
/// </code>
/// </para>
/// </summary>
/// <typeparam name="TId">Tipo del identificador único. Generalmente <c>Guid</c>.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Lista de eventos de dominio pendientes de publicar.
    /// Se publican después de que SaveChanges completa exitosamente.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registra un evento de dominio para ser publicado al finalizar la transacción.
    /// </summary>
    /// <param name="domainEvent">Evento que describe lo que ocurrió en el dominio.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Limpia la lista de eventos pendientes.
    /// Llamar después de publicar los eventos.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
