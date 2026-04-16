namespace CoreTemplate.SharedKernel.Domain;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// <para>
/// Una entidad tiene identidad propia definida por su <see cref="Id"/>.
/// Dos entidades son iguales si tienen el mismo tipo e identificador,
/// independientemente del valor de sus propiedades.
/// </para>
/// <para>
/// Uso típico:
/// <code>
/// public class UsuarioRol : Entity&lt;Guid&gt;
/// {
///     public Guid UsuarioId { get; private set; }
///     public Guid RolId { get; private set; }
/// }
/// </code>
/// </para>
/// </summary>
/// <typeparam name="TId">Tipo del identificador único. Generalmente <c>Guid</c>.</typeparam>
public abstract class Entity<TId>
{
    /// <summary>
    /// Identificador único de la entidad.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Dos entidades son iguales si tienen el mismo tipo e identificador.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id!.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left is not null && right is not null && left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}
