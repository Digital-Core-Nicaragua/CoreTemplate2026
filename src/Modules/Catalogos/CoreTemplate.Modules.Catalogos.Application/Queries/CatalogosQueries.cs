using CoreTemplate.Modules.Catalogos.Application.Constants;
using CoreTemplate.Modules.Catalogos.Application.DTOs;
using CoreTemplate.Modules.Catalogos.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Catalogos.Application.Queries;

// ─── GetItems (paginado) ──────────────────────────────────────────────────────

public sealed record GetCatalogoItemsQuery(
    int Pagina = 1,
    int TamanoPagina = 20,
    bool? SoloActivos = null,
    string? Busqueda = null) : IRequest<Result<PagedResult<CatalogoItemResumenDto>>>;

internal sealed class GetCatalogoItemsQueryHandler(
    ICatalogoItemRepository _repo) : IRequestHandler<GetCatalogoItemsQuery, Result<PagedResult<CatalogoItemResumenDto>>>
{
    public async Task<Result<PagedResult<CatalogoItemResumenDto>>> Handle(
        GetCatalogoItemsQuery query, CancellationToken ct)
    {
        var paged = await _repo.GetPagedAsync(
            query.Pagina, query.TamanoPagina, query.SoloActivos, query.Busqueda, ct);

        var items = paged.Items
            .Select(i => new CatalogoItemResumenDto(i.Id, i.Codigo, i.Nombre, i.EsActivo))
            .ToList();

        return Result<PagedResult<CatalogoItemResumenDto>>.Success(
            new PagedResult<CatalogoItemResumenDto>(items, paged.Pagina, paged.TamanoPagina, paged.Total));
    }
}

// ─── GetItemById ──────────────────────────────────────────────────────────────

public sealed record GetCatalogoItemByIdQuery(Guid ItemId) : IRequest<Result<CatalogoItemDto>>;

internal sealed class GetCatalogoItemByIdQueryHandler(
    ICatalogoItemRepository _repo) : IRequestHandler<GetCatalogoItemByIdQuery, Result<CatalogoItemDto>>
{
    public async Task<Result<CatalogoItemDto>> Handle(
        GetCatalogoItemByIdQuery query, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(query.ItemId, ct);
        if (item is null)
        {
            return Result<CatalogoItemDto>.Failure(CatalogosErrorMessages.ItemNoEncontrado);
        }

        return Result<CatalogoItemDto>.Success(new CatalogoItemDto(
            item.Id, item.TenantId, item.Codigo, item.Nombre,
            item.Descripcion, item.EsActivo, item.CreadoEn, item.ModificadoEn));
    }
}
