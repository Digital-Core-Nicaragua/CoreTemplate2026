using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Commands.Sesiones;

// ─── Cerrar sesión específica (usuario propio) ────────────────────────────────

public sealed record CerrarSesionCommand(Guid SesionId) : IRequest<Result>;

internal sealed class CerrarSesionCommandHandler(
    ISesionRepository _sesionRepo,
    ICurrentUser _currentUser) : IRequestHandler<CerrarSesionCommand, Result>
{
    public async Task<Result> Handle(CerrarSesionCommand cmd, CancellationToken ct)
    {
        var sesion = await _sesionRepo.GetByIdAsync(cmd.SesionId, ct);

        if (sesion is null || sesion.UsuarioId != _currentUser.Id)
        {
            return Result.Failure(AuthErrorMessages.SesionNoEncontrada);
        }

        sesion.Revocar();
        await _sesionRepo.UpdateAsync(sesion, ct);
        return Result.Success();
    }
}

// ─── Cerrar todas las sesiones excepto la actual ──────────────────────────────

public sealed record CerrarOtrasSesionesCommand(Guid SesionActualId) : IRequest<Result>;

internal sealed class CerrarOtrasSesionesCommandHandler(
    ISesionRepository _sesionRepo,
    ICurrentUser _currentUser) : IRequestHandler<CerrarOtrasSesionesCommand, Result>
{
    public async Task<Result> Handle(CerrarOtrasSesionesCommand cmd, CancellationToken ct)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var sesiones = await _sesionRepo.GetActivasByUsuarioAsync(_currentUser.Id.Value, ct);

        foreach (var sesion in sesiones.Where(s => s.Id != cmd.SesionActualId))
        {
            sesion.Revocar();
            await _sesionRepo.UpdateAsync(sesion, ct);
        }

        return Result.Success();
    }
}

// ─── Cerrar todas las sesiones de un usuario (admin) ─────────────────────────

public sealed record CerrarTodasSesionesUsuarioCommand(Guid UsuarioId) : IRequest<Result>;

internal sealed class CerrarTodasSesionesUsuarioCommandHandler(
    ISesionRepository _sesionRepo,
    IUsuarioRepository _usuarioRepo) : IRequestHandler<CerrarTodasSesionesUsuarioCommand, Result>
{
    public async Task<Result> Handle(CerrarTodasSesionesUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        await _sesionRepo.RevocarTodasAsync(cmd.UsuarioId, ct);
        return Result.Success();
    }
}
