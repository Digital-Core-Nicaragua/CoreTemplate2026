using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.AsignacionRoles;

// ─── Asignar rol por sucursal ─────────────────────────────────────────────────

public sealed record AsignarRolSucursalCommand(
    Guid UsuarioId,
    Guid SucursalId,
    Guid RolId) : IRequest<Result>;

internal sealed class AsignarRolSucursalCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISucursalRepository _sucursalRepo,
    IRolRepository _rolRepo,
    IAsignacionRolRepository _asignacionRepo,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<AsignarRolSucursalCommand, Result>
{
    public async Task<Result> Handle(AsignarRolSucursalCommand cmd, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result.Failure("Las sucursales no están habilitadas en este sistema.");

        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        // Verificar que el usuario tenga asignada esa sucursal
        if (!usuario.Sucursales.Any(s => s.SucursalId == cmd.SucursalId))
            return Result.Failure("El usuario no tiene asignada esta sucursal.");

        var sucursal = await _sucursalRepo.GetByIdAsync(cmd.SucursalId, ct);
        if (sucursal is null || !sucursal.EsActiva)
            return Result.Failure("La sucursal no existe o está inactiva.");

        var rol = await _rolRepo.GetByIdAsync(cmd.RolId, ct);
        if (rol is null)
            return Result.Failure(AuthErrorMessages.RolNoEncontrado);

        if (await _asignacionRepo.ExisteAsync(cmd.UsuarioId, cmd.SucursalId, cmd.RolId, ct))
            return Result.Failure("El usuario ya tiene este rol en esta sucursal.");

        var asignacion = AsignacionRol.Crear(cmd.UsuarioId, cmd.SucursalId, cmd.RolId);
        await _asignacionRepo.AddAsync(asignacion.Value!, ct);
        return Result.Success();
    }
}

// ─── Quitar rol por sucursal ──────────────────────────────────────────────────

public sealed record QuitarRolSucursalCommand(
    Guid UsuarioId,
    Guid SucursalId,
    Guid RolId) : IRequest<Result>;

internal sealed class QuitarRolSucursalCommandHandler(
    IAsignacionRolRepository _asignacionRepo,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<QuitarRolSucursalCommand, Result>
{
    public async Task<Result> Handle(QuitarRolSucursalCommand cmd, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result.Failure("Las sucursales no están habilitadas en este sistema.");

        var asignaciones = await _asignacionRepo.GetByUsuarioSucursalAsync(cmd.UsuarioId, cmd.SucursalId, ct);
        var asignacion = asignaciones.FirstOrDefault(a => a.RolId == cmd.RolId);

        if (asignacion is null)
            return Result.Failure("El usuario no tiene este rol en esta sucursal.");

        await _asignacionRepo.DeleteAsync(asignacion, ct);
        return Result.Success();
    }
}
