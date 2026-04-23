using CoreTemplate.Api.Common;
using CoreTemplate.Modules.EmailTemplates.Api.Contracts;
using CoreTemplate.Modules.EmailTemplates.Application.Commands;
using CoreTemplate.Modules.EmailTemplates.Application.DTOs;
using CoreTemplate.Modules.EmailTemplates.Application.Queries;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.EmailTemplates.Api.Controllers;

[Authorize]
[Route("api/email-templates")]
public sealed class EmailTemplatesController(ISender sender, ICurrentUser currentUser) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? modulo, [FromQuery] bool? soloActivos, CancellationToken ct)
    {
        var result = await sender.Send(new GetPlantillasQuery(modulo, soloActivos), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPlantillaByIdQuery(id), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : NotFoundResponse<object>(result.Error!);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPlantillaRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new CrearPlantillaCommand(
            req.Codigo, req.Nombre, req.Modulo, req.Asunto, req.CuerpoHtml, req.Variables, req.UsarLayout), ct);
        return result.IsSuccess
            ? Created(string.Empty, ApiResponse<EmailTemplateDto>.FromResult(result))
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarPlantillaRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new ActualizarPlantillaCommand(
            id, req.Asunto, req.CuerpoHtml, req.Variables, currentUser.Id ?? Guid.Empty), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpPut("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ActivarPlantillaCommand(id), ct);
        return result.IsSuccess
            ? SuccessResponse(true, "Plantilla activada.")
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpPut("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DesactivarPlantillaCommand(id), ct);
        return result.IsSuccess
            ? SuccessResponse(true, "Plantilla desactivada.")
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpPost("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id, [FromBody] PreviewPlantillaRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new PreviewPlantillaQuery(id, req.Variables), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    [HttpPost("{id:guid}/enviar-prueba")]
    public async Task<IActionResult> EnviarPrueba(Guid id, [FromBody] EnviarPruebaRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new EnviarPruebaCommand(id, req.Destinatario, req.Variables), ct);
        return result.IsSuccess
            ? SuccessResponse(true, "Correo de prueba enviado.")
            : BadRequestResponse<object>(result.Error!);
    }
}
