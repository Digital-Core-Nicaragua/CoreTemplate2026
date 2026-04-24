using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Configuracion.Api.Contracts;
using CoreTemplate.Modules.Configuracion.Application.Commands;
using CoreTemplate.Modules.Configuracion.Application.DTOs;
using CoreTemplate.Modules.Configuracion.Application.Queries;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Configuracion.Api.Controllers;

[Authorize]
[Route("api/configuracion")]
public sealed class ConfiguracionController(ISender sender, ICurrentUser currentUser) : BaseApiController
{
    /// <summary>Lista todos los parámetros agrupados por categoría.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var result = await sender.Send(new GetConfiguracionQuery(), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Lista los parámetros de un grupo específico.</summary>
    [HttpGet("grupo/{grupo}")]
    public async Task<IActionResult> ListarPorGrupo(string grupo, CancellationToken ct)
    {
        var result = await sender.Send(new GetConfiguracionPorGrupoQuery(grupo), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Obtiene un parámetro por su clave.</summary>
    [HttpGet("{clave}")]
    public async Task<IActionResult> ObtenerPorClave(string clave, CancellationToken ct)
    {
        var result = await sender.Send(new GetConfiguracionPorClaveQuery(clave), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : NotFoundResponse<object>(result.Error!);
    }

    /// <summary>Actualiza el valor de un parámetro.</summary>
    [HttpPut("{clave}")]
    public async Task<IActionResult> Actualizar(
        string clave, [FromBody] ActualizarConfiguracionRequest req, CancellationToken ct)
    {
        var result = await sender.Send(
            new ActualizarConfiguracionCommand(clave, req.Valor, currentUser.Id ?? Guid.Empty), ct);

        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }
}
