namespace CoreTemplate.SharedKernel.Domain;

/// <summary>
/// Clase base para todos los Value Objects del dominio.
/// <para>
/// Un Value Object no tiene identidad propia — su igualdad se define
/// por el valor de sus propiedades, no por un identificador.
/// Son inmutables por diseño.
/// </para>
/// <para>
/// Uso típico:
/// <code>
/// public class Email : ValueObject
/// {
///     public string Valor { get; private set; }
///
///     private Email(string valor) => Valor = valor;
///
///     public static Result&lt;Email&gt; Crear(string valor)
///     {
///         if (!valor.Contains('@'))
///             return Result&lt;Email&gt;.Failure("El email no es válido.");
///         return Result&lt;Email&gt;.Success(new Email(valor.ToLowerInvariant()));
///     }
///
///     protected override IEnumerable&lt;object&gt; GetEqualityComponents()
///     {
///         yield return Valor;
///     }
/// }
/// </code>
/// </para>
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Retorna los componentes que definen la igualdad del Value Object.
    /// Cada subclase debe implementar este método retornando todas sus propiedades.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left is not null && right is not null && left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);
}
