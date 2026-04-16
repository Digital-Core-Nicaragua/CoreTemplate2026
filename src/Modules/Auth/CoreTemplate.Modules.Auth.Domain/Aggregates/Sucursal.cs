using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate que representa una sucursal o unidad organizacional.
/// Solo existe cuando OrganizationSettings:EnableBranches = true.
/// </summary>
public sealed class Sucursal : AggregateRoot<Guid>
{
    /// <summary>ID del tenant al que pertenece.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Código único de la sucursal.</summary>
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Nombre de la sucursal.</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Indica si la sucursal está activa.</summary>
    public bool EsActiva { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    private Sucursal() { }

    public static Result<Sucursal> Crear(string codigo, string nombre, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Result<Sucursal>.Failure("El código de la sucursal es requerido.");

        if (codigo.Trim().Length > 20)
            return Result<Sucursal>.Failure("El código no puede superar 20 caracteres.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<Sucursal>.Failure("El nombre de la sucursal es requerido.");

        if (nombre.Trim().Length > 100)
            return Result<Sucursal>.Failure("El nombre no puede superar 100 caracteres.");

        return Result<Sucursal>.Success(new Sucursal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Nombre = nombre.Trim(),
            EsActiva = true,
            CreadoEn = DateTime.UtcNow
        });
    }

    public Result Activar()
    {
        if (EsActiva) return Result.Failure("La sucursal ya está activa.");
        EsActiva = true;
        return Result.Success();
    }

    public Result Desactivar()
    {
        if (!EsActiva) return Result.Failure("La sucursal ya está inactiva.");
        EsActiva = false;
        return Result.Success();
    }
}
