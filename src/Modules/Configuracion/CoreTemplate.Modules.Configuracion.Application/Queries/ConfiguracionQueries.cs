using CoreTemplate.Modules.Configuracion.Application.Commands;
using CoreTemplate.Modules.Configuracion.Application.DTOs;
using CoreTemplate.Modules.Configuracion.Domain.Repositories;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;

namespace CoreTemplate.Modules.Configuracion.Application.Queries;

// ─── Listar todos agrupados ───────────────────────────────────────────────────

public record GetConfiguracionQuery : IRequest<Result<IReadOnlyList<ConfiguracionGrupoDto>>>;

internal sealed class GetConfiguracionHandler(
    IConfiguracionItemRepository repo,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetConfiguracionQuery, Result<IReadOnlyList<ConfiguracionGrupoDto>>>
{
    public async Task<Result<IReadOnlyList<ConfiguracionGrupoDto>>> Handle(
        GetConfiguracionQuery q, CancellationToken ct)
    {
        var items = await repo.ListarAsync(null, currentTenant.TenantId, ct);
        var grupos = items
            .GroupBy(i => i.Grupo)
            .OrderBy(g => g.Key)
            .Select(g => new ConfiguracionGrupoDto(g.Key, g.Select(i => i.ToDto()).ToList()))
            .ToList();

        return Result<IReadOnlyList<ConfiguracionGrupoDto>>.Success(grupos);
    }
}

// ─── Por grupo ────────────────────────────────────────────────────────────────

public record GetConfiguracionPorGrupoQuery(string Grupo)
    : IRequest<Result<ConfiguracionGrupoDto>>;

internal sealed class GetConfiguracionPorGrupoHandler(
    IConfiguracionItemRepository repo,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetConfiguracionPorGrupoQuery, Result<ConfiguracionGrupoDto>>
{
    public async Task<Result<ConfiguracionGrupoDto>> Handle(
        GetConfiguracionPorGrupoQuery q, CancellationToken ct)
    {
        var items = await repo.ListarAsync(q.Grupo, currentTenant.TenantId, ct);
        return Result<ConfiguracionGrupoDto>.Success(
            new ConfiguracionGrupoDto(q.Grupo, items.Select(i => i.ToDto()).ToList()));
    }
}

// ─── Por clave ────────────────────────────────────────────────────────────────

public record GetConfiguracionPorClaveQuery(string Clave)
    : IRequest<Result<ConfiguracionItemDto>>;

internal sealed class GetConfiguracionPorClaveHandler(
    IConfiguracionItemRepository repo,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetConfiguracionPorClaveQuery, Result<ConfiguracionItemDto>>
{
    public async Task<Result<ConfiguracionItemDto>> Handle(
        GetConfiguracionPorClaveQuery q, CancellationToken ct)
    {
        var item = await repo.ObtenerPorClaveAsync(q.Clave, currentTenant.TenantId, ct)
                ?? await repo.ObtenerPorClaveAsync(q.Clave, null, ct);

        return item is null
            ? Result<ConfiguracionItemDto>.Failure($"Parámetro '{q.Clave}' no encontrado.")
            : Result<ConfiguracionItemDto>.Success(item.ToDto());
    }
}
