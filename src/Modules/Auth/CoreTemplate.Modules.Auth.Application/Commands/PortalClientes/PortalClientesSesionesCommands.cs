using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.PortalClientes;

// ─── Query: mis sesiones ──────────────────────────────────────────────────────

/// <summary>Obtiene las sesiones activas del cliente autenticado.</summary>
public sealed record GetMisSesionesClienteQuery(Guid ClienteId) : IRequest<Result<List<SesionDto>>>;

internal sealed class GetMisSesionesClienteQueryHandler(
    ISesionRepository _sesionRepo,
    IOptions<CustomerPortalSettings> _portalSettings)
    : IRequestHandler<GetMisSesionesClienteQuery, Result<List<SesionDto>>>
{
    public async Task<Result<List<SesionDto>>> Handle(GetMisSesionesClienteQuery query, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableSessionManagement)
            return Result<List<SesionDto>>.Failure("La gestión de sesiones no está habilitada en el portal.");

        var sesiones = await _sesionRepo.GetActivasByUsuarioAsync(query.ClienteId, ct);

        var dtos = sesiones.Select(s => new SesionDto(
            s.Id, s.Canal, s.Dispositivo, s.Ip, s.UserAgent,
            s.UltimaActividad, s.ExpiraEn, s.CreadoEn)).ToList();

        return Result<List<SesionDto>>.Success(dtos);
    }
}

// ─── Command: cerrar sesión específica ───────────────────────────────────────

/// <summary>Cierra una sesión específica del cliente autenticado.</summary>
public sealed record CerrarSesionClienteCommand(Guid ClienteId, Guid SesionId) : IRequest<Result>;

internal sealed class CerrarSesionClienteCommandHandler(
    ISesionRepository _sesionRepo,
    IOptions<CustomerPortalSettings> _portalSettings)
    : IRequestHandler<CerrarSesionClienteCommand, Result>
{
    public async Task<Result> Handle(CerrarSesionClienteCommand cmd, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableSessionManagement)
            return Result.Failure("La gestión de sesiones no está habilitada en el portal.");

        var sesion = await _sesionRepo.GetByIdAsync(cmd.SesionId, ct);

        // Verificar que la sesión pertenece al cliente autenticado
        if (sesion is null || sesion.UsuarioId != cmd.ClienteId)
            return Result.Failure(PortalErrorMessages.SesionNoEncontrada);

        sesion.Revocar();
        await _sesionRepo.UpdateAsync(sesion, ct);
        return Result.Success();
    }
}

// ─── Command: cerrar otras sesiones ──────────────────────────────────────────

/// <summary>Cierra todas las sesiones del cliente excepto la que corresponde al AccessToken actual.</summary>
public sealed record CerrarOtrasSesionesClienteCommand(Guid ClienteId, string AccessToken) : IRequest<Result>;

internal sealed class CerrarOtrasSesionesClienteCommandHandler(
    ISesionRepository _sesionRepo,
    IJwtService _jwtService,
    IOptions<CustomerPortalSettings> _portalSettings)
    : IRequestHandler<CerrarOtrasSesionesClienteCommand, Result>
{
    public async Task<Result> Handle(CerrarOtrasSesionesClienteCommand cmd, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableSessionManagement)
            return Result.Failure("La gestión de sesiones no está habilitada en el portal.");

        // Extraer el JTI del token actual para identificar la sesión activa
        var jtiActual = _jwtService.ExtraerJti(cmd.AccessToken);

        var sesiones = await _sesionRepo.GetActivasByUsuarioAsync(cmd.ClienteId, ct);

        foreach (var sesion in sesiones)
        {
            // No revocar la sesión actual — identificada por el JTI del AccessToken
            if (jtiActual is not null && sesion.Id.ToString() == jtiActual)
                continue;

            sesion.Revocar();
            await _sesionRepo.UpdateAsync(sesion, ct);
        }

        return Result.Success();
    }
}
