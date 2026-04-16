using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.CambiarPassword;
using CoreTemplate.Modules.Auth.Application.Commands.DosFactores;
using CoreTemplate.Modules.Auth.Application.Commands.Login;
using CoreTemplate.Modules.Auth.Application.Commands.Logout;
using CoreTemplate.Modules.Auth.Application.Commands.RefreshToken;
using CoreTemplate.Modules.Auth.Application.Commands.Registro;
using CoreTemplate.Modules.Auth.Application.Commands.RestablecerPassword;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints de autenticación: login, registro, refresh token, logout y 2FA.
/// </summary>
[Route("api/auth")]
public sealed class AuthController(IMediator _mediator) : BaseApiController
{
    // ─── Login ────────────────────────────────────────────────────────────────

    /// <summary>Autentica un usuario con email y contraseña.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password, ip, userAgent, request.Canal), ct);

        if (!result.IsSuccess)
        {
            return UnauthorizedResponse<object>(result.Errors);
        }

        return SuccessResponse(result.Value!, result.Message);
    }

    // ─── Registro ─────────────────────────────────────────────────────────────

    /// <summary>Registra un nuevo usuario en el sistema.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registro([FromBody] RegistrarUsuarioRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RegistrarUsuarioCommand(
                request.Email, request.Nombre, request.Password,
                request.ConfirmPassword, TipoUsuario: request.TipoUsuario), ct);

        if (!result.IsSuccess)
        {
            return ConflictResponse<Guid>(result.Errors);
        }

        return CreatedResponse(
            nameof(UsuariosController) + "_" + nameof(UsuariosController.GetById),
            new { id = result.Value },
            result.Value,
            result.Message);
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────

    /// <summary>Renueva el AccessToken usando un RefreshToken válido.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ip), ct);

        if (!result.IsSuccess)
        {
            return UnauthorizedResponse<TokenResponseDto>(result.Errors);
        }

        return SuccessResponse(result.Value!, result.Message);
    }

    // ─── Logout ───────────────────────────────────────────────────────────────

    /// <summary>Cierra la sesión revocando el RefreshToken.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var result = await _mediator.Send(new LogoutCommand(request.RefreshToken, accessToken, ip, userAgent), ct);

        if (!result.IsSuccess)
        {
            return NotFoundResponse<object>(result.Error!);
        }

        return SuccessResponse(true, CommonSuccessMessages.ActualizadoExitosamente);
    }

    // ─── Restablecimiento de contraseña ──────────────────────────────────────

    /// <summary>Solicita el restablecimiento de contraseña por email.</summary>
    [HttpPost("solicitar-restablecimiento")]
    [AllowAnonymous]
    public async Task<IActionResult> SolicitarRestablecimiento(
        [FromBody] SolicitarRestablecimientoRequest request, CancellationToken ct)
    {
        await _mediator.Send(new SolicitarRestablecimientoCommand(request.Email), ct);

        // Siempre retornar éxito — no revelar si el email existe
        return SuccessResponse(true, "Si el email existe, recibirás las instrucciones en tu correo.");
    }

    /// <summary>Restablece la contraseña usando el token recibido por email.</summary>
    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RestablecerPassword(
        [FromBody] RestablecerPasswordRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(
            new RestablecerPasswordCommand(request.Token, request.NuevoPassword, request.ConfirmPassword, ip, userAgent), ct);

        if (!result.IsSuccess)
        {
            return BadRequestResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "Contraseña restablecida correctamente.");
    }

    // ─── 2FA ──────────────────────────────────────────────────────────────────

    /// <summary>Inicia la activación del 2FA generando el QR y códigos de recuperación.</summary>
    [HttpPost("2fa/activar")]
    [Authorize]
    public async Task<IActionResult> Activar2FA(CancellationToken ct)
    {
        var result = await _mediator.Send(new Activar2FACommand(), ct);

        if (!result.IsSuccess)
        {
            return BadRequestResponse<object>(result.Errors);
        }

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Confirma la activación del 2FA verificando el primer código TOTP.</summary>
    [HttpPost("2fa/confirmar")]
    [Authorize]
    public async Task<IActionResult> Confirmar2FA([FromBody] Confirmar2FARequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new Confirmar2FACommand(request.Codigo), ct);

        if (!result.IsSuccess)
        {
            return BadRequestResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "2FA activado y verificado correctamente.");
    }

    /// <summary>Verifica el código TOTP en el flujo de login con 2FA activo.</summary>
    [HttpPost("2fa/verificar")]
    [AllowAnonymous]
    public async Task<IActionResult> Verificar2FA([FromBody] Verificar2FARequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(
            new Verificar2FACommand(request.TokenTemporal, request.Codigo, ip, userAgent), ct);

        if (!result.IsSuccess)
        {
            return UnauthorizedResponse<LoginResponseDto>(result.Errors);
        }

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Desactiva el 2FA verificando el código TOTP actual.</summary>
    [HttpPost("2fa/desactivar")]
    [Authorize]
    public async Task<IActionResult> Desactivar2FA([FromBody] Desactivar2FARequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new Desactivar2FACommand(request.Codigo), ct);

        if (!result.IsSuccess)
        {
            return BadRequestResponse<object>(result.Error!);
        }

        return SuccessResponse(true, "2FA desactivado correctamente.");
    }
}
