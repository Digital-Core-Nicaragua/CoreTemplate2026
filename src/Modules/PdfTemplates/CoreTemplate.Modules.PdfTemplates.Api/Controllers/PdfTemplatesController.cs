using CoreTemplate.Api.Common;
using CoreTemplate.Modules.PdfTemplates.Api.Contracts;
using CoreTemplate.Modules.PdfTemplates.Application.Commands;
using CoreTemplate.Modules.PdfTemplates.Application.DTOs;
using CoreTemplate.Modules.PdfTemplates.Application.Queries;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.PdfTemplates.Api.Controllers;

[Authorize]
[Route("api/pdf-templates")]
public sealed class PdfTemplatesController(ISender sender, ICurrentUser currentUser) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? modulo, [FromQuery] bool? soloActivos, CancellationToken ct)
    {
        var result = await sender.Send(new GetPdfPlantillasQuery(modulo, soloActivos), ct);
        return result.IsSuccess ? SuccessResponse(result.Value!, result.Message) : BadRequestResponse<object>(result.Error!);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPdfPlantillaByIdQuery(id), ct);
        return result.IsSuccess ? SuccessResponse(result.Value!, result.Message) : NotFoundResponse<object>(result.Error!);
    }

    [HttpGet("disenios")]
    public async Task<IActionResult> ObtenerDisenios(CancellationToken ct)
    {
        var result = await sender.Send(new GetDiseniosDisponiblesQuery(), ct);
        return result.IsSuccess ? SuccessResponse(result.Value!, result.Message) : BadRequestResponse<object>(result.Error!);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPdfPlantillaRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new CrearPdfPlantillaCommand(
            req.Codigo, req.Nombre, req.Modulo, req.CodigoTemplate,
            req.NombreEmpresa, req.LogoUrl,
            req.ColorEncabezado, req.ColorTextoHeader, req.ColorAcento,
            req.TextoSecundario, req.TextoPiePagina,
            req.MostrarNumeroPagina, req.MostrarFechaGeneracion, req.MarcaDeAgua), ct);

        return result.IsSuccess
            ? Created(string.Empty, ApiResponse<PdfPlantillaDto>.FromResult(result))
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarPdfPlantillaRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new ActualizarPdfPlantillaCommand(
            id, req.Nombre, req.CodigoTemplate,
            req.NombreEmpresa, req.LogoUrl,
            req.ColorEncabezado, req.ColorTextoHeader, req.ColorAcento,
            req.TextoSecundario, req.TextoPiePagina,
            req.MostrarNumeroPagina, req.MostrarFechaGeneracion, req.MarcaDeAgua,
            currentUser.Id ?? Guid.Empty), ct);

        return result.IsSuccess ? SuccessResponse(result.Value!, result.Message) : BadRequestResponse<object>(result.Error!);
    }

    [HttpPut("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ActivarPdfPlantillaCommand(id), ct);
        return result.IsSuccess ? SuccessResponse(true, "Plantilla activada.") : BadRequestResponse<object>(result.Error!);
    }

    [HttpPut("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DesactivarPdfPlantillaCommand(id), ct);
        return result.IsSuccess ? SuccessResponse(true, "Plantilla desactivada.") : BadRequestResponse<object>(result.Error!);
    }

    [HttpPost("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id, [FromBody] PreviewPdfRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new PreviewPdfQuery(id, req.Datos), ct);
        if (!result.IsSuccess) return BadRequestResponse<object>(result.Error!);

        return File(result.Value!, "application/pdf", "preview.pdf");
    }
}
