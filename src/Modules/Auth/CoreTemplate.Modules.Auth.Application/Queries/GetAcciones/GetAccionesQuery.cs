using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetAcciones;

public sealed record GetAccionesQuery(string? Modulo = null) : IRequest<Result<List<AccionDto>>>;

internal sealed class GetAccionesQueryHandler(
    IAccionRepository _accionRepo,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<GetAccionesQuery, Result<List<AccionDto>>>
{
    public async Task<Result<List<AccionDto>>> Handle(GetAccionesQuery query, CancellationToken ct)
    {
        if (!_authSettings.Value.UseActionCatalog)
            return Result<List<AccionDto>>.Success([]);

        var acciones = await _accionRepo.GetAllAsync(query.Modulo, ct);
        var dtos = acciones.Select(a => new AccionDto(
            a.Id, a.Codigo, a.Nombre, a.Modulo, a.Descripcion, a.EsActiva)).ToList();

        return Result<List<AccionDto>>.Success(dtos);
    }
}
