using CoreTemplate.Modules.Archivos.Application.Commands;
using CoreTemplate.Modules.Archivos.Application.DTOs;
using CoreTemplate.Modules.Archivos.Domain.Repositories;
using CoreTemplate.SharedKernel;
using CoreTemplate.Storage.Abstractions;
using MediatR;

namespace CoreTemplate.Modules.Archivos.Application.Queries;

// ─── Obtener por ID ───────────────────────────────────────────────────────────

public record GetArchivoByIdQuery(Guid Id) : IRequest<Result<ArchivoAdjuntoDto>>;

internal sealed class GetArchivoByIdHandler(IArchivoAdjuntoRepository repo)
    : IRequestHandler<GetArchivoByIdQuery, Result<ArchivoAdjuntoDto>>
{
    public async Task<Result<ArchivoAdjuntoDto>> Handle(GetArchivoByIdQuery q, CancellationToken ct)
    {
        var archivo = await repo.ObtenerPorIdAsync(q.Id, ct);
        return archivo is null
            ? Result<ArchivoAdjuntoDto>.Failure("Archivo no encontrado.")
            : Result<ArchivoAdjuntoDto>.Success(archivo.ToDto());
    }
}

// ─── Obtener URL ──────────────────────────────────────────────────────────────

public record GetArchivoUrlQuery(Guid Id) : IRequest<Result<ArchivoUrlDto>>;

internal sealed class GetArchivoUrlHandler(
    IArchivoAdjuntoRepository repo,
    IStorageService storage) : IRequestHandler<GetArchivoUrlQuery, Result<ArchivoUrlDto>>
{
    public async Task<Result<ArchivoUrlDto>> Handle(GetArchivoUrlQuery q, CancellationToken ct)
    {
        var archivo = await repo.ObtenerPorIdAsync(q.Id, ct);
        if (archivo is null) return Result<ArchivoUrlDto>.Failure("Archivo no encontrado.");

        var url = await storage.ObtenerUrlAsync(archivo.RutaAlmacenada, ct);
        if (url is null) return Result<ArchivoUrlDto>.Failure("No se pudo generar la URL del archivo.");

        // Actualizar URL en BD si cambió (ej: URL firmada de S3 regenerada)
        archivo.ActualizarUrl(url);
        await repo.ActualizarAsync(archivo, ct);

        return Result<ArchivoUrlDto>.Success(new ArchivoUrlDto(archivo.Id, url));
    }
}

// ─── Listar por entidad ───────────────────────────────────────────────────────

public record GetArchivosPorEntidadQuery(
    string? ModuloOrigen,
    Guid? EntidadId,
    int Pagina = 1,
    int Tamano = 20) : IRequest<Result<PagedResult<ArchivoAdjuntoDto>>>;

internal sealed class GetArchivosPorEntidadHandler(IArchivoAdjuntoRepository repo)
    : IRequestHandler<GetArchivosPorEntidadQuery, Result<PagedResult<ArchivoAdjuntoDto>>>
{
    public async Task<Result<PagedResult<ArchivoAdjuntoDto>>> Handle(GetArchivosPorEntidadQuery q, CancellationToken ct)
    {
        var paged = await repo.ListarAsync(q.ModuloOrigen, q.EntidadId, q.Pagina, q.Tamano, ct);
        var dtos = new PagedResult<ArchivoAdjuntoDto>(
            paged.Items.Select(a => a.ToDto()).ToList(),
            paged.Pagina, paged.TamanoPagina, paged.Total);
        return Result<PagedResult<ArchivoAdjuntoDto>>.Success(dtos);
    }
}
