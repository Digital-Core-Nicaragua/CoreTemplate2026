using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetClientes;

/// <summary>Obtiene una página de clientes del portal. Solo para admin.</summary>
public sealed record GetClientesQuery(
    Guid? TenantId,
    EstadoUsuarioCliente? Estado,
    int Pagina,
    int TamanoPagina) : IRequest<Result<PagedResult<ClienteResumenDto>>>;

internal sealed class GetClientesQueryHandler(
    IUsuarioClienteRepository _clienteRepo,
    IOptions<CustomerPortalSettings> _portalSettings)
    : IRequestHandler<GetClientesQuery, Result<PagedResult<ClienteResumenDto>>>
{
    public async Task<Result<PagedResult<ClienteResumenDto>>> Handle(GetClientesQuery query, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableCustomerPortal)
            return Result<PagedResult<ClienteResumenDto>>.Failure("El portal de clientes no está habilitado.");

        var paged = await _clienteRepo.GetPagedAsync(
            query.TenantId, query.Estado, query.Pagina, query.TamanoPagina, ct);

        var dtos = paged.Items.Select(c => new ClienteResumenDto(
            c.Id,
            c.Email ?? string.Empty,
            $"{c.Nombre} {c.Apellido}",
            c.Estado,
            c.EmailVerificado,
            c.CreadoEn)).ToList();

        return Result<PagedResult<ClienteResumenDto>>.Success(
            new PagedResult<ClienteResumenDto>(dtos, paged.Pagina, paged.TamanoPagina, paged.Total));
    }
}
