using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.Auth.Application.Queries.GetMisSesiones;

public sealed record GetMisSesionesQuery : IRequest<Result<List<SesionDto>>>;

internal sealed class GetMisSesionesQueryHandler(
    ISesionRepository _sesionRepo,
    ICurrentUser _currentUser) : IRequestHandler<GetMisSesionesQuery, Result<List<SesionDto>>>
{
    public async Task<Result<List<SesionDto>>> Handle(GetMisSesionesQuery query, CancellationToken ct)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<List<SesionDto>>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var sesiones = await _sesionRepo.GetActivasByUsuarioAsync(_currentUser.Id.Value, ct);

        var dtos = sesiones.Select(s => new SesionDto(
            s.Id, s.Canal, s.Dispositivo, s.Ip, s.UserAgent,
            s.UltimaActividad, s.ExpiraEn, s.CreadoEn)).ToList();

        return Result<List<SesionDto>>.Success(dtos);
    }
}
