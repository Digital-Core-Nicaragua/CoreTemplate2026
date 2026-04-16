using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.Sucursales;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Application.Queries.GetSucursales;
using CoreTemplate.Modules.Auth.Application.Queries.GetSucursalesUsuario;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints de gestión de sucursales.
/// Solo disponibles cuando OrganizationSettings:EnableBranches = true.
/// </summary>
[Route("api/sucursales")]
[Authorize]
public sealed class SucursalesController(IMediator _mediator) : BaseApiController
{
    /// <summary>Lista todas las sucursales del tenant.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSucursalesQuery(), ct);
        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Crea una nueva sucursal.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSucursalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CrearSucursalCommand(request.Codigo, request.Nombre), ct);

        if (!result.IsSuccess)
            return ConflictResponse<Guid>(result.Errors);

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Lista las sucursales asignadas a un usuario.</summary>
    [HttpGet("usuarios/{usuarioId:guid}")]
    public async Task<IActionResult> GetSucursalesUsuario(Guid usuarioId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSucursalesUsuarioQuery(usuarioId), ct);

        if (!result.IsSuccess)
            return NotFoundResponse<object>(result.Error!);

        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Asigna una sucursal a un usuario.</summary>
    [HttpPost("usuarios/{usuarioId:guid}")]
    public async Task<IActionResult> AsignarSucursal(
        Guid usuarioId, [FromBody] AsignarSucursalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AsignarSucursalUsuarioCommand(usuarioId, request.SucursalId), ct);

        if (!result.IsSuccess)
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    /// <summary>Remueve una sucursal de un usuario.</summary>
    [HttpDelete("usuarios/{usuarioId:guid}/{sucursalId:guid}")]
    public async Task<IActionResult> RemoverSucursal(
        Guid usuarioId, Guid sucursalId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RemoverSucursalUsuarioCommand(usuarioId, sucursalId), ct);

        if (!result.IsSuccess)
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }
}
