using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Notificaciones.Application.Commands;
using CoreTemplate.Modules.Notificaciones.Application.DTOs;
using CoreTemplate.Modules.Notificaciones.Application.Queries;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Notificaciones.Api.Controllers;

[Authorize]
[Route("api/notificaciones")]
public sealed class NotificacionesController(ISender sender, ICurrentUser currentUser) : BaseApiController
{
    /// <summary>Lista mis notificaciones paginadas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] bool? soloNoLeidas,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var usuarioId = currentUser.Id ?? Guid.Empty;
        var result = await sender.Send(
            new GetMisNotificacionesQuery(usuarioId, soloNoLeidas, pagina, tamano), ct);

        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Retorna el conteo de notificaciones no leídas (para el badge 🔔).</summary>
    [HttpGet("no-leidas/count")]
    public async Task<IActionResult> ConteoNoLeidas(CancellationToken ct)
    {
        var result = await sender.Send(new GetConteoNoLeidasQuery(currentUser.Id ?? Guid.Empty), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Marca una notificación como leída.</summary>
    [HttpPut("{id:guid}/leer")]
    public async Task<IActionResult> MarcarComoLeida(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(
            new MarcarComoLeidaCommand(id, currentUser.Id ?? Guid.Empty), ct);

        return result.IsSuccess
            ? SuccessResponse(true, "Notificación marcada como leída.")
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Marca todas las notificaciones como leídas.</summary>
    [HttpPut("leer-todas")]
    public async Task<IActionResult> MarcarTodasComoLeidas(CancellationToken ct)
    {
        var result = await sender.Send(
            new MarcarTodasComoLeidasCommand(currentUser.Id ?? Guid.Empty), ct);

        return result.IsSuccess
            ? SuccessResponse(true, "Todas las notificaciones marcadas como leídas.")
            : BadRequestResponse<object>(result.Error!);
    }
}
