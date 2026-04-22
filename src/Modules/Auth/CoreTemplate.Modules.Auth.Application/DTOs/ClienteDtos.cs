using CoreTemplate.Modules.Auth.Domain.Enums;

namespace CoreTemplate.Modules.Auth.Application.DTOs;

/// <summary>
/// Datos del cliente retornados tras login o consulta de perfil.
/// </summary>
public sealed record ClienteDto(
    Guid Id,
    string Email,
    string Nombre,
    string Apellido,
    string? Telefono,
    EstadoUsuarioCliente Estado,
    bool EmailVerificado,
    bool TelefonoVerificado,
    IReadOnlyList<string> Proveedores,
    Guid? TenantId,
    DateTime CreadoEn);

/// <summary>
/// Respuesta del login de cliente — incluye tokens y datos básicos del cliente.
/// </summary>
public sealed record LoginClienteResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiraEn,
    ClienteDto Cliente);

/// <summary>
/// Resumen del cliente para listados de admin.
/// </summary>
public sealed record ClienteResumenDto(
    Guid Id,
    string Email,
    string NombreCompleto,
    EstadoUsuarioCliente Estado,
    bool EmailVerificado,
    DateTime CreadoEn);
