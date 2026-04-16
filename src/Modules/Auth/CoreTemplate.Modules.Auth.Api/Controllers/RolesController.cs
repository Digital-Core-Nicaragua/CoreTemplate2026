using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.Roles;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Application.Queries;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints de gestión de roles.
/// </summary>
[Route("api/roles")]
[Authorize]
public sealed class RolesController(IMediator _mediator) : BaseApiController
{
    /// <summary>Lista todos los roles disponibles.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRolesQuery(), ct);
        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Obtiene el detalle de un rol por su ID.</summary>
    [HttpGet("{id:guid}", Name = nameof(RolesController) + "_" + nameof(GetById))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRolByIdQuery(id), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<RolDto>(result.Errors);
        }

        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Crea un nuevo rol con los permisos indicados.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearRolRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearRolCommand(request.Nombre, request.Descripcion, request.PermisoIds), ct);

        if (!result.IsSuccess)
        {
            return ConflictResponse<Guid>(result.Errors);
        }

        return CreatedResponse(
            nameof(RolesController) + "_" + nameof(GetById),
            new { id = result.Value },
            result.Value,
            result.Message);
    }

    /// <summary>Actualiza el nombre, descripción y permisos de un rol.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarRolRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ActualizarRolCommand(id, request.Nombre, request.Descripcion, request.PermisoIds), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    /// <summary>Elimina un rol (solo roles no del sistema y sin usuarios asignados).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new EliminarRolCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.EliminadoExitosamente);
    }
}
