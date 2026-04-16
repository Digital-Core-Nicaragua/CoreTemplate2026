using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate que representa la asignación de un rol a un usuario en una sucursal específica.
/// Solo existe cuando OrganizationSettings:EnableBranches = true.
/// Invariante: no puede existir la misma combinación UsuarioId+SucursalId+RolId.
/// </summary>
public sealed class AsignacionRol : AggregateRoot<Guid>
{
    /// <summary>ID del usuario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>ID de la sucursal.</summary>
    public Guid SucursalId { get; private set; }

    /// <summary>ID del rol asignado.</summary>
    public Guid RolId { get; private set; }

    /// <summary>Fecha de asignación.</summary>
    public DateTime AsignadoEn { get; private set; }

    private AsignacionRol() { }

    public static Result<AsignacionRol> Crear(Guid usuarioId, Guid sucursalId, Guid rolId) =>
        Result<AsignacionRol>.Success(new AsignacionRol
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            SucursalId = sucursalId,
            RolId = rolId,
            AsignadoEn = DateTime.UtcNow
        });
}
