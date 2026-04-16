namespace CoreTemplate.Modules.Catalogos.Application.DTOs;

/// <summary>Detalle completo de un ítem de catálogo.</summary>
public sealed record CatalogoItemDto(
    Guid Id,
    Guid? TenantId,
    string Codigo,
    string Nombre,
    string? Descripcion,
    bool EsActivo,
    DateTime CreadoEn,
    DateTime? ModificadoEn);

/// <summary>Resumen de ítem para listados paginados.</summary>
public sealed record CatalogoItemResumenDto(
    Guid Id,
    string Codigo,
    string Nombre,
    bool EsActivo);
