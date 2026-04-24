using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auditoria.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auditoria.Api.Controllers;

[Authorize]
[Route("api/auditoria")]
public sealed class AuditoriaController(ISender sender) : BaseApiController
{
    /// <summary>Lista logs de auditoría con filtros opcionales.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? entidad,
        [FromQuery] string? entidadId,
        [FromQuery] Guid? usuarioId,
        [FromQuery] string? accion,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var filtros = new AuditLogFiltros(
            entidad, entidadId, usuarioId, accion,
            fechaDesde, fechaHasta, pagina, Math.Min(tamano, 100));

        var result = await sender.Send(new GetAuditLogsQuery(filtros), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Obtiene el detalle de un log incluyendo valores anteriores y nuevos.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetAuditLogByIdQuery(id), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : NotFoundResponse<object>(result.Error!);
    }

    /// <summary>Lista el historial completo de una entidad específica.</summary>
    [HttpGet("entidad/{entidadId}")]
    public async Task<IActionResult> PorEntidad(
        string entidadId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAuditLogsPorEntidadQuery(entidadId, pagina, tamano), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }

    /// <summary>Lista toda la actividad de un usuario en un período.</summary>
    [HttpGet("usuario/{usuarioId:guid}")]
    public async Task<IActionResult> PorUsuario(
        Guid usuarioId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAuditLogsPorUsuarioQuery(usuarioId, fechaDesde, fechaHasta, pagina, tamano), ct);
        return result.IsSuccess
            ? SuccessResponse(result.Value!, result.Message)
            : BadRequestResponse<object>(result.Error!);
    }
}
