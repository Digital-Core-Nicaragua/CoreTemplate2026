namespace CoreTemplate.Modules.Auth.Application.DTOs;

/// <summary>Detalle de una sucursal.</summary>
public sealed record SucursalDto(
    Guid Id,
    string Codigo,
    string Nombre,
    bool EsActiva);

/// <summary>Sucursal asignada a un usuario con flag de principal.</summary>
public sealed record UsuarioSucursalDto(
    Guid SucursalId,
    string Codigo,
    string Nombre,
    bool EsPrincipal);
