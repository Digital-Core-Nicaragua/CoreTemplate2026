using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;

namespace CoreTemplate.Modules.PdfTemplates.Domain.Repositories;

public interface IPdfPlantillaRepository
{
    Task<PdfPlantilla?> ObtenerPorCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);
    Task<PdfPlantilla?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PdfPlantilla>> ListarAsync(string? modulo = null, bool? soloActivos = null, CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, Guid? tenantId = null, CancellationToken ct = default);
    Task GuardarAsync(PdfPlantilla plantilla, CancellationToken ct = default);
    Task ActualizarAsync(PdfPlantilla plantilla, CancellationToken ct = default);
}
