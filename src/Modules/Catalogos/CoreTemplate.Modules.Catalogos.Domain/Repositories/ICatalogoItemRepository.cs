using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Catalogos.Domain.Repositories;

/// <summary>
/// Contrato del repositorio de ítems de catálogo.
/// </summary>
public interface ICatalogoItemRepository
{
    /// <summary>Obtiene un ítem por su ID.</summary>
    Task<CatalogoItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Verifica si existe un ítem con el código indicado.</summary>
    Task<bool> ExistsByCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>Obtiene una página de ítems con filtros opcionales.</summary>
    Task<PagedResult<CatalogoItem>> GetPagedAsync(
        int pagina,
        int tamanoPagina,
        bool? soloActivos = null,
        string? busqueda = null,
        CancellationToken ct = default);

    /// <summary>Agrega un nuevo ítem y persiste los cambios.</summary>
    Task AddAsync(CatalogoItem item, CancellationToken ct = default);

    /// <summary>Actualiza un ítem existente y persiste los cambios.</summary>
    Task UpdateAsync(CatalogoItem item, CancellationToken ct = default);
}
