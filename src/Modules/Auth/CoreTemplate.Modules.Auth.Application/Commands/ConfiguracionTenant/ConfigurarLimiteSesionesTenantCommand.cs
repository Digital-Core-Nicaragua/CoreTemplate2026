using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;
using DomainConfiguracionTenant = CoreTemplate.Modules.Auth.Domain.Entities.ConfiguracionTenant;

namespace CoreTemplate.Modules.Auth.Application.Commands.ConfiguracionTenant;

/// <summary>
/// Configura el límite de sesiones simultáneas para un tenant específico.
/// Requiere TenantSettings:EnableSessionLimitsPerTenant = true.
/// </summary>
public sealed record ConfigurarLimiteSesionesTenantCommand(
    Guid TenantId,
    int? MaxSesionesSimultaneas) : IRequest<Result>;

internal sealed class ConfigurarLimiteSesionesTenantCommandHandler(
    IConfiguracionTenantRepository _configRepo,
    IOptions<TenantSettings> _tenantSettings) : IRequestHandler<ConfigurarLimiteSesionesTenantCommand, Result>
{
    public async Task<Result> Handle(ConfigurarLimiteSesionesTenantCommand cmd, CancellationToken ct)
    {
        if (!_tenantSettings.Value.IsMultiTenant || !_tenantSettings.Value.EnableSessionLimitsPerTenant)
            return Result.Failure("Los límites de sesiones por tenant no están habilitados.");

        if (cmd.MaxSesionesSimultaneas.HasValue && cmd.MaxSesionesSimultaneas.Value < 1)
            return Result.Failure("El límite de sesiones debe ser mayor a 0.");

        var config = await _configRepo.GetByTenantIdAsync(cmd.TenantId, ct);

        if (config is null)
        {
            config = DomainConfiguracionTenant.Crear(cmd.TenantId, cmd.MaxSesionesSimultaneas);
            await _configRepo.AddAsync(config, ct);
        }
        else
        {
            config.ActualizarLimiteSesiones(cmd.MaxSesionesSimultaneas);
            await _configRepo.UpdateAsync(config, ct);
        }

        return Result.Success();
    }
}
