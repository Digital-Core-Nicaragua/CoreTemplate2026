using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetConfiguracionTenant;

public sealed record ConfiguracionTenantDto(
    Guid TenantId,
    int? MaxSesionesSimultaneas,
    DateTime ModificadoEn);

public sealed record GetConfiguracionTenantQuery(Guid TenantId) : IRequest<Result<ConfiguracionTenantDto?>>;

internal sealed class GetConfiguracionTenantQueryHandler(
    IConfiguracionTenantRepository _configRepo) : IRequestHandler<GetConfiguracionTenantQuery, Result<ConfiguracionTenantDto?>>
{
    public async Task<Result<ConfiguracionTenantDto?>> Handle(GetConfiguracionTenantQuery query, CancellationToken ct)
    {
        var config = await _configRepo.GetByTenantIdAsync(query.TenantId, ct);

        if (config is null)
            return Result<ConfiguracionTenantDto?>.Success(null);

        return Result<ConfiguracionTenantDto?>.Success(
            new ConfiguracionTenantDto(config.TenantId, config.MaxSesionesSimultaneas, config.ModificadoEn));
    }
}
