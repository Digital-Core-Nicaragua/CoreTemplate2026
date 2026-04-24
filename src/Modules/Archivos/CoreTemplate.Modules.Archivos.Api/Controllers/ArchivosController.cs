using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Archivos.Application.Commands;
using CoreTemplate.Modules.Archivos.Application.DTOs;
using CoreTemplate.Modules.Archivos.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Archivos.Api.Controllers;

/// <summary>Request model para subida de archivos via multipart/form-data.</summary>
public sealed class SubirArchivoFormRequest
{
    public IFormFile Archivo { get; set; } = null!;
    public string ModuloOrigen { get; set; } = string.Empty;
    public Guid? EntidadId { get; set; }
}

[Authorize]
[Route("api/archivos")]
public sealed class ArchivosController(ISender sender) : BaseApiController
{
    /// <summary>Sube un archivo al proveedor de almacenamiento configurado.</summary>
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Subir([FromForm] SubirArchivoFormRequest req, CancellationToken ct)
    {
        if (req.Archivo.Length == 0)
            return BadRequestResponse<object>("El archivo está vacío.");

        await using var stream = req.Archivo.OpenReadStream();

        var result = await sender.Send(new SubirArchivoCommand(
            stream, req.Archivo.FileName, req.Archivo.ContentType,
            req.ModuloOrigen, req.EntidadId), ct);

        return result.IsSuccess
            ? Created(string.Empty, ApiResponse<ArchivoAdjuntoDto>.FromResult(result))
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Obtiene los metadatos de un archivo por ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetArchivoByIdQuery(id), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : NotFoundResponse<object>(result.Error!);
    }

    /// <summary>Obtiene la URL de acceso (firmada si S3) para visualizar o descargar el archivo.</summary>
    [HttpGet("{id:guid}/url")]
    public async Task<IActionResult> ObtenerUrl(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetArchivoUrlQuery(id), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : NotFoundResponse<object>(result.Error!);
    }

    /// <summary>Lista archivos filtrados por módulo origen y/o entidad relacionada.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? moduloOrigen,
        [FromQuery] Guid? entidadId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetArchivosPorEntidadQuery(moduloOrigen, entidadId, pagina, tamano), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Elimina un archivo del almacenamiento y marca el registro como inactivo.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new EliminarArchivoCommand(id), ct);
        return result.IsSuccess
            ? SuccessResponse(true, "Archivo eliminado correctamente.")
            : BadRequestResponse<object>(result.Error!);
    }
}
