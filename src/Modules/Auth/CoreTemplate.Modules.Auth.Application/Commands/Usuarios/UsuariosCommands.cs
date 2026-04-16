using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Commands.Usuarios;

// ─── Activar Usuario ──────────────────────────────────────────────────────────

public sealed record ActivarUsuarioCommand(Guid UsuarioId) : IRequest<Result>;

internal sealed class ActivarUsuarioCommandHandler(
    IUsuarioRepository _usuarioRepo) : IRequestHandler<ActivarUsuarioCommand, Result>
{
    public async Task<Result> Handle(ActivarUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var result = usuario.Activar();
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Desactivar Usuario ───────────────────────────────────────────────────────

public sealed record DesactivarUsuarioCommand(Guid UsuarioId) : IRequest<Result>;

internal sealed class DesactivarUsuarioCommandHandler(
    IUsuarioRepository _usuarioRepo) : IRequestHandler<DesactivarUsuarioCommand, Result>
{
    public async Task<Result> Handle(DesactivarUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var result = usuario.Desactivar();
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Desbloquear Usuario ──────────────────────────────────────────────────────

public sealed record DesbloquearUsuarioCommand(Guid UsuarioId) : IRequest<Result>;

internal sealed class DesbloquearUsuarioCommandHandler(
    IUsuarioRepository _usuarioRepo) : IRequestHandler<DesbloquearUsuarioCommand, Result>
{
    public async Task<Result> Handle(DesbloquearUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var result = usuario.Desbloquear();
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Asignar Rol ──────────────────────────────────────────────────────────────

public sealed record AsignarRolCommand(Guid UsuarioId, Guid RolId) : IRequest<Result>;

internal sealed class AsignarRolCommandHandler(
    IUsuarioRepository _usuarioRepo,
    IRolRepository _rolRepo) : IRequestHandler<AsignarRolCommand, Result>
{
    public async Task<Result> Handle(AsignarRolCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var rol = await _rolRepo.GetByIdAsync(cmd.RolId, ct);
        if (rol is null)
        {
            return Result.Failure(AuthErrorMessages.RolNoEncontrado);
        }

        var result = usuario.AsignarRol(cmd.RolId);
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Quitar Rol ───────────────────────────────────────────────────────────────

public sealed record QuitarRolCommand(Guid UsuarioId, Guid RolId) : IRequest<Result>;

internal sealed class QuitarRolCommandHandler(
    IUsuarioRepository _usuarioRepo) : IRequestHandler<QuitarRolCommand, Result>
{
    public async Task<Result> Handle(QuitarRolCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var result = usuario.QuitarRol(cmd.RolId);
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}
