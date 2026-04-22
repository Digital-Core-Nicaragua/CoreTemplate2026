using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Constants;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Application.Commands.PortalClientes;

// --- Commands -----------------------------------------------------------------

/// <summary>Registra un nuevo cliente con email y contrase�a.</summary>
public sealed record RegistrarClienteCommand(
    string Email,
    string Password,
    string Nombre,
    string Apellido,
    string? Telefono,
    Guid? TenantId) : IRequest<Result<ClienteDto>>;

/// <summary>Login de cliente con email y contrase�a.</summary>
public sealed record LoginClienteCommand(
    string Email,
    string Password,
    string Ip,
    string UserAgent) : IRequest<Result<LoginClienteResponseDto>>;

/// <summary>Login o registro impl�cito de cliente mediante token OAuth externo.</summary>
public sealed record LoginClienteOAuthCommand(
    TipoProveedorOAuth Proveedor,
    string Token,
    string Ip,
    string UserAgent,
    Guid? TenantId) : IRequest<Result<LoginClienteResponseDto>>;

/// <summary>Verifica el email del cliente usando el token enviado por correo.</summary>
public sealed record VerificarEmailClienteCommand(
    Guid ClienteId,
    string Token) : IRequest<Result>;

/// <summary>Verifica el tel�fono del cliente usando el c�digo enviado por SMS.</summary>
public sealed record VerificarTelefonoClienteCommand(
    Guid ClienteId,
    string Codigo) : IRequest<Result>;

/// <summary>Regenera y reenv�a el token de verificaci�n de email.</summary>
public sealed record ReenviarVerificacionEmailCommand(
    Guid ClienteId) : IRequest<Result>;

/// <summary>Cambia la contrase�a del cliente autenticado.</summary>
public sealed record CambiarPasswordClienteCommand(
    Guid ClienteId,
    string PasswordActual,
    string NuevoPassword) : IRequest<Result>;

// --- Validators ---------------------------------------------------------------

internal sealed class RegistrarClienteCommandValidator : AbstractValidator<RegistrarClienteCommand>
{
    public RegistrarClienteCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("El email no es v�lido.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
    }
}

internal sealed class LoginClienteCommandValidator : AbstractValidator<LoginClienteCommand>
{
    public LoginClienteCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

// --- Handlers -----------------------------------------------------------------

/// <summary>
/// Registra un nuevo cliente con email/contrase�a.
/// Si RequireEmailVerification = true, genera el token de verificaci�n
/// y el cliente queda en estado Registered hasta confirmar su email.
/// </summary>
internal sealed class RegistrarClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IPasswordService _passwordService,
    IOptions<CustomerPortalSettings> _portalSettings) : IRequestHandler<RegistrarClienteCommand, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(RegistrarClienteCommand cmd, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableCustomerPortal)
            return Result<ClienteDto>.Failure("El portal de clientes no est� habilitado.");

        if (!_portalSettings.Value.RegistroHabilitado)
            return Result<ClienteDto>.Failure(PortalErrorMessages.RegistroCerrado);

        if (await _clienteRepo.ExistsByEmailAsync(cmd.Email, cmd.TenantId, ct))
            return Result<ClienteDto>.Failure(PortalErrorMessages.EmailYaRegistrado);

        var erroresPassword = _passwordService.ValidarPolitica(cmd.Password);
        if (erroresPassword.Count > 0)
            return Result<ClienteDto>.Failure(string.Join(" ", erroresPassword));

        var clienteResult = UsuarioCliente.Crear(
            cmd.Email,
            _passwordService.HashPassword(cmd.Password),
            cmd.Nombre,
            cmd.Apellido,
            cmd.TenantId,
            cmd.Telefono);

        if (!clienteResult.IsSuccess) return Result<ClienteDto>.Failure(clienteResult.Error!);

        var cliente = clienteResult.Value!;

        // Generar token de verificaci�n si el sistema lo requiere
        if (_portalSettings.Value.RequireEmailVerification)
            cliente.GenerarTokenVerificacionEmail();

        await _clienteRepo.AddAsync(cliente, ct);

        return Result<ClienteDto>.Success(MapearDto(cliente), PortalSuccessMessages.ClienteRegistrado);
    }

    private static ClienteDto MapearDto(UsuarioCliente c) => new(
        c.Id, c.Email ?? string.Empty, c.Nombre, c.Apellido, c.Telefono,
        c.Estado, c.EmailVerificado, c.TelefonoVerificado,
        c.Proveedores.Select(p => p.Proveedor.ToString()).ToList(),
        c.TenantId, c.CreadoEn);
}

/// <summary>
/// Autentica un cliente con email y contrase�a.
/// Aplica bloqueo por intentos fallidos seg�n LockoutSettings.
/// Crea una sesi�n reutilizando el aggregate Sesion del m�dulo Auth.
/// </summary>
internal sealed class LoginClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IPasswordService _passwordService,
    IJwtService _jwtService,
    ISesionRepository _sesionRepo,
    IOptions<CustomerPortalSettings> _portalSettings,
    IOptions<LockoutSettings> _lockout,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<LoginClienteCommand, Result<LoginClienteResponseDto>>
{
    public async Task<Result<LoginClienteResponseDto>> Handle(LoginClienteCommand cmd, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableCustomerPortal)
            return Result<LoginClienteResponseDto>.Failure("El portal de clientes no est� habilitado.");

        var cliente = await _clienteRepo.GetByEmailAsync(cmd.Email, ct: ct);
        if (cliente is null)
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CredencialesInvalidas);

        if (!cliente.PuedeAutenticarse())
        {
            return cliente.Estado == EstadoUsuarioCliente.Blocked
                ? Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CuentaBloqueada)
                : Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CuentaNoActiva);
        }

        // Clientes que solo tienen OAuth no tienen contrase�a local
        if (string.IsNullOrEmpty(cliente.PasswordHash))
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.SinPasswordLocal);

        if (!_passwordService.VerifyPassword(cmd.Password, cliente.PasswordHash))
        {
            var lockout = _lockout.Value;
            cliente.IncrementarIntentosFallidos(lockout.MaxFailedAttempts, lockout.LockoutDurationMinutes);
            await _clienteRepo.UpdateAsync(cliente, ct);
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CredencialesInvalidas);
        }

        cliente.ResetearIntentosFallidos();

        var refreshToken = _jwtService.GenerarRefreshToken();
        var refreshTokenHash = ComputarHash(refreshToken);

        // Reutilizamos Sesion del m�dulo Auth � el usuarioId apunta al cliente
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
        var accessTokenExpiraEn = _jwtService.ObtenerExpiracionAccessToken();

        return Result<LoginClienteResponseDto>.Success(
            new LoginClienteResponseDto(accessToken, refreshToken, accessTokenExpiraEn, MapearDto(cliente)),
            PortalSuccessMessages.LoginExitoso);
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
/// Autentica o registra un cliente mediante OAuth externo (Google, Facebook).
/// Flujo:
/// 1. Resuelve el servicio OAuth correcto seg�n el proveedor indicado.
/// 2. Valida el token con el proveedor externo.
/// 3. Busca cliente por externalId del proveedor.
/// 4. Si no existe, busca por email � puede que ya tenga cuenta con otro proveedor.
/// 5. Si tampoco existe, crea el cliente autom�ticamente (registro impl�cito).
/// 6. Crea la sesi�n y retorna los tokens.
/// </summary>
internal sealed class LoginClienteOAuthCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IJwtService _jwtService,
    ISesionRepository _sesionRepo,
    IOAuthServiceFactory _oauthFactory,
    IOptions<CustomerPortalSettings> _portalSettings,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<LoginClienteOAuthCommand, Result<LoginClienteResponseDto>>
{
    public async Task<Result<LoginClienteResponseDto>> Handle(LoginClienteOAuthCommand cmd, CancellationToken ct)
    {
        if (!_portalSettings.Value.EnableCustomerPortal)
            return Result<LoginClienteResponseDto>.Failure("El portal de clientes no est� habilitado.");

        // Resolver el servicio OAuth correcto seg�n el proveedor (Google, Facebook, etc.)
        var oauthService = _oauthFactory.Resolver(cmd.Proveedor);
        var oauthInfo = await oauthService.ValidarTokenAsync(cmd.Token, ct);
        if (oauthInfo is null)
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.TokenOAuthInvalido);

        // Buscar por externalId primero (login recurrente con el mismo proveedor)
        var cliente = await _clienteRepo.GetByExternalIdAsync(cmd.Proveedor, oauthInfo.ExternalId, cmd.TenantId, ct);

        if (cliente is null)
        {
            // Buscar por email � puede que ya tenga cuenta con email/password u otro proveedor
            cliente = await _clienteRepo.GetByEmailAsync(oauthInfo.Email, cmd.TenantId, ct);

            if (cliente is null)
            {
                // Primer acceso � crear cliente desde OAuth (email ya verificado por el proveedor)
                var nuevoClienteResult = UsuarioCliente.CrearDesdeOAuth(
                    oauthInfo.Email,
                    oauthInfo.Nombre,
                    oauthInfo.Apellido,
                    cmd.Proveedor,
                    oauthInfo.ExternalId,
                    cmd.TenantId);

                if (!nuevoClienteResult.IsSuccess)
                    return Result<LoginClienteResponseDto>.Failure(nuevoClienteResult.Error!);

                cliente = nuevoClienteResult.Value!;
                await _clienteRepo.AddAsync(cliente, ct);
            }
            else
            {
                // Ya existe con otro proveedor � vincular el nuevo autom�ticamente
                var vincularResult = cliente.VincularProveedor(cmd.Proveedor, oauthInfo.ExternalId, oauthInfo.Email);
                if (!vincularResult.IsSuccess)
                    return Result<LoginClienteResponseDto>.Failure(vincularResult.Error!);

                await _clienteRepo.UpdateAsync(cliente, ct);
            }
        }

        if (!cliente.PuedeAutenticarse())
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CuentaBloqueada);

        var refreshToken = _jwtService.GenerarRefreshToken();
        var refreshTokenHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(refreshToken))).ToLowerInvariant();

        var sesion = Sesion.Crear(
            cliente.Id,
            cliente.TenantId,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_authSettings.Value.RefreshTokenExpirationDays),
            CanalAcceso.Api,
            cmd.Ip,
            cmd.UserAgent);

        await _sesionRepo.AddAsync(sesion, ct);

        var accessToken = _jwtService.GenerarAccessTokenCliente(cliente);
        var expiraEn = _jwtService.ObtenerExpiracionAccessToken();

        return Result<LoginClienteResponseDto>.Success(
            new LoginClienteResponseDto(accessToken, refreshToken, expiraEn, MapearDto(cliente)),
            PortalSuccessMessages.LoginExitoso);
    }

    private static ClienteDto MapearDto(UsuarioCliente c) => new(
        c.Id, c.Email ?? string.Empty, c.Nombre, c.Apellido, c.Telefono,
        c.Estado, c.EmailVerificado, c.TelefonoVerificado,
        c.Proveedores.Select(p => p.Proveedor.ToString()).ToList(),
        c.TenantId, c.CreadoEn);
}

/// <summary>
/// Verifica el email del cliente con el token recibido por correo.
/// Si RequirePhoneVerification = false, el cliente queda Active directamente.
/// Si RequirePhoneVerification = true, queda en Verified hasta verificar el tel�fono.
/// </summary>
internal sealed class VerificarEmailClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IOptions<CustomerPortalSettings> _portalSettings) : IRequestHandler<VerificarEmailClienteCommand, Result>
{
    public async Task<Result> Handle(VerificarEmailClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(cmd.ClienteId, ct);
        if (cliente is null) return Result.Failure(PortalErrorMessages.ClienteNoEncontrado);

        var result = cliente.VerificarEmail(cmd.Token, _portalSettings.Value.RequirePhoneVerification);
        if (!result.IsSuccess) return result;

        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}

/// <summary>
/// Verifica el tel�fono del cliente con el c�digo recibido por SMS.
/// Al verificar, el estado avanza a Active.
/// </summary>
internal sealed class VerificarTelefonoClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo) : IRequestHandler<VerificarTelefonoClienteCommand, Result>
{
    public async Task<Result> Handle(VerificarTelefonoClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(cmd.ClienteId, ct);
        if (cliente is null) return Result.Failure(PortalErrorMessages.ClienteNoEncontrado);

        var result = cliente.VerificarTelefono(cmd.Codigo);
        if (!result.IsSuccess) return result;

        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}

/// <summary>
/// Regenera el token de verificaci�n de email y lo persiste.
/// El env�o del email debe ser manejado por un event handler externo
/// que escuche ClienteRegistradoEvent o un evento espec�fico de reenv�o.
/// </summary>
internal sealed class ReenviarVerificacionEmailCommandHandler(
    IUsuarioClienteRepository _clienteRepo) : IRequestHandler<ReenviarVerificacionEmailCommand, Result>
{
    public async Task<Result> Handle(ReenviarVerificacionEmailCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(cmd.ClienteId, ct);
        if (cliente is null) return Result.Failure(PortalErrorMessages.ClienteNoEncontrado);

        if (cliente.EmailVerificado)
            return Result.Failure("El email ya est� verificado.");

        cliente.GenerarTokenVerificacionEmail();
        await _clienteRepo.UpdateAsync(cliente, ct);

        return Result.Success();
    }
}

/// <summary>
/// Cambia la contrase�a del cliente autenticado.
/// Los clientes OAuth sin contrase�a local pueden establecer una nueva sin verificar la actual.
/// </summary>
internal sealed class CambiarPasswordClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IPasswordService _passwordService) : IRequestHandler<CambiarPasswordClienteCommand, Result>
{
    public async Task<Result> Handle(CambiarPasswordClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(cmd.ClienteId, ct);
        if (cliente is null) return Result.Failure(PortalErrorMessages.ClienteNoEncontrado);

        // Solo verificar contrase�a actual si el cliente ya tiene una
        if (!string.IsNullOrEmpty(cliente.PasswordHash))
        {
            if (!_passwordService.VerifyPassword(cmd.PasswordActual, cliente.PasswordHash))
                return Result.Failure(AuthErrorMessages.PasswordActualIncorrecto);
        }

        var errores = _passwordService.ValidarPolitica(cmd.NuevoPassword);
        if (errores.Count > 0) return Result.Failure(string.Join(" ", errores));

        var result = cliente.CambiarPassword(_passwordService.HashPassword(cmd.NuevoPassword));
        if (!result.IsSuccess) return result;

        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}

// --- Commands adicionales del portal ----------------------------------------

/// <summary>Renueva el AccessToken del cliente usando un RefreshToken v�lido.</summary>
public sealed record RefreshTokenClienteCommand(
    string RefreshToken,
    string Ip) : IRequest<Result<LoginClienteResponseDto>>;

/// <summary>Cierra la sesi�n del cliente revocando el RefreshToken y blacklisteando el AccessToken.</summary>
public sealed record LogoutClienteCommand(
    string RefreshToken,
    string AccessToken) : IRequest<Result>;

/// <summary>Solicita el restablecimiento de contrase�a del cliente por email.</summary>
public sealed record SolicitarRestablecimientoClienteCommand(
    string Email,
    Guid? TenantId) : IRequest<Result>;

/// <summary>Restablece la contrase�a del cliente usando el token recibido por email.</summary>
public sealed record RestablecerPasswordClienteCommand(
    string Token,
    string NuevoPassword) : IRequest<Result>;

/// <summary>Bloquea un cliente (acci�n de admin).</summary>
public sealed record BloquearClienteCommand(
    Guid ClienteId) : IRequest<Result>;

/// <summary>Reactiva un cliente bloqueado (acci�n de admin).</summary>
public sealed record ReactivarClienteCommand(
    Guid ClienteId) : IRequest<Result>;

// --- Handlers adicionales ----------------------------------------------------

/// <summary>
/// Renueva el AccessToken del cliente usando un RefreshToken v�lido.
/// Busca la sesi�n activa por el hash del refresh token.
/// </summary>
internal sealed class RefreshTokenClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    ISesionRepository _sesionRepo,
    IJwtService _jwtService,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<RefreshTokenClienteCommand, Result<LoginClienteResponseDto>>
{
    public async Task<Result<LoginClienteResponseDto>> Handle(RefreshTokenClienteCommand cmd, CancellationToken ct)
    {
        var hash = ComputarHash(cmd.RefreshToken);
        var sesion = await _sesionRepo.GetActivaByRefreshTokenHashAsync(hash, ct);

        if (sesion is null)
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.RefreshTokenInvalido);

        var cliente = await _clienteRepo.GetByIdAsync(sesion.UsuarioId, ct);
        if (cliente is null)
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.ClienteNoEncontrado);

        if (!cliente.PuedeAutenticarse())
            return Result<LoginClienteResponseDto>.Failure(PortalErrorMessages.CuentaBloqueada);

        // Rotar el refresh token � invalidar el anterior y emitir uno nuevo
        var nuevoRefreshToken = _jwtService.GenerarRefreshToken();
        var nuevoHash = ComputarHash(nuevoRefreshToken);
        var nuevaExpiracion = DateTime.UtcNow.AddDays(_authSettings.Value.RefreshTokenExpirationDays);

        sesion.Renovar(nuevoHash, nuevaExpiracion);
        await _sesionRepo.UpdateAsync(sesion, ct);

        var accessToken = _jwtService.GenerarAccessTokenCliente(cliente);
        var expiraEn = _jwtService.ObtenerExpiracionAccessToken();

        return Result<LoginClienteResponseDto>.Success(
            new LoginClienteResponseDto(accessToken, nuevoRefreshToken, expiraEn, MapearDto(cliente)));
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
/// Cierra la sesi�n del cliente: revoca la sesi�n y agrega el AccessToken a la blacklist.
/// </summary>
internal sealed class LogoutClienteCommandHandler(
    ISesionRepository _sesionRepo,
    ITokenBlacklistService _blacklist,
    IJwtService _jwtService) : IRequestHandler<LogoutClienteCommand, Result>
{
    public async Task<Result> Handle(LogoutClienteCommand cmd, CancellationToken ct)
    {
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(cmd.RefreshToken))).ToLowerInvariant();

        var sesion = await _sesionRepo.GetActivaByRefreshTokenHashAsync(hash, ct);
        if (sesion is null)
            return Result.Failure(PortalErrorMessages.SesionNoEncontrada);

        sesion.Revocar();
        await _sesionRepo.UpdateAsync(sesion, ct);

        // Blacklistear el AccessToken para invalidarlo inmediatamente
        var jti = _jwtService.ExtraerJti(cmd.AccessToken);
        var expiracion = _jwtService.ExtraerExpiracion(cmd.AccessToken);
        if (jti is not null && expiracion.HasValue)
        {
            var ttl = expiracion.Value - DateTime.UtcNow;
            if (ttl > TimeSpan.Zero)
                await _blacklist.AgregarAsync(jti, ttl, ct);
        }

        return Result.Success();
    }
}

/// <summary>
/// Solicita el restablecimiento de contrase�a del cliente.
/// Genera un token y lo guarda en el aggregate � el env�o del email
/// <summary>
/// Solicita el restablecimiento de contrasena del cliente.
/// Genera un token con expiracion y lo persiste en el aggregate.
/// Siempre retorna exito para no revelar si el email esta registrado.
/// </summary>
internal sealed class SolicitarRestablecimientoClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IOptions<AuthSettings> _authSettings) : IRequestHandler<SolicitarRestablecimientoClienteCommand, Result>
{
    public async Task<Result> Handle(SolicitarRestablecimientoClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByEmailAsync(cmd.Email, cmd.TenantId, ct);
        if (cliente is null) return Result.Success();

        cliente.GenerarTokenRestablecimiento(_authSettings.Value.PasswordResetTokenExpirationHours);
        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}

/// <summary>
/// Restablece la contrasena del cliente usando el token recibido por email.
/// </summary>
internal sealed class RestablecerPasswordClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo,
    IPasswordService _passwordService) : IRequestHandler<RestablecerPasswordClienteCommand, Result>
{
    public async Task<Result> Handle(RestablecerPasswordClienteCommand cmd, CancellationToken ct)
    {
        var errores = _passwordService.ValidarPolitica(cmd.NuevoPassword);
        if (errores.Count > 0) return Result.Failure(string.Join(" ", errores));

        var cliente = await _clienteRepo.GetByTokenRestablecimientoAsync(cmd.Token, ct: ct);
        if (cliente is null)
            return Result.Failure("El token de restablecimiento es invalido o ha expirado.");

        var result = cliente.RestablecerPassword(cmd.Token, _passwordService.HashPassword(cmd.NuevoPassword));
        if (!result.IsSuccess) return result;

        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}
/// Bloquea un cliente. Solo puede ejecutarlo un administrador.
/// </summary>
internal sealed class BloquearClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo) : IRequestHandler<BloquearClienteCommand, Result>
{
    public async Task<Result> Handle(BloquearClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(cmd.ClienteId, ct);
        if (cliente is null) return Result.Failure(PortalErrorMessages.ClienteNoEncontrado);

        var result = cliente.Bloquear();
        if (!result.IsSuccess) return result;

        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}

/// <summary>
/// Reactiva un cliente bloqueado. Solo puede ejecutarlo un administrador.
/// </summary>
internal sealed class ReactivarClienteCommandHandler(
    IUsuarioClienteRepository _clienteRepo) : IRequestHandler<ReactivarClienteCommand, Result>
{
    public async Task<Result> Handle(ReactivarClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _clienteRepo.GetByIdAsync(cmd.ClienteId, ct);
        if (cliente is null) return Result.Failure(PortalErrorMessages.ClienteNoEncontrado);

        var result = cliente.Reactivar();
        if (!result.IsSuccess) return result;

        await _clienteRepo.UpdateAsync(cliente, ct);
        return Result.Success();
    }
}

// --- Mensajes espec�ficos del portal -----------------------------------------

/// <summary>Mensajes de error del portal de clientes.</summary>
internal static class PortalErrorMessages
{
    public const string ClienteNoEncontrado = "El cliente no fue encontrado.";
    public const string EmailYaRegistrado = "El email ya est� registrado en el portal.";
    public const string CredencialesInvalidas = "Las credenciales son inv�lidas.";
    public const string CuentaBloqueada = "La cuenta est� bloqueada. Contacte al soporte.";
    public const string CuentaNoActiva = "La cuenta no est� activa. Verifica tu email para continuar.";
    public const string SinPasswordLocal = "Esta cuenta usa login social. Usa Google o Facebook para ingresar.";
    public const string TokenOAuthInvalido = "El token del proveedor externo es inv�lido o ha expirado.";
    public const string RefreshTokenInvalido = "El refresh token es inv�lido o ha expirado.";
    public const string SesionNoEncontrada = "La sesi�n no fue encontrada.";
    public const string RegistroCerrado = "El registro de nuevos clientes est� deshabilitado.";
}

/// <summary>Mensajes de �xito del portal de clientes.</summary>
internal static class PortalSuccessMessages
{
    public const string ClienteRegistrado = "Registro exitoso. Revisa tu email para verificar tu cuenta.";
    public const string LoginExitoso = "Login exitoso.";
    public const string VerificacionReenviada = "Email de verificaci�n reenviado correctamente.";
}
