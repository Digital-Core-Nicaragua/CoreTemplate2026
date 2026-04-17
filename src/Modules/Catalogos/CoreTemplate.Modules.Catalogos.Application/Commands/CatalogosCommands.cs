using CoreTemplate.Modules.Catalogos.Application.Constants;
using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using CoreTemplate.Modules.Catalogos.Domain.Repositories;
using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;

namespace CoreTemplate.Modules.Catalogos.Application.Commands;

// ─── Crear Item ───────────────────────────────────────────────────────────────

public sealed record CrearCatalogoItemCommand(
    string Codigo,
    string Nombre,
    string? Descripcion) : IRequest<Result<Guid>>;

internal sealed class CrearCatalogoItemCommandValidator : AbstractValidator<CrearCatalogoItemCommand>
{
    public CrearCatalogoItemCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50)
            .WithMessage("El código es requerido y no puede superar 50 caracteres.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200)
            .WithMessage("El nombre es requerido y no puede superar 200 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(500).When(x => x.Descripcion is not null)
            .WithMessage("La descripción no puede superar 500 caracteres.");
    }
}

internal sealed class CrearCatalogoItemCommandHandler(
    ICatalogoItemRepository _repo,
    ICurrentTenant _currentTenant) : IRequestHandler<CrearCatalogoItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearCatalogoItemCommand cmd, CancellationToken ct)
    {
        if (await _repo.ExistsByCodigoAsync(cmd.Codigo, _currentTenant.TenantId, ct))
        {
            return Result<Guid>.Failure(CatalogosErrorMessages.CodigoYaExiste);
        }

        var result = CatalogoItem.Crear(cmd.Codigo, cmd.Nombre, cmd.Descripcion, _currentTenant.TenantId);
        if (!result.IsSuccess)
        {
            return Result<Guid>.Failure(result.Error!);
        }

        await _repo.AddAsync(result.Value!, ct);
        return Result<Guid>.Success(result.Value!.Id, CatalogosSuccessMessages.ItemCreado);
    }
}

// ─── Activar Item ─────────────────────────────────────────────────────────────

public sealed record ActivarCatalogoItemCommand(Guid ItemId) : IRequest<Result>;

internal sealed class ActivarCatalogoItemCommandHandler(
    ICatalogoItemRepository _repo) : IRequestHandler<ActivarCatalogoItemCommand, Result>
{
    public async Task<Result> Handle(ActivarCatalogoItemCommand cmd, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(cmd.ItemId, ct);
        if (item is null)
        {
            return Result.Failure(CatalogosErrorMessages.ItemNoEncontrado);
        }

        var result = item.Activar();
        if (!result.IsSuccess)
        {
            return result;
        }

        await _repo.UpdateAsync(item, ct);
        return Result.Success();
    }
}

// ─── Desactivar Item ──────────────────────────────────────────────────────────

public sealed record DesactivarCatalogoItemCommand(Guid ItemId) : IRequest<Result>;

internal sealed class DesactivarCatalogoItemCommandHandler(
    ICatalogoItemRepository _repo) : IRequestHandler<DesactivarCatalogoItemCommand, Result>
{
    public async Task<Result> Handle(DesactivarCatalogoItemCommand cmd, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(cmd.ItemId, ct);
        if (item is null)
        {
            return Result.Failure(CatalogosErrorMessages.ItemNoEncontrado);
        }

        var result = item.Desactivar();
        if (!result.IsSuccess)
        {
            return result;
        }

        await _repo.UpdateAsync(item, ct);
        return Result.Success();
    }
}
