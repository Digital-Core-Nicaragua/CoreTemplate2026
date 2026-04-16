using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula el hash de una contraseña.
/// <para>
/// El dominio nunca trabaja con contraseñas en texto plano.
/// La capa de Infrastructure genera el hash (BCrypt) y lo pasa
/// al dominio ya hasheado a través de este Value Object.
/// </para>
/// </summary>
public sealed class PasswordHash : ValueObject
{
    /// <summary>Hash de la contraseña (BCrypt).</summary>
    public string Valor { get; }

    private PasswordHash(string valor) => Valor = valor;

    /// <summary>
    /// Crea un <see cref="PasswordHash"/> a partir de un hash ya generado.
    /// </summary>
    /// <param name="hash">Hash BCrypt generado por la capa de Infrastructure.</param>
    public static Result<PasswordHash> Crear(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result<PasswordHash>.Failure("El hash de la contraseña es requerido.");
        }

        return Result<PasswordHash>.Success(new PasswordHash(hash));
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }
}
