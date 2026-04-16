using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string Token,
    string Ip) : IRequest<Result<TokenResponseDto>>;

internal sealed class RefreshTokenCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISesionRepository _sesionRepo,
    IJwtService _jwtService,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<RefreshTokenCommand, Result<TokenResponseDto>>
{
    public async Task<Result<TokenResponseDto>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var tokenHash = ComputarHash(cmd.Token);
        var sesion = await _sesionRepo.GetActivaByRefreshTokenHashAsync(tokenHash, ct);

        if (sesion is null || !sesion.EsValida)
        {
            return Result<TokenResponseDto>.Failure(AuthErrorMessages.RefreshTokenInvalido);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(sesion.UsuarioId, ct);
        if (usuario is null || !usuario.PuedeAutenticarse())
        {
            return Result<TokenResponseDto>.Failure(AuthErrorMessages.RefreshTokenInvalido);
        }

        var nuevoRefreshToken = _jwtService.GenerarRefreshToken();
        var nuevoHash = ComputarHash(nuevoRefreshToken);
        var nuevaExpiracion = DateTime.UtcNow.AddDays(_authSettings.Value.RefreshTokenExpirationDays);

        sesion.Renovar(nuevoHash, nuevaExpiracion);
        await _sesionRepo.UpdateAsync(sesion, ct);

        var nuevoAccessToken = _jwtService.GenerarAccessToken(usuario);
        var expiraEn = _jwtService.ObtenerExpiracionAccessToken();

        return Result<TokenResponseDto>.Success(
            new TokenResponseDto(nuevoAccessToken, nuevoRefreshToken, expiraEn),
            AuthSuccessMessages.TokenRefrescado);
    }

    private static string ComputarHash(string valor)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
