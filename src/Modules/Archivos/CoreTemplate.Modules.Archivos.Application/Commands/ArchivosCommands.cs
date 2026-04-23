using CoreTemplate.Modules.Archivos.Application.DTOs;
using CoreTemplate.Modules.Archivos.Domain.Aggregates;
using CoreTemplate.Modules.Archivos.Domain.Repositories;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.Storage.Abstractions;
using MediatR;

namespace CoreTemplate.Modules.Archivos.Application.Commands;

// ─── Subir ────────────────────────────────────────────────────────────────────

public record SubirArchivoCommand(
    Stream Contenido,
    string NombreOriginal,
    string ContentType,
    string Contexto,
    string ModuloOrigen,
    Guid? EntidadId = null) : IRequest<Result<ArchivoAdjuntoDto>>;

internal sealed class SubirArchivoHandler(
    IStorageService storage,
    IArchivoAdjuntoRepository repo,
    ICurrentUser currentUser,
    ICurrentTenant currentTenant) : IRequestHandler<SubirArchivoCommand, Result<ArchivoAdjuntoDto>>
{
    public async Task<Result<ArchivoAdjuntoDto>> Handle(SubirArchivoCommand cmd, CancellationToken ct)
    {
        var storageResult = await storage.SubirAsync(new SubirArchivoRequest(
            cmd.Contenido, cmd.NombreOriginal, cmd.Contexto, cmd.ContentType), ct);

        if (!storageResult.Exitoso)
            return Result<ArchivoAdjuntoDto>.Failure(storageResult.Error!);

        var nombreAlmacenado = Path.GetFileName(storageResult.RutaAlmacenada!);

        var result = ArchivoAdjunto.Crear(
            cmd.NombreOriginal,
            nombreAlmacenado,
            storageResult.RutaAlmacenada!,
            storageResult.Url!,
            cmd.ContentType,
            storageResult.TamanioBytes,
            storageResult.Proveedor!,
            cmd.Contexto,
            cmd.ModuloOrigen,
            currentUser.Id ?? Guid.Empty,
            cmd.EntidadId,
            currentTenant.TenantId);

        if (!result.IsSuccess)
            return Result<ArchivoAdjuntoDto>.Failure(result.Error!);

        await repo.GuardarAsync(result.Value!, ct);
        return Result<ArchivoAdjuntoDto>.Success(result.Value!.ToDto(), "Archivo subido correctamente.");
    }
}

// ─── Eliminar ─────────────────────────────────────────────────────────────────

public record EliminarArchivoCommand(Guid Id) : IRequest<Result>;

internal sealed class EliminarArchivoHandler(
    IStorageService storage,
    IArchivoAdjuntoRepository repo) : IRequestHandler<EliminarArchivoCommand, Result>
{
    public async Task<Result> Handle(EliminarArchivoCommand cmd, CancellationToken ct)
    {
        var archivo = await repo.ObtenerPorIdAsync(cmd.Id, ct);
        if (archivo is null) return Result.Failure("Archivo no encontrado.");

        await storage.EliminarAsync(archivo.RutaAlmacenada, ct);

        var result = archivo.Eliminar();
        if (!result.IsSuccess) return result;

        await repo.ActualizarAsync(archivo, ct);
        return Result.Success();
    }
}

// ─── Mapeo ────────────────────────────────────────────────────────────────────

internal static class ArchivoExtensions
{
    public static ArchivoAdjuntoDto ToDto(this ArchivoAdjunto a) => new(
        a.Id, a.NombreOriginal, a.ContentType, a.TamanioBytes,
        a.Proveedor, a.Contexto, a.ModuloOrigen, a.EntidadId, a.FechaSubida);
}
