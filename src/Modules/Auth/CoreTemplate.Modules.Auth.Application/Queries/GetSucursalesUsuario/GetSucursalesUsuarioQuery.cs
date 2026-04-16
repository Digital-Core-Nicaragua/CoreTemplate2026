using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetSucursalesUsuario;

public sealed record GetSucursalesUsuarioQuery(Guid UsuarioId) : IRequest<Result<List<UsuarioSucursalDto>>>;

internal sealed class GetSucursalesUsuarioQueryHandler(
    IUsuarioRepository _usuarioRepo,
    ISucursalRepository _sucursalRepo,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<GetSucursalesUsuarioQuery, Result<List<UsuarioSucursalDto>>>
{
    public async Task<Result<List<UsuarioSucursalDto>>> Handle(GetSucursalesUsuarioQuery query, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result<List<UsuarioSucursalDto>>.Success([]);

        var usuario = await _usuarioRepo.GetByIdAsync(query.UsuarioId, ct);
        if (usuario is null)
            return Result<List<UsuarioSucursalDto>>.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var dtos = new List<UsuarioSucursalDto>();
        foreach (var us in usuario.Sucursales)
        {
            var sucursal = await _sucursalRepo.GetByIdAsync(us.SucursalId, ct);
            if (sucursal is not null)
                dtos.Add(new UsuarioSucursalDto(sucursal.Id, sucursal.Codigo, sucursal.Nombre, us.EsPrincipal));
        }

        return Result<List<UsuarioSucursalDto>>.Success(dtos);
    }
}
