namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Contrato para obtener la sucursal activa del usuario autenticado.
/// Solo relevante cuando OrganizationSettings:EnableBranches = true.
/// </summary>
public interface ICurrentBranch
{
    /// <summary>ID de la sucursal activa. Null si sucursales no está habilitado o no hay claim.</summary>
    Guid? BranchId { get; }
}
