using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.ConfiguracionTenant;
using CoreTemplate.Modules.Auth.Application.Queries.GetConfiguracionTenant;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints de configuración por tenant.
/// Solo relevantes cuando TenantSettings:IsMultiTenant = true.
/// </summary>
[Route("api/tenants")]
[Authorize]
public sealed class TenantsController(IMediator _mediator) : BaseApiController
{
    /// <summary>Obtiene la configuración de un tenant.</summary>
    [HttpGet("{tenantId:guid}/configuracion")]
    public async Task<IActionResult> GetConfiguracion(Guid tenantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetConfiguracionTenantQuery(tenantId), ct);
        return SuccessResponse(result.Value, CommonSuccessMessages.ConsultaExitosa);
    }

    /// <summary>Configura el límite de sesiones simultáneas para un tenant.</summary>
    [HttpPut("{tenantId:guid}/limite-sesiones")]
    public async Task<IActionResult> ConfigurarLimiteSesiones(
        Guid tenantId, [FromBody] ConfigurarLimiteSesionesRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ConfigurarLimiteSesionesTenantCommand(tenantId, request.MaxSesionesSimultaneas), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }
}
