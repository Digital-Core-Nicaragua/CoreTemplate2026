using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Commands.Roles;

// ─── Crear Rol ────────────────────────────────────────────────────────────────

public sealed record CrearRolCommand(
    string Nombre,
    string Descripcion,
    IReadOnlyList<Guid> PermisoIds) : IRequest<Result<Guid>>;

internal sealed class CrearRolCommandValidator : AbstractValidator<CrearRolCommand>
{
    public CrearRolCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100)
            .WithMessage("El nombre del rol es requerido y no puede superar 100 caracteres.");
    }
}

internal sealed class CrearRolCommandHandler(
    IRolRepository _rolRepo,
    IPermisoRepository _permisoRepo,
    ICurrentTenant _currentTenant) : IRequestHandler<CrearRolCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearRolCommand cmd, CancellationToken ct)
    {
        if (await _rolRepo.ExistsByNombreAsync(cmd.Nombre, _currentTenant.TenantId, ct))
        {
            return Result<Guid>.Failure(AuthErrorMessages.RolNombreYaExiste);
        }

        var rolResult = Rol.Crear(cmd.Nombre, cmd.Descripcion, esSistema: false, _currentTenant.TenantId);
        if (!rolResult.IsSuccess)
        {
            return Result<Guid>.Failure(rolResult.Error!);
        }

        var rol = rolResult.Value!;

        foreach (var permisoId in cmd.PermisoIds)
        {
            var permiso = await _permisoRepo.GetByIdAsync(permisoId, ct);
            if (permiso is null)
            {
                return Result<Guid>.Failure(AuthErrorMessages.PermisoNoEncontrado);
            }

            rol.AgregarPermiso(permisoId);
        }

        await _rolRepo.AddAsync(rol, ct);
        return Result<Guid>.Success(rol.Id, AuthSuccessMessages.RolCreado);
    }
}

// ─── Actualizar Rol ───────────────────────────────────────────────────────────

public sealed record ActualizarRolCommand(
    Guid RolId,
    string Nombre,
    string Descripcion,
    IReadOnlyList<Guid> PermisoIds) : IRequest<Result>;

internal sealed class ActualizarRolCommandValidator : AbstractValidator<ActualizarRolCommand>
{
    public ActualizarRolCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100)
            .WithMessage("El nombre del rol es requerido y no puede superar 100 caracteres.");
    }
}

internal sealed class ActualizarRolCommandHandler(
    IRolRepository _rolRepo,
    IPermisoRepository _permisoRepo,
    ICurrentTenant _currentTenant) : IRequestHandler<ActualizarRolCommand, Result>
{
    public async Task<Result> Handle(ActualizarRolCommand cmd, CancellationToken ct)
    {
        var rol = await _rolRepo.GetByIdAsync(cmd.RolId, ct);
        if (rol is null)
        {
            return Result.Failure(AuthErrorMessages.RolNoEncontrado);
        }

        // Verificar nombre único (excluyendo el rol actual)
        var existeNombre = await _rolRepo.ExistsByNombreAsync(cmd.Nombre, _currentTenant.TenantId, ct);
        if (existeNombre && !rol.Nombre.Equals(cmd.Nombre, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(AuthErrorMessages.RolNombreYaExiste);
        }

        var updateResult = rol.Actualizar(cmd.Nombre, cmd.Descripcion);
        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        // Sincronizar permisos: quitar los que no están en la nueva lista
        foreach (var permiso in rol.Permisos.ToList())
        {
            if (!cmd.PermisoIds.Contains(permiso.PermisoId))
            {
                rol.QuitarPermiso(permiso.PermisoId);
            }
        }

        // Agregar los nuevos
        foreach (var permisoId in cmd.PermisoIds)
        {
            if (!rol.Permisos.Any(p => p.PermisoId == permisoId))
            {
                var permiso = await _permisoRepo.GetByIdAsync(permisoId, ct);
                if (permiso is null)
                {
                    return Result.Failure(AuthErrorMessages.PermisoNoEncontrado);
                }

                rol.AgregarPermiso(permisoId);
            }
        }

        await _rolRepo.UpdateAsync(rol, ct);
        return Result.Success();
    }
}

// ─── Eliminar Rol ─────────────────────────────────────────────────────────────

public sealed record EliminarRolCommand(Guid RolId) : IRequest<Result>;

internal sealed class EliminarRolCommandHandler(
    IRolRepository _rolRepo) : IRequestHandler<EliminarRolCommand, Result>
{
    public async Task<Result> Handle(EliminarRolCommand cmd, CancellationToken ct)
    {
        var rol = await _rolRepo.GetByIdAsync(cmd.RolId, ct);
        if (rol is null)
        {
            return Result.Failure(AuthErrorMessages.RolNoEncontrado);
        }

        if (!rol.PuedeEliminarse())
        {
            return Result.Failure(AuthErrorMessages.RolEsSistema);
        }

        if (await _rolRepo.TieneUsuariosAsync(cmd.RolId, ct))
        {
            return Result.Failure(AuthErrorMessages.RolTieneUsuarios);
        }

        await _rolRepo.DeleteAsync(rol, ct);
        return Result.Success();
    }
}
