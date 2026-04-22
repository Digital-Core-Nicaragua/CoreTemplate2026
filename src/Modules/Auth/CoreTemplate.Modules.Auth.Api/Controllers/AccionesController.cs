using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.Acciones;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Application.Queries.GetAcciones;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints del catálogo de acciones.
/// Solo disponibles cuando AuthSettings:UseActionCatalog = true.
/// </summary>
[Route("api/acciones")]
[Authorize]
public sealed class AccionesController(IMediator _mediator) : BaseApiController
{
    /// <summary>Lista todas las acciones, con filtro opcional por módulo.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? modulo, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAccionesQuery(modulo), ct);
        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Crea una nueva acción en el catálogo.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearAccionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearAccionCommand(request.Codigo, request.Nombre, request.Modulo, request.Descripcion ?? ""), ct);

        if (!result.IsSuccess)
        {
            return ConflictResponse<Guid>(result.Errors);
        }

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Activa una acción del catálogo.</summary>
    [HttpPut("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ActivarAccionCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrada")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActivadoExitosamente);
    }

    /// <summary>Desactiva una acción del catálogo.</summary>
    [HttpPut("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DesactivarAccionCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrada")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.DesactivadoExitosamente);
    }
}
