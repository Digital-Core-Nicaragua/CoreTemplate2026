using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.Acciones;

// ─── Crear acción ─────────────────────────────────────────────────────────────

public sealed record CrearAccionCommand(
    string Codigo,
    string Nombre,
    string Modulo,
    string Descripcion = "") : IRequest<Result<Guid>>;

internal sealed class CrearAccionCommandValidator : AbstractValidator<CrearAccionCommand>
{
    public CrearAccionCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(100)
            .Must(c => c.Contains('.'))
            .WithMessage("El código debe tener formato 'Modulo.Recurso.Accion'.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Modulo).NotEmpty().MaximumLength(50);
    }
}

internal sealed class CrearAccionCommandHandler(
    IAccionRepository _accionRepo,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<CrearAccionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearAccionCommand cmd, CancellationToken ct)
    {
        if (!_authSettings.Value.UseActionCatalog)
            return Result<Guid>.Failure("El catálogo de acciones no está habilitado.");

        if (await _accionRepo.ExistsByCodigoAsync(cmd.Codigo, ct))
            return Result<Guid>.Failure("Ya existe una acción con ese código.");

        var result = Accion.Crear(cmd.Codigo, cmd.Nombre, cmd.Modulo, cmd.Descripcion);
        if (!result.IsSuccess)
            return Result<Guid>.Failure(result.Error!);

        await _accionRepo.AddAsync(result.Value!, ct);
        return Result<Guid>.Success(result.Value!.Id, "Acción creada correctamente.");
    }
}

// ─── Activar acción ───────────────────────────────────────────────────────────

public sealed record ActivarAccionCommand(Guid AccionId) : IRequest<Result>;

internal sealed class ActivarAccionCommandHandler(
    IAccionRepository _accionRepo) : IRequestHandler<ActivarAccionCommand, Result>
{
    public async Task<Result> Handle(ActivarAccionCommand cmd, CancellationToken ct)
    {
        var accion = await _accionRepo.GetByIdAsync(cmd.AccionId, ct);
        if (accion is null)
            return Result.Failure("La acción no fue encontrada.");

        var result = accion.Activar();
        if (!result.IsSuccess) return result;

        await _accionRepo.UpdateAsync(accion, ct);
        return Result.Success();
    }
}

// ─── Desactivar acción ────────────────────────────────────────────────────────

public sealed record DesactivarAccionCommand(Guid AccionId) : IRequest<Result>;

internal sealed class DesactivarAccionCommandHandler(
    IAccionRepository _accionRepo) : IRequestHandler<DesactivarAccionCommand, Result>
{
    public async Task<Result> Handle(DesactivarAccionCommand cmd, CancellationToken ct)
    {
        var accion = await _accionRepo.GetByIdAsync(cmd.AccionId, ct);
        if (accion is null)
            return Result.Failure("La acción no fue encontrada.");

        var result = accion.Desactivar();
        if (!result.IsSuccess) return result;

        await _accionRepo.UpdateAsync(accion, ct);
        return Result.Success();
    }
}
