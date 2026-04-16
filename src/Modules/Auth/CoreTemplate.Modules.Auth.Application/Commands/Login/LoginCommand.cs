using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string Ip,
    string UserAgent,
    CanalAcceso Canal = CanalAcceso.Web) : IRequest<Result<object>>;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("El email no es válido.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es requerida.");
    }
}

internal sealed class LoginCommandHandler(
    IUsuarioRepository _usuarioRepo,
    IRegistroAuditoriaRepository _auditoriaRepo,
    ISesionRepository _sesionRepo,
    IPasswordService _passwordService,
    IJwtService _jwtService,
    ISesionService _sesionService,
    IOptions<LockoutSettings> _lockout,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<LoginCommand, Result<object>>
{
    public async Task<Result<object>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByEmailAsync(cmd.Email, ct: ct);

        if (usuario is null)
        {
            await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
                null, null, cmd.Email, EventoAuditoria.LoginFallido, cmd.Ip, cmd.UserAgent, "Email no encontrado"), ct);
            return Result<object>.Failure(AuthErrorMessages.CredencialesInvalidas);
        }

        if (!usuario.PuedeAutenticarse())
        {
            await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
                usuario.TenantId, usuario.Id, usuario.Email.Valor,
                EventoAuditoria.LoginFallido, cmd.Ip, cmd.UserAgent, $"Estado: {usuario.Estado}"), ct);

            return usuario.Estado == EstadoUsuario.Bloqueado
                ? Result<object>.Failure(AuthErrorMessages.CuentaBloqueada)
                : Result<object>.Failure(AuthErrorMessages.CuentaInactiva);
        }

        if (!_passwordService.VerifyPassword(cmd.Password, usuario.PasswordHash.Valor))
        {
            // Sistema/Integracion no se bloquean por intentos fallidos
            if (usuario.TipoUsuario == TipoUsuario.Humano)
            {
                var lockout = _lockout.Value;
                usuario.IncrementarIntentosFallidos(lockout.MaxFailedAttempts, lockout.LockoutDurationMinutes);
                await _usuarioRepo.UpdateAsync(usuario, ct);
            }

            await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
                usuario.TenantId, usuario.Id, usuario.Email.Valor,
                EventoAuditoria.LoginFallido, cmd.Ip, cmd.UserAgent,
                $"Intento {usuario.IntentosFallidos}/{_lockout.Value.MaxFailedAttempts}"), ct);

            return Result<object>.Failure(AuthErrorMessages.CredencialesInvalidas);
        }

        usuario.ResetearIntentosFallidos();
        usuario.RegistrarAcceso();

        // 2FA solo aplica a usuarios Humano
        if (usuario.TwoFactorActivo && usuario.TipoUsuario == TipoUsuario.Humano)
        {
            await _usuarioRepo.UpdateAsync(usuario, ct);
            var tokenTemporal = _jwtService.GenerarTokenTemporal2FA(usuario.Id);
            return Result<object>.Success(
                new Login2FARequeridoDto(tokenTemporal, true),
                AuthSuccessMessages.LoginExitoso);
        }

        // Verificar límite de sesiones (no aplica a Sistema/Integracion)
        var puedeCrearSesion = await _sesionService.VerificarYAplicarLimiteAsync(
            usuario.Id, usuario.TipoUsuario, ct);

        if (!puedeCrearSesion)
        {
            return Result<object>.Failure(AuthErrorMessages.LimiteSesionesAlcanzado);
        }

        // Crear sesión
        var refreshToken = _jwtService.GenerarRefreshToken();
        var refreshTokenHash = ComputarHash(refreshToken);
        var expiraEn = DateTime.UtcNow.AddDays(_authSettings.Value.RefreshTokenExpirationDays);

        var sesion = Sesion.Crear(
            usuario.Id,
            usuario.TenantId,
            refreshTokenHash,
            expiraEn,
            cmd.Canal,
            cmd.Ip,
            cmd.UserAgent);

        await _sesionRepo.AddAsync(sesion, ct);
        await _usuarioRepo.UpdateAsync(usuario, ct);

        var accessToken = _jwtService.GenerarAccessToken(usuario);
        var accessTokenExpiraEn = _jwtService.ObtenerExpiracionAccessToken();

        await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
            usuario.TenantId, usuario.Id, usuario.Email.Valor,
            EventoAuditoria.Login, cmd.Ip, cmd.UserAgent), ct);

        var usuarioDto = MapearUsuarioDto(usuario);
        return Result<object>.Success(
            new LoginResponseDto(accessToken, refreshToken, accessTokenExpiraEn, usuarioDto),
            AuthSuccessMessages.LoginExitoso);
    }

    private static string ComputarHash(string valor)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static UsuarioDto MapearUsuarioDto(Usuario u) => new(
        u.Id, u.TenantId, u.Email.Valor, u.Nombre, u.Estado,
        u.TwoFactorActivo, u.UltimoAcceso, u.CreadoEn,
        u.Roles.Select(r => r.RolId.ToString()).ToList());
}
