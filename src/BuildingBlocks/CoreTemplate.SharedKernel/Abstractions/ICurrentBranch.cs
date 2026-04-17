namespace CoreTemplate.SharedKernel.Abstractions;

/// <summary>
/// Contrato para obtener la sucursal activa del usuario autenticado.
/// Solo relevante cuando <c>OrganizationSettings:EnableBranches = true</c>.
/// </summary>
public interface ICurrentBranch
{
    /// <summary>ID de la sucursal activa. Null si sucursales no está habilitado o no hay claim.</summary>
    Guid? BranchId { get; }
}
