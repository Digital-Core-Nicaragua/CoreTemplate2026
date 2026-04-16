namespace CoreTemplate.Modules.Catalogos.Api.Contracts;

/// <summary>Request para crear un nuevo ítem de catálogo.</summary>
public sealed record CrearCatalogoItemRequest(
    string Codigo,
    string Nombre,
    string? Descripcion);
