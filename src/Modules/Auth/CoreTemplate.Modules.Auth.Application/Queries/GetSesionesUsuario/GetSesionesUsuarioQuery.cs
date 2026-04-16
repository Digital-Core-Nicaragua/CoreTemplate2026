using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetSesionesUsuario;

public sealed record GetSesionesUsuarioQuery(Guid UsuarioId) : IRequest<Result<List<SesionDto>>>;

internal sealed class GetSesionesUsuarioQueryHandler(
    ISesionRepository _sesionRepo,
    IUsuarioRepository _usuarioRepo) : IRequestHandler<GetSesionesUsuarioQuery, Result<List<SesionDto>>>
{
    public async Task<Result<List<SesionDto>>> Handle(GetSesionesUsuarioQuery query, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(query.UsuarioId, ct);
        if (usuario is null)
        {
            return Result<List<SesionDto>>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var sesiones = await _sesionRepo.GetActivasByUsuarioAsync(query.UsuarioId, ct);

        var dtos = sesiones.Select(s => new SesionDto(
            s.Id, s.Canal, s.Dispositivo, s.Ip, s.UserAgent,
            s.UltimaActividad, s.ExpiraEn, s.CreadoEn)).ToList();

        return Result<List<SesionDto>>.Success(dtos);
    }
}
