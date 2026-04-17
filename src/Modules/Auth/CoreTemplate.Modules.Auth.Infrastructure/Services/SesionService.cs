using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Gestiona la lógica de límites de sesiones simultáneas.
/// Jerarquía: Tenant > Global > Default (5).
/// </summary>
internal sealed class SesionService(
    ISesionRepository _sesionRepo,
    IConfiguracionTenantRepository _configTenantRepo,
    ICurrentTenant _currentTenant,
    IOptions<AuthSettings> _authSettings,
    IOptions<TenantSettings> _tenantSettings) : ISesionService
{
    public async Task<bool> VerificarYAplicarLimiteAsync(
        Guid usuarioId, TipoUsuario tipoUsuario, CancellationToken ct = default)
    {
        // Sistema e Integracion no tienen límite de sesiones
        if (tipoUsuario != TipoUsuario.Humano)
            return true;

        var limite = await ObtenerLimiteAsync(ct);
        var activas = await _sesionRepo.ContarActivasAsync(usuarioId, ct);

        if (activas < limite)
            return true;

        if (_authSettings.Value.AccionAlLlegarLimiteSesiones == AccionAlLlegarLimiteSesiones.CerrarMasAntigua)
        {
            var masAntigua = await _sesionRepo.GetMasAntiguaActivaAsync(usuarioId, ct);
            if (masAntigua is not null)
            {
                masAntigua.Revocar();
                await _sesionRepo.UpdateAsync(masAntigua, ct);
            }
            return true;
        }

        return false;
    }

    private async Task<int> ObtenerLimiteAsync(CancellationToken ct)
    {
        // Jerarquía: Tenant > Global > Default (5)
        if (_tenantSettings.Value.IsMultiTenant
            && _tenantSettings.Value.EnableSessionLimitsPerTenant
            && _currentTenant.TenantId.HasValue)
        {
            var configTenant = await _configTenantRepo.GetByTenantIdAsync(_currentTenant.TenantId.Value, ct);
            if (configTenant?.MaxSesionesSimultaneas.HasValue == true)
                return configTenant.MaxSesionesSimultaneas.Value;
        }

        return _authSettings.Value.MaxSesionesSimultaneas;
    }
}
