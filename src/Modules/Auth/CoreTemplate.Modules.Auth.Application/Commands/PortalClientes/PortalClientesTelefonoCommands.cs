using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.PortalClientes;

// ─── Commands ─────────────────────────────────────────────────────────────────

/// <summary>
/// Registra un nuevo cliente usando su número de teléfono.
/// Genera un OTP y lo envía por WhatsApp o SMS.
/// Requiere CustomerPortalSettings:RegistroPorTelefono:Enabled = true.
/// </summary>
public sealed record RegistrarClientePorTelefonoCommand(
    string Telefono,
    string Nombre,
    string Apellido,
    Guid? TenantId) : IRequest<Result<Guid>>;

/// <summary>
/// Verifica el OTP recibido por WhatsApp/SMS y activa el cliente.
/// Retorna los tokens de acceso si el OTP es válido.
/// </summary>
public sealed record VerificarOtpTelefonoCommand(
    string Telefono,
    string Codigo,
    string Ip,
    string UserAgent,
    Guid? TenantId) : IRequest<Result<LoginClienteResponseDto>>;

/// <summary>
/// Inicia el login de un cliente registrado por teléfono.
/// Genera un nuevo OTP y lo envía por WhatsApp/SMS.
/// </summary>
public sealed record SolicitarOtpLoginTelefonoCommand(
    string Telefono,
    Guid? TenantId) : IRequest<Result>;

// ─── Validators ───────────────────────────────────────────────────────────────

internal sealed class RegistrarClientePorTelefonoCommandValidator
    : AbstractValidator<RegistrarClientePorTelefonoCommand>
{
    public RegistrarClientePorTelefonoCommandValidator()
    {
        RuleFor(x => x.Telefono).NotEmpty().Matches(@"^\+[1-9]\d{6,14}$")
            .WithMessage("El teléfono debe estar en formato E.164 (ej: +521234567890).");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
    }
}

internal sealed class VerificarOtpTelefonoCommandValidator
    : AbstractValidator<VerificarOtpTelefonoCommand>
{
    public VerificarOtpTelefonoCommandValidator()
    {
        RuleFor(x => x.Telefono).NotEmpty();
        RuleFor(x => x.Codigo).NotEmpty().Length(6).WithMessage("El código debe tener 6 dígitos.");
    }
}

// ─── Handlers ─────────────────────────────────────────────────────────────────

/// <summary>
/// Registra un cliente por teléfono y envía el OTP.
/// Si el teléfono ya existe, reenvía el OTP (idempotente para UX).
/// </summary>
internal sealed class RegistrarClientePorTelefonoCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    INotificacionClienteService _notificacion,
    IOptions<CustomerPortalSettings> _portalSettings)
    : IRequestHandler<RegistrarClientePorTelefonoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegistrarClientePorTelefonoCommand cmd, CancellationToken ct)
    {
        var settings = _portalSettings.Value;

        if (!settings.EnableCustomerPortal)
            return Result<Guid>.Failure("El portal de clientes no está habilitado.");

        if (!settings.RegistroPorTelefono.Enabled)
            return Result<Guid>.Failure("El registro por teléfono no está habilitado.");

        if (!settings.RegistroHabilitado)
            return Result<Guid>.Failure(PortalErrorMessages.RegistroCerrado);

        var telefono = cmd.Telefono.Trim();

        // Si ya existe, reenviar OTP (no crear duplicado)
        var existente = await _clienteRepo.GetByTelefonoAsync(telefono, cmd.TenantId, ct);
        if (existente is not null)
        {
            var reenvioResult = existente.GenerarOtpTelefono(settings.RegistroPorTelefono.OtpExpirationMinutes);
            if (!reenvioResult.IsSuccess) return Result<Guid>.Failure(reenvioResult.Error!);

            await _clienteRepo.UpdateAsync(existente, ct);
            await EnviarOtp(existente.CodigoVerificacionTelefono!, telefono, settings, ct);
            return Result<Guid>.Success(existente.Id, "Código reenviado al teléfono registrado.");
        }

        var clienteResult = UsuarioCliente.CrearPorTelefono(telefono, cmd.Nombre, cmd.Apellido, cmd.TenantId);
        if (!clienteResult.IsSuccess) return Result<Guid>.Failure(clienteResult.Error!);

        var cliente = clienteResult.Value!;
        var otpResult = cliente.GenerarOtpTelefono(settings.RegistroPorTelefono.OtpExpirationMinutes);
        if (!otpResult.IsSuccess) return Result<Guid>.Failure(otpResult.Error!);

        await _clienteRepo.AddAsync(cliente, ct);
        await EnviarOtp(cliente.CodigoVerificacionTelefono!, telefono, settings, ct);

        return Result<Guid>.Success(cliente.Id, "Código enviado al teléfono. Verifica para activar tu cuenta.");
    }

    private async Task EnviarOtp(string codigo, string telefono, CustomerPortalSettings settings, CancellationToken ct)
    {
        if (settings.RegistroPorTelefono.Proveedor == "SMS")
            await _notificacion.EnviarOtpSmsAsync(telefono, codigo, ct);
        else
            await _notificacion.EnviarOtpWhatsAppAsync(telefono, codigo, ct);
    }
}

/// <summary>
/// Verifica el OTP de teléfono. Si es válido, activa el cliente y retorna los tokens.
/// Funciona tanto para registro como para login por teléfono.
/// </summary>
internal sealed class VerificarOtpTelefonoCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IJwtService _jwtService,
    ISesionRepository _sesionRepo,
    IOptions<CustomerPortalSettings> _portalSettings,
    IOptions<AuthSettings> _authSettings)
    : IRequestHandler<VerificarOtpTelefonoCommand, Result<LoginClienteResponseDto>>
{
    public async Task<Result<LoginClienteResponseDto>> Handle(VerificarOtpTelefonoCommand cmd, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableCustomerPortal)
            return Result<LoginClienteResponseDto>.Failure("El portal de clientes no está habilitado.");

        var cliente = await _clienteRepo.GetByTelefonoAsync(cmd.Telefono.Trim(), cmd.TenantId, ct);
        if (cliente is null)
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.ClienteNoEncontrado);

        if (cliente.Estado == EstadoUsuarioCliente.Blocked)
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CuentaBloqueada);

        var result = cliente.VerificarOtpTelefono(cmd.Codigo);
        if (!result.IsSuccess) return Result<LoginClienteResponseDto>.Failure(result.Error!);

        var refreshToken = _jwtService.GenerarRefreshToken();
        var refreshTokenHash = ComputarHash(refreshToken);

        var sesion = Sesion.Crear(
            cliente.Id,
            cliente.TenantId,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_authSettings.Value.RefreshTokenExpirationDays),
            CanalAcceso.Api,
            cmd.Ip,
            cmd.UserAgent);

        await _sesionRepo.AddAsync(sesion, ct);
        await _clienteRepo.UpdateAsync(cliente, ct);

        var accessToken = _jwtService.GenerarAccessTokenCliente(cliente);
        var expiraEn = _jwtService.ObtenerExpiracionAccessToken();

        return Result<LoginClienteResponseDto>.Success(
            new LoginClienteResponseDto(accessToken, refreshToken, expiraEn, MapearDto(cliente)),
            "Teléfono verificado. Sesión iniciada.");
    }

    private static ClienteDto MapearDto(UsuarioCliente c) => new(
        c.Id, c.Email ?? string.Empty, c.Nombre, c.Apellido, c.Telefono,
        c.Estado, c.EmailVerificado, c.TelefonoVerificado,
        c.Proveedores.Select(p => p.Proveedor.ToString()).ToList(),
        c.TenantId, c.CreadoEn);

    private static string ComputarHash(string valor)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

/// <summary>
/// Genera y envía un nuevo OTP para login de cliente ya registrado por teléfono.
/// </summary>
internal sealed class SolicitarOtpLoginTelefonoCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    INotificacionClienteService _notificacion,
    IOptions<CustomerPortalSettings> _portalSettings)
    : IRequestHandler<SolicitarOtpLoginTelefonoCommand, Result>
{
    public async Task<Result> Handle(SolicitarOtpLoginTelefonoCommand cmd, CancellationToken ct)
    {
        var settings = _portalSettings.Value;

        if (!settings.EnableCustomerPortal)
            return Result.Failure("El portal de clientes no está habilitado.");

        if (!settings.RegistroPorTelefono.Enabled)
            return Result.Failure("El login por teléfono no está habilitado.");

        // Siempre retorna éxito para no revelar si el teléfono existe
        var cliente = await _clienteRepo.GetByTelefonoAsync(cmd.Telefono.Trim(), cmd.TenantId, ct);
        if (cliente is null) return Result.Success();

        if (!cliente.PuedeAutenticarse()) return Result.Success();

        var otpResult = cliente.GenerarOtpTelefono(settings.RegistroPorTelefono.OtpExpirationMinutes);
        if (!otpResult.IsSuccess) return Result.Success();

        await _clienteRepo.UpdateAsync(cliente, ct);

        if (settings.RegistroPorTelefono.Proveedor == "SMS")
            await _notificacion.EnviarOtpSmsAsync(cmd.Telefono, cliente.CodigoVerificacionTelefono!, ct);
        else
            await _notificacion.EnviarOtpWhatsAppAsync(cmd.Telefono, cliente.CodigoVerificacionTelefono!, ct);

        return Result.Success();
    }
}
