using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Catalogos.Domain.Events;

/// <summary>Se dispara cuando se crea un nuevo ítem de catálogo.</summary>
public record CatalogoItemCreadoEvent(Guid ItemId, string Codigo, string Nombre) : IDomainEvent;

/// <summary>Se dispara cuando un ítem de catálogo es activado.</summary>
public record CatalogoItemActivadoEvent(Guid ItemId, string Codigo) : IDomainEvent;

/// <summary>Se dispara cuando un ítem de catálogo es desactivado.</summary>
public record CatalogoItemDesactivadoEvent(Guid ItemId, string Codigo) : IDomainEvent;
