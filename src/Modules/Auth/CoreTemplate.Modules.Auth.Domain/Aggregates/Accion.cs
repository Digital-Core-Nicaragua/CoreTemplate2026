using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate que representa una acción del catálogo de permisos gestionable.
/// Solo existe cuando AuthSettings:UseActionCatalog = true.
/// Extiende el modelo de permisos con gestión centralizada y habilitación por canal.
/// </summary>
public sealed class Accion : AggregateRoot<Guid>, IAuditable
{
    /// <summary>Código único en formato Modulo.Recurso.Accion.</summary>
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Nombre legible de la acción.</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Módulo al que pertenece.</summary>
    public string Modulo { get; private set; } = string.Empty;

    /// <summary>Descripción de lo que permite hacer.</summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>Indica si la acción está activa globalmente.</summary>
    public bool EsActiva { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    private Accion() { }

    public static Result<Accion> Crear(string codigo, string nombre, string modulo, string descripcion = "")
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Result<Accion>.Failure("El código de la acción es requerido.");
        }

        if (!codigo.Contains('.'))
        {
            return Result<Accion>.Failure("El código debe tener formato 'Modulo.Recurso.Accion'.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result<Accion>.Failure("El nombre de la acción es requerido.");
        }

        if (string.IsNullOrWhiteSpace(modulo))
        {
            return Result<Accion>.Failure("El módulo es requerido.");
        }

        return Result<Accion>.Success(new Accion
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            Modulo = modulo.Trim(),
            Descripcion = descripcion.Trim(),
            EsActiva = true,
            CreadoEn = DateTime.UtcNow
        });
    }

    public Result Activar()
    {
        if (EsActiva)
        {
            return Result.Failure("La acción ya está activa.");
        }

        EsActiva = true;
        return Result.Success();
    }

    public Result Desactivar()
    {
        if (!EsActiva)
        {
            return Result.Failure("La acción ya está inactiva.");
        }

        EsActiva = false;
        return Result.Success();
    }
}
