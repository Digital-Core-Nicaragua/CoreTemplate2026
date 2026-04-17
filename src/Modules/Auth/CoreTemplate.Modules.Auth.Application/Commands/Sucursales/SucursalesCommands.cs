using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.Sucursales;

// ─── Crear sucursal ───────────────────────────────────────────────────────────

public sealed record CrearSucursalCommand(
    string Codigo,
    string Nombre,
    Guid? TenantId = null) : IRequest<Result<Guid>>;

internal sealed class CrearSucursalCommandValidator : AbstractValidator<CrearSucursalCommand>
{
    public CrearSucursalCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

internal sealed class CrearSucursalCommandHandler(
    ISucursalRepository _sucursalRepo,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<CrearSucursalCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearSucursalCommand cmd, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result<Guid>.Failure("Las sucursales no están habilitadas en este sistema.");

        if (await _sucursalRepo.ExistsByCodigoAsync(cmd.Codigo, cmd.TenantId, ct))
            return Result<Guid>.Failure("Ya existe una sucursal con ese código.");

        var result = Sucursal.Crear(cmd.Codigo, cmd.Nombre, cmd.TenantId);
        if (!result.IsSuccess)
            return Result<Guid>.Failure(result.Error!);

        await _sucursalRepo.AddAsync(result.Value!, ct);
        return Result<Guid>.Success(result.Value!.Id, "Sucursal creada correctamente.");
    }
}

// ─── Asignar sucursal a usuario ───────────────────────────────────────────────

public sealed record AsignarSucursalUsuarioCommand(
    Guid UsuarioId,
    Guid SucursalId) : IRequest<Result>;

internal sealed class AsignarSucursalUsuarioCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISucursalRepository _sucursalRepo,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<AsignarSucursalUsuarioCommand, Result>
{
    public async Task<Result> Handle(AsignarSucursalUsuarioCommand cmd, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result.Failure("Las sucursales no están habilitadas en este sistema.");

        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var sucursal = await _sucursalRepo.GetByIdAsync(cmd.SucursalId, ct);
        if (sucursal is null || !sucursal.EsActiva)
            return Result.Failure("La sucursal no existe o está inactiva.");

        var result = usuario.AsignarSucursal(cmd.SucursalId);
        if (!result.IsSuccess)
            return result;

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Remover sucursal de usuario ──────────────────────────────────────────────

public sealed record RemoverSucursalUsuarioCommand(
    Guid UsuarioId,
    Guid SucursalId) : IRequest<Result>;

internal sealed class RemoverSucursalUsuarioCommandHandler(
    IUsuarioRepository _usuarioRepo,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<RemoverSucursalUsuarioCommand, Result>
{
    public async Task<Result> Handle(RemoverSucursalUsuarioCommand cmd, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result.Failure("Las sucursales no están habilitadas en este sistema.");

        var usuario = await _usuarioRepo.GetByIdAsync(cmd.UsuarioId, ct);
        if (usuario is null)
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var result = usuario.RemoverSucursal(cmd.SucursalId);
        if (!result.IsSuccess)
            return result;

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Cambiar sucursal activa (perfil propio) ──────────────────────────────────

public sealed record CambiarSucursalActivaCommand(Guid SucursalId) : IRequest<Result<SucursalDto>>;

internal sealed class CambiarSucursalActivaCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISucursalRepository _sucursalRepo,
    ICurrentUser _currentUser,
    IOptions<OrganizationSettings> _orgSettings) : IRequestHandler<CambiarSucursalActivaCommand, Result<SucursalDto>>
{
    public async Task<Result<SucursalDto>> Handle(CambiarSucursalActivaCommand cmd, CancellationToken ct)
    {
        if (!_orgSettings.Value.EnableBranches)
            return Result<SucursalDto>.Failure("Las sucursales no están habilitadas en este sistema.");

        if (!_currentUser.Id.HasValue)
            return Result<SucursalDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var usuario = await _usuarioRepo.GetByIdAsync(_currentUser.Id.Value, ct);
        if (usuario is null)
            return Result<SucursalDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);

        var result = usuario.CambiarSucursalPrincipal(cmd.SucursalId);
        if (!result.IsSuccess)
            return Result<SucursalDto>.Failure(result.Error!);

        await _usuarioRepo.UpdateAsync(usuario, ct);

        var sucursal = await _sucursalRepo.GetByIdAsync(cmd.SucursalId, ct);
        return Result<SucursalDto>.Success(
            new SucursalDto(sucursal!.Id, sucursal.Codigo, sucursal.Nombre, sucursal.EsActiva),
            "Sucursal activa actualizada.");
    }
}
