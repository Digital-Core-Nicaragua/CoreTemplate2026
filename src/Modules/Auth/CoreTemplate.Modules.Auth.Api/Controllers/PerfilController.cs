using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.CambiarPassword;
using CoreTemplate.Modules.Auth.Application.Commands.Sesiones;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Application.Queries;
using CoreTemplate.Modules.Auth.Application.Queries.GetMisSesiones;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints del perfil del usuario autenticado.
/// </summary>
[Route("api/perfil")]
[Authorize]
public sealed class PerfilController(IMediator _mediator) : BaseApiController
{
    /// <summary>Obtiene el perfil del usuario autenticado.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMiPerfil(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMiPerfilQuery(), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<UsuarioDto>(result.Errors);
        }

        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Cambia la contraseña del usuario autenticado.</summary>
    [HttpPut("cambiar-password")]
    public async Task<IActionResult> CambiarPassword(
        [FromBody] CambiarPasswordRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var result = await _mediator.Send(
            new CambiarPasswordCommand(
                request.PasswordActual,
                request.NuevoPassword,
                request.ConfirmPassword,
                ip,
                userAgent,
                accessToken), ct);

        if (!result.IsSuccess)
        {
            return BadRequestResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "Contraseña actualizada correctamente.");
    }

    /// <summary>Lista las sesiones activas del usuario autenticado.</summary>
    [HttpGet("sesiones")]
    public async Task<IActionResult> GetMisSesiones(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMisSesionesQuery(), ct);
        return SuccessResponse(result.Value!, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Cierra una sesión específica del usuario autenticado.</summary>
    [HttpDelete("sesiones/{id:guid}")]
    public async Task<IActionResult> CerrarSesion(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CerrarSesionCommand(id), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "Sesión cerrada correctamente.");
    }

    /// <summary>Cierra todas las sesiones excepto la actual.</summary>
    [HttpDelete("sesiones/otras")]
    public async Task<IActionResult> CerrarOtrasSesiones(
        [FromQuery] Guid sesionActualId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CerrarOtrasSesionesCommand(sesionActualId), ct);

        if (!result.IsSuccess)
        {
            return BadRequestResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "Otras sesiones cerradas correctamente.");
    }

    /// <summary>Cambia la sucursal activa del usuario autenticado.</summary>
    [HttpPut("sucursal-activa")]
    public async Task<IActionResult> CambiarSucursalActiva(
        [FromBody] CambiarSucursalActivaRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CoreTemplate.Modules.Auth.Application.Commands.Sucursales.CambiarSucursalActivaCommand(
                request.SucursalId), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(result.Value!, result.Message);
    }
}
