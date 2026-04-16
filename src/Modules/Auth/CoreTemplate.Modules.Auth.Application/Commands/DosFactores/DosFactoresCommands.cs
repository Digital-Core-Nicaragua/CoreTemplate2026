using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Infrastructure.Services;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.DosFactores;

// ─── Activar 2FA (genera QR y códigos de recuperación) ───────────────────────

public sealed record Activar2FACommand : IRequest<Result<Activar2FAResponseDto>>;

internal sealed class Activar2FACommandHandler(
    IUsuarioRepository _usuarioRepo,
    ITotpService _totpService,
    ICurrentUser _currentUser,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<Activar2FACommand, Result<Activar2FAResponseDto>>
{
    public async Task<Result<Activar2FAResponseDto>> Handle(Activar2FACommand cmd, CancellationToken ct)
    {
        if (!_authSettings.Value.TwoFactorEnabled)
        {
            return Result<Activar2FAResponseDto>.Failure(AuthErrorMessages.DosFactoresNoHabilitado);
        }

        if (!_currentUser.Id.HasValue)
        {
            return Result<Activar2FAResponseDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(_currentUser.Id.Value, ct);
        if (usuario is null)
        {
            return Result<Activar2FAResponseDto>.Failure(AuthErrorMessages.UsuarioNoEncontrado);
        }

        if (usuario.TwoFactorActivo)
        {
            return Result<Activar2FAResponseDto>.Failure(AuthErrorMessages.DosFactoresYaActivo);
        }

        var secretKey = _totpService.GenerarSecretKey();
        var qrUri = _totpService.GenerarQrCodeUri(usuario.Email.Valor, secretKey, _authSettings.Value.JwtIssuer);
        var codigosRecuperacion = _totpService.GenerarCodigosRecuperacion();
        var codigosHash = codigosRecuperacion.Select(c => _totpService.HashCodigoRecuperacion(c)).ToList();

        // Guardamos el secretKey temporalmente en el usuario (sin activar aún)
        // La activación se confirma en Confirmar2FA
        usuario.GuardarSecretKeyTemporal(secretKey);
        await _usuarioRepo.UpdateAsync(usuario, ct);

        return Result<Activar2FAResponseDto>.Success(
            new Activar2FAResponseDto(qrUri, secretKey, codigosRecuperacion),
            AuthSuccessMessages.DosFactoresActivado);
    }
}

// ─── Confirmar 2FA (verificar primer código TOTP y activar definitivamente) ───

public sealed record Confirmar2FACommand(string Codigo) : IRequest<Result>;

internal sealed class Confirmar2FACommandValidator : AbstractValidator<Confirmar2FACommand>
{
    public Confirmar2FACommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().Length(6).WithMessage("El código debe tener 6 dígitos.");
    }
}

internal sealed class Confirmar2FACommandHandler(
    IUsuarioRepository _usuarioRepo,
    ITotpService _totpService,
    ICurrentUser _currentUser) : IRequestHandler<Confirmar2FACommand, Result>
{
    public async Task<Result> Handle(Confirmar2FACommand cmd, CancellationToken ct)
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

        if (usuario.TwoFactorSecretKey is null)
        {
            return Result.Failure(AuthErrorMessages.DosFactoresNoActivo);
        }

        if (!_totpService.ValidarCodigo(usuario.TwoFactorSecretKey, cmd.Codigo))
        {
            return Result.Failure(AuthErrorMessages.CodigoTotpInvalido);
        }

        // Generar códigos de recuperación y activar definitivamente
        var codigosRecuperacion = _totpService.GenerarCodigosRecuperacion();
        var codigosHash = codigosRecuperacion.Select(c => _totpService.HashCodigoRecuperacion(c)).ToList();

        var result = usuario.ActivarDosFactores(usuario.TwoFactorSecretKey, codigosHash);
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}

// ─── Verificar 2FA en el flujo de login ──────────────────────────────────────

public sealed record Verificar2FACommand(
    string TokenTemporal,
    string Codigo,
    string Ip,
    string UserAgent) : IRequest<Result<LoginResponseDto>>;

internal sealed class Verificar2FACommandValidator : AbstractValidator<Verificar2FACommand>
{
    public Verificar2FACommandValidator()
    {
        RuleFor(x => x.TokenTemporal).NotEmpty();
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("El código es requerido.");
    }
}

internal sealed class Verificar2FACommandHandler(
    IUsuarioRepository _usuarioRepo,
    ISesionRepository _sesionRepo,
    IRegistroAuditoriaRepository _auditoriaRepo,
    IJwtService _jwtService,
    ITotpService _totpService,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<Verificar2FACommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(Verificar2FACommand cmd, CancellationToken ct)
    {
        var usuarioId = _jwtService.ValidarTokenTemporal2FA(cmd.TokenTemporal);
        if (!usuarioId.HasValue)
        {
            return Result<LoginResponseDto>.Failure(AuthErrorMessages.CredencialesInvalidas);
        }

        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId.Value, ct);
        if (usuario is null || !usuario.TwoFactorActivo || usuario.TwoFactorSecretKey is null)
        {
            return Result<LoginResponseDto>.Failure(AuthErrorMessages.CredencialesInvalidas);
        }

        // Intentar con código TOTP primero, luego con código de recuperación
        bool codigoValido = _totpService.ValidarCodigo(usuario.TwoFactorSecretKey, cmd.Codigo);

        if (!codigoValido)
        {
            // Intentar con código de recuperación
            var codigoHash = _totpService.HashCodigoRecuperacion(cmd.Codigo);
            codigoValido = usuario.UsarCodigoRecuperacion(codigoHash);

            if (!codigoValido)
            {
                await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
                    usuario.TenantId, usuario.Id, usuario.Email.Valor,
                    EventoAuditoria.DosFactoresFallido, cmd.Ip, cmd.UserAgent), ct);

                return Result<LoginResponseDto>.Failure(AuthErrorMessages.CodigoTotpInvalido);
            }
        }

        usuario.RegistrarAcceso();
        var accessToken = _jwtService.GenerarAccessToken(usuario);
        var refreshToken = _jwtService.GenerarRefreshToken();
        var expiraEn = _jwtService.ObtenerExpiracionAccessToken();

        var refreshTokenHash = ComputarHash(refreshToken);
        var sesion = Sesion.Crear(
            usuario.Id,
            usuario.TenantId,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_authSettings.Value.RefreshTokenExpirationDays),
            CanalAcceso.Web,
            cmd.Ip,
            cmd.UserAgent);

        await _sesionRepo.AddAsync(sesion, ct);
        await _usuarioRepo.UpdateAsync(usuario, ct);

        await _auditoriaRepo.AddAsync(Domain.Entities.RegistroAuditoria.Crear(
            usuario.TenantId, usuario.Id, usuario.Email.Valor,
            EventoAuditoria.DosFactoresVerificado, cmd.Ip, cmd.UserAgent), ct);

        var usuarioDto = new UsuarioDto(
            usuario.Id, usuario.TenantId, usuario.Email.Valor, usuario.Nombre,
            usuario.Estado, usuario.TwoFactorActivo, usuario.UltimoAcceso, usuario.CreadoEn,
            usuario.Roles.Select(r => r.RolId.ToString()).ToList());

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto(accessToken, refreshToken, expiraEn, usuarioDto),
            AuthSuccessMessages.DosFactoresVerificado);
    }

    private static string ComputarHash(string valor)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

// ─── Desactivar 2FA ───────────────────────────────────────────────────────────

public sealed record Desactivar2FACommand(string Codigo) : IRequest<Result>;

internal sealed class Desactivar2FACommandHandler(
    IUsuarioRepository _usuarioRepo,
    ITotpService _totpService,
    ICurrentUser _currentUser) : IRequestHandler<Desactivar2FACommand, Result>
{
    public async Task<Result> Handle(Desactivar2FACommand cmd, CancellationToken ct)
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

        if (!usuario.TwoFactorActivo || usuario.TwoFactorSecretKey is null)
        {
            return Result.Failure(AuthErrorMessages.DosFactoresNoActivo);
        }

        if (!_totpService.ValidarCodigo(usuario.TwoFactorSecretKey, cmd.Codigo))
        {
            return Result.Failure(AuthErrorMessages.CodigoTotpInvalido);
        }

        var result = usuario.DesactivarDosFactores();
        if (!result.IsSuccess)
        {
            return result;
        }

        await _usuarioRepo.UpdateAsync(usuario, ct);
        return Result.Success();
    }
}
