using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetPermisosEfectivos;

/// <summary>
/// Retorna los permisos efectivos del usuario según su sucursal activa.
/// Si EnableBranches = false, retorna los permisos globales del usuario.
/// </summary>
public sealed record GetPermisosEfectivosQuery : IRequest<Result<List<string>>>;

internal sealed class GetPermisosEfectivosQueryHandler(
    IUsuarioRepository _usuarioRepo,
    IRolRepository _rolRepo,
    IAsignacionRolRepository _asignacionRepo,
    IPermisoRepository _permisoRepo,
    ICurrentUser _currentUser,
    ICurrentBranch _currentBranch,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<GetPermisosEfectivosQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(GetPermisosEfectivosQuery query, CancellationToken ct)
    {
        if (!_currentUser.Id.HasValue)
            return Result<List<string>>.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var usuario = await _usuarioRepo.GetByIdAsync(_currentUser.Id.Value, ct);
        if (usuario is null)
            return Result<List<string>>.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var todosPermisos = await _permisoRepo.GetAllAsync(ct);
        var permisos = new HashSet<string>();

        if (_orgSettings.Value.EnableBranches && _currentBranch.BranchId.HasValue)
        {
            // Permisos según roles de la sucursal activa
            var asignaciones = await _asignacionRepo.GetByUsuarioSucursalAsync(
                usuario.Id, _currentBranch.BranchId.Value, ct);

            foreach (var asignacion in asignaciones)
            {
                var rol = await _rolRepo.GetByIdAsync(asignacion.RolId, ct);
                if (rol is null) continue;

                foreach (var rp in rol.Permisos)
                {
                    var permiso = todosPermisos.FirstOrDefault(p => p.Id == rp.PermisoId);
                    if (permiso is not null)
                        permisos.Add(permiso.Codigo);
                }
            }
        }
        else
        {
            // Permisos globales del usuario (modo sin sucursales)
            foreach (var ur in usuario.Roles)
            {
                var rol = await _rolRepo.GetByIdAsync(ur.RolId, ct);
                if (rol is null) continue;

                foreach (var rp in rol.Permisos)
                {
                    var permiso = todosPermisos.FirstOrDefault(p => p.Id == rp.PermisoId);
                    if (permiso is not null)
                        permisos.Add(permiso.Codigo);
                }
            }
        }

        return Result<List<string>>.Success([.. permisos]);
    }
}
