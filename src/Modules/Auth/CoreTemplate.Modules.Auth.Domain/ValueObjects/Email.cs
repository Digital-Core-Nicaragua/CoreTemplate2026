using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una dirección de email válida.
/// <para>
/// Garantiza que el email tenga formato válido y lo normaliza
/// a minúsculas para comparaciones consistentes.
/// </para>
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>Valor del email normalizado a minúsculas.</summary>
    public string Valor { get; }

    private Email(string valor) => Valor = valor;

    /// <summary>
    /// Crea un <see cref="Email"/> validando el formato.
    /// </summary>
    /// <param name="valor">Dirección de email a validar.</param>
    public static Result<Email> Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return Result<Email>.Failure("El email es requerido.");
        }

        valor = valor.Trim().ToLowerInvariant();

        if (valor.Length > 200)
        {
            return Result<Email>.Failure("El email no puede superar los 200 caracteres.");
        }

        // Validación básica: debe contener @ y al menos un punto después del @
        var atIndex = valor.IndexOf('@');
        if (atIndex <= 0 || atIndex == valor.Length - 1)
        {
            return Result<Email>.Failure("El formato del email no es válido.");
        }

        var dominio = valor[(atIndex + 1)..];
        if (!dominio.Contains('.') || dominio.StartsWith('.') || dominio.EndsWith('.'))
        {
            return Result<Email>.Failure("El formato del email no es válido.");
        }

        return Result<Email>.Success(new Email(valor));
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }

    /// <inheritdoc/>
    public override string ToString() => Valor;
}
