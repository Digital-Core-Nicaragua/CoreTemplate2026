namespace CoreTemplate.Modules.Auth.Application.DTOs;

/// <summary>Detalle de una acción del catálogo.</summary>
public sealed record AccionDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string Modulo,
    string Descripcion,
    bool EsActiva);
