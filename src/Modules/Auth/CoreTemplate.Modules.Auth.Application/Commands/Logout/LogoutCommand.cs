using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.Logout;

public sealed record LogoutCommand(
    string RefreshToken,
    string AccessToken,
    string Ip,
    string UserAgent) : IRequest<Result>;

internal sealed class LogoutCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISesionRepository _sesionRepo,
    IRegistroAuditoriaRepository _auditoriaRepo,
    ITokenBlacklistService _blacklist,
    IJwtService _jwtService,
    ICurrentUser _currentUser,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        // Revocar sesión
        var tokenHash = ComputarHash(cmd.RefreshToken);
        var sesion = await _sesionRepo.GetActivaByRefreshTokenHashAsync(tokenHash, ct);
        if (sesion is not null)
        {
            sesion.Revocar();
            await _sesionRepo.UpdateAsync(sesion, ct);
        }

        // Agregar AccessToken a blacklist
        if (_authSettings.Value.EnableTokenBlacklist && !string.IsNullOrEmpty(cmd.AccessToken))
        {
            var jti = _jwtService.ExtraerJti(cmd.AccessToken);
            var expira = _jwtService.ExtraerExpiracion(cmd.AccessToken);
            if (jti is not null && expira.HasValue)
            {
                var ttl = expira.Value - DateTime.UtcNow;
                if (ttl > TimeSpan.Zero)
                {
                    await _blacklist.AgregarAsync(jti, ttl, ct);
                }
            }
        }

        var usuario = await _usuarioRepo.GetByIdAsync(_currentUser.Id.Value, ct);
        if (usuario is not null)
        {
            await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
                usuario.TenantId, usuario.Id, usuario.Email.Valor,
                EventoAuditoria.Logout, cmd.Ip, cmd.UserAgent), ct);
        }

        return Result.Success();
    }

    private static string ComputarHash(string valor)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
