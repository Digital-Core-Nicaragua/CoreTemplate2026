using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.CambiarPassword;

public sealed record CambiarPasswordCommand(
    string PasswordActual,
    string NuevoPassword,
    string ConfirmPassword,
    string Ip,
    string UserAgent,
    string AccessToken = "") : IRequest<Result>;

internal sealed class CambiarPasswordCommandValidator : AbstractValidator<CambiarPasswordCommand>
{
    public CambiarPasswordCommandValidator()
    {
        RuleFor(x => x.PasswordActual).NotEmpty().WithMessage("La contraseña actual es requerida.");
        RuleFor(x => x.NuevoPassword).NotEmpty().MinimumLength(8)
            .WithMessage("La nueva contraseña debe tener al menos 8 caracteres.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NuevoPassword)
            .WithMessage(AuthErrorMessages.PasswordsNoCoinciden);
    }
}

internal sealed class CambiarPasswordCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISesionRepository _sesionRepo,
    IRegistroAuditoriaRepository _auditoriaRepo,
    IPasswordService _passwordService,
    ITokenBlacklistService _blacklist,
    IJwtService _jwtService,
    ICurrentUser _currentUser,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<CambiarPasswordCommand, Result>
{
    public async Task<Result> Handle(CambiarPasswordCommand cmd, CancellationToken ct)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(_currentUser.Id.Value, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        if (!_passwordService.VerifyPassword(cmd.PasswordActual, usuario.PasswordHash.Valor))
        {
            return Result.Failure(AuthErrorMessages.PasswordActualIncorrecto);
        }

        var erroresPolicy = _passwordService.ValidarPolitica(cmd.NuevoPassword);
        if (erroresPolicy.Count > 0)
        {
            return Result.Failure(string.Join(" ", erroresPolicy));
        }

        var nuevoHash = _passwordService.HashPassword(cmd.NuevoPassword);
        var passwordHashResult = PasswordHash.Crear(nuevoHash);
        if (!passwordHashResult.IsSuccess)
        {
            return Result.Failure(passwordHashResult.Error!);
        }

        usuario.CambiarPassword(passwordHashResult.Value!);
        await _sesionRepo.RevocarTodasAsync(usuario.Id, ct);
        await _usuarioRepo.UpdateAsync(usuario, ct);

        // Agregar AccessToken actual a blacklist
        if (_authSettings.Value.EnableTokenBlacklist && !string.IsNullOrEmpty(cmd.AccessToken))
        {
            var jti = _jwtService.ExtraerJti(cmd.AccessToken);
            var expira = _jwtService.ExtraerExpiracion(cmd.AccessToken);
            if (jti is not null && expira.HasValue)
            {
                var ttl = expira.Value - DateTime.UtcNow;
                if (ttl > TimeSpan.Zero)
                    await _blacklist.AgregarAsync(jti, ttl, ct);
            }
        }

        await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
            usuario.TenantId, usuario.Id, usuario.Email.Valor,
            EventoAuditoria.CambioPassword, cmd.Ip, cmd.UserAgent), ct);

        return Result.Success();
    }
}
