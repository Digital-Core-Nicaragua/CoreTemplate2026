using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.AsignacionRoles;
using CoreTemplate.Modules.Auth.Application.Commands.Sesiones;
using CoreTemplate.Modules.Auth.Application.Commands.Usuarios;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Application.Queries;
using CoreTemplate.Modules.Auth.Application.Queries.GetSesionesUsuario;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints de gestión de usuarios (requiere rol Admin o SuperAdmin).
/// </summary>
[Route("api/usuarios")]
[Authorize]
public sealed class UsuariosController(IMediator _mediator) : BaseApiController
{
    /// <summary>Lista usuarios con paginación y filtro opcional por estado.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] EstadoUsuario? estado = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsuariosQuery(pagina, tamanoPagina, estado), ct);
        return SuccessPagedResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Obtiene el detalle de un usuario por su ID.</summary>
    [HttpGet("{id:guid}", Name = nameof(UsuariosController) + "_" + nameof(GetById))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUsuarioByIdQuery(id), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<UsuarioDto>(result.Errors);
        }

        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Activa un usuario.</summary>
    [HttpPut("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ActivarUsuarioCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActivadoExitosamente);
    }

    /// <summary>Desactiva un usuario.</summary>
    [HttpPut("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DesactivarUsuarioCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.DesactivadoExitosamente);
    }

    /// <summary>Desbloquea un usuario bloqueado por intentos fallidos.</summary>
    [HttpPut("{id:guid}/desbloquear")]
    public async Task<IActionResult> Desbloquear(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DesbloquearUsuarioCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    /// <summary>Asigna un rol a un usuario.</summary>
    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AsignarRol(Guid id, [FromBody] AsignarRolRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AsignarRolCommand(id, request.RolId), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    /// <summary>Quita un rol de un usuario.</summary>
    [HttpDelete("{id:guid}/roles/{rolId:guid}")]
    public async Task<IActionResult> QuitarRol(Guid id, Guid rolId, CancellationToken ct)
    {
        var result = await _mediator.Send(new QuitarRolCommand(id, rolId), ct);

        if (!result.IsSuccess)
        {
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    /// <summary>Lista las sesiones activas de un usuario (admin).</summary>
    [HttpGet("{id:guid}/sesiones")]
    public async Task<IActionResult> GetSesiones(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSesionesUsuarioQuery(id), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<object>(result.Error!);
        }

        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Cierra todas las sesiones activas de un usuario (admin).</summary>
    [HttpDelete("{id:guid}/sesiones")]
    public async Task<IActionResult> CerrarTodasSesiones(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CerrarTodasSesionesUsuarioCommand(id), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "Todas las sesiones del usuario han sido cerradas.");
    }

    /// <summary>Asigna un rol a un usuario en una sucursal específica.</summary>
    [HttpPost("{id:guid}/sucursales/{sucursalId:guid}/roles")]
    public async Task<IActionResult> AsignarRolSucursal(
        Guid id, Guid sucursalId, [FromBody] AsignarRolRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AsignarRolSucursalCommand(id, sucursalId, request.RolId), ct);

        if (!result.IsSuccess)
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    /// <summary>Quita un rol de un usuario en una sucursal específica.</summary>
    [HttpDelete("{id:guid}/sucursales/{sucursalId:guid}/roles/{rolId:guid}")]
    public async Task<IActionResult> QuitarRolSucursal(
        Guid id, Guid sucursalId, Guid rolId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new QuitarRolSucursalCommand(id, sucursalId, rolId), ct);

        if (!result.IsSuccess)
            return result.Error!.Contains("no fue encontrado")
                ? NotFoundResponse<object>(result.Error!)
                : ConflictResponse<object>(result.Error!);

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }
}
