using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetSucursales;

public sealed record GetSucursalesQuery(Guid? TenantId = null) : IRequest<Result<List<SucursalDto>>>;

internal sealed class GetSucursalesQueryHandler(
    ISucursalRepository _sucursalRepo,
    ICurrentTenant _currentTenant,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<GetSucursalesQuery, Result<List<SucursalDto>>>
{
    public async Task<Result<List<SucursalDto>>> Handle(GetSucursalesQuery query, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result<List<SucursalDto>>.Success([]);

        var tenantId = _currentTenant.EsMultiTenant ? _currentTenant.TenantId : null;
        var sucursales = await _sucursalRepo.GetAllAsync(tenantId, ct);

        var dtos = sucursales.Select(s => new SucursalDto(s.Id, s.Codigo, s.Nombre, s.EsActiva)).ToList();
        return Result<List<SucursalDto>>.Success(dtos);
    }
}
