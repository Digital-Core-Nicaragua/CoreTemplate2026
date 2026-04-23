using CoreTemplate.Modules.Archivos.Domain.Aggregates;
using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Archivos.Domain.Repositories;

public interface IArchivoAdjuntoRepository
{
    Task<ArchivoAdjunto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ArchivoAdjunto>> ListarAsync(
        string? moduloOrigen, Guid? entidadId, int pagina, int tamano, CancellationToken ct = default);
    Task GuardarAsync(ArchivoAdjunto archivo, CancellationToken ct = default);
    Task ActualizarAsync(ArchivoAdjunto archivo, CancellationToken ct = default);
}
