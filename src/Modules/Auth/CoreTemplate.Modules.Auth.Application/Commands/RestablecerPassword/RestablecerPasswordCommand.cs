using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.RestablecerPassword;

// ─── Solicitar restablecimiento ───────────────────────────────────────────────

public sealed record SolicitarRestablecimientoCommand(string Email) : IRequest<Result>;

internal sealed class SolicitarRestablecimientoCommandValidator : AbstractValidator<SolicitarRestablecimientoCommand>
{
    public SolicitarRestablecimientoCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("El email no es válido.");
    }
}

internal sealed class SolicitarRestablecimientoCommandHandler(
    IUsuarioRepository _usuarioRepo,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<SolicitarRestablecimientoCommand, Result>
{
    public async Task<Result> Handle(SolicitarRestablecimientoCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByEmailAsync(cmd.Email, ct: ct);

        // Siempre retornar éxito — no revelar si el email existe
        if (usuario is null)
        {
            return Result.Success();
        }

        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        usuario.AgregarTokenRestablecimiento(token, _authSettings.Value.PasswordResetTokenExpirationHours);
        await _usuarioRepo.UpdateAsync(usuario, ct);

        // El evento RestablecimientoSolicitadoEvent contiene el token
        // El sistema implementador escucha ese evento y envía el email

        return Result.Success();
    }
}

// ─── Restablecer contraseña ───────────────────────────────────────────────────

public sealed record RestablecerPasswordCommand(
    string Token,
    string NuevoPassword,
    string ConfirmPassword,
    string Ip,
    string UserAgent) : IRequest<Result>;

internal sealed class RestablecerPasswordCommandValidator : AbstractValidator<RestablecerPasswordCommand>
{
    public RestablecerPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("El token es requerido.");
        RuleFor(x => x.NuevoPassword).NotEmpty().MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NuevoPassword)
            .WithMessage(AuthErrorMessages.PasswordsNoCoinciden);
    }
}

internal sealed class RestablecerPasswordCommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISesionRepository _sesionRepo,
    IRegistroAuditoriaRepository _auditoriaRepo,
    IPasswordService _passwordService) : IRequestHandler<RestablecerPasswordCommand, Result>
{
    public async Task<Result> Handle(RestablecerPasswordCommand cmd, CancellationToken ct)
    {
        // Buscar usuario que tenga este token válido
        // El repositorio busca por token en los TokensRestablecimiento
        var usuario = await _usuarioRepo.GetByTokenRestablecimientoAsync(cmd.Token, ct);
        if (usuario is null)
        {
            return Result.Failure(AuthErrorMessages.TokenRestablecimientoInvalido);
        }

        var erroresPolicy = _passwordService.ValidarPolitica(cmd.NuevoPassword);
        if (erroresPolicy.Count > 0)
        {
            return Result.Failure(string.Join(" ", erroresPolicy));
        }

        if (!usuario.UsarTokenRestablecimiento(cmd.Token))
        {
            return Result.Failure(AuthErrorMessages.TokenRestablecimientoInvalido);
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

        await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
            usuario.TenantId, usuario.Id, usuario.Email.Valor,
            EventoAuditoria.RestablecimientoCompletado, cmd.Ip, cmd.UserAgent), ct);

        return Result.Success();
    }
}
