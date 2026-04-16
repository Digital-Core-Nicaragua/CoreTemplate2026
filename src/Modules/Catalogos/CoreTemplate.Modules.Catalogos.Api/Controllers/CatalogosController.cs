using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Catalogos.Api.Contracts;
using CoreTemplate.Modules.Catalogos.Application.Commands;
using CoreTemplate.Modules.Catalogos.Application.DTOs;
using CoreTemplate.Modules.Catalogos.Application.Queries;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Catalogos.Api.Controllers;

/// <summary>
/// Endpoints del módulo Catálogos.
/// Sirve como patrón de referencia para crear nuevos catálogos en el sistema.
/// </summary>
[Route("api/catalogos")]
[Authorize]
public sealed class CatalogosController(IMediator _mediator) : BaseApiController
{
    /// <summary>Lista ítems de catálogo con paginación, filtros y búsqueda.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] bool? soloActivos = null,
        [FromQuery] string? busqueda = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCatalogoItemsQuery(pagina, tamanoPagina, soloActivos, busqueda), ct);

        return SuccessPagedResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Obtiene el detalle de un ítem por su ID.</summary>
    [HttpGet("{id:guid}", Name = nameof(CatalogosController) + "_" + nameof(GetById))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCatalogoItemByIdQuery(id), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<CatalogoItemDto>(result.Errors);
        }

        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Crea un nuevo ítem de catálogo.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearCatalogoItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearCatalogoItemCommand(request.Codigo, request.Nombre, request.Descripcion), ct);

        if (!result.IsSuccess)
        {
            return ConflictResponse<Guid>(result.Errors);
        }

        return CreatedResponse(
            nameof(CatalogosController) + "_" + nameof(GetById),
            new { id = result.Value },
            result.Value,
            result.Message);
    }

    /// <summary>Activa un ítem de catálogo.</summary>
    [HttpPut("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ActivarCatalogoItemCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActivadoExitosamente);
    }

    /// <summary>Desactiva un ítem de catálogo.</summary>
    [HttpPut("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DesactivarCatalogoItemCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.DesactivadoExitosamente);
    }
}
