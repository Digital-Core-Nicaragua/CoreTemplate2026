using CoreTemplate.Api.Common;
using CoreTemplate.Modules.Auth.Api.Contracts;
using CoreTemplate.Modules.Auth.Application.Commands.PortalClientes;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Application.Queries.GetClientes;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Modules.Auth.Api.Controllers;

/// <summary>
/// Endpoints del portal de clientes externos.
/// Solo disponibles cuando CustomerPortalSettings:EnableCustomerPortal = true.
/// </summary>
[Route("api/portal/clientes")]
public sealed class PortalClientesController(
    IMediator _mediator,
    ICurrentUser _currentUser,
    ICurrentTenant _currentTenant) : BaseApiController
{
    // --- Registro ---

    /// <summary>Registra un nuevo cliente con email y contraseña.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registro([FromBody] RegistrarClienteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegistrarClienteCommand(
            request.Email, request.Password, request.Nombre,
            request.Apellido, request.Telefono, _currentTenant.TenantId), ct);

        if (!result.IsSuccess)
            return ConflictResponse<ClienteDto>(result.Errors);

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>
    /// Registra un cliente por numero de telefono y envia OTP por WhatsApp/SMS.
    /// Requiere CustomerPortalSettings:RegistroPorTelefono:Enabled = true.
    /// </summary>
    [HttpPost("registro/telefono")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistroPorTelefono(
        [FromBody] RegistrarClientePorTelefonoRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegistrarClientePorTelefonoCommand(
            request.Telefono, request.Nombre, request.Apellido, _currentTenant.TenantId), ct);

        if (!result.IsSuccess)
            return ConflictResponse<Guid>(result.Errors);

        return SuccessResponse(result.Value, result.Message);
    }

    // --- Login ---

    /// <summary>Autentica un cliente con email y contraseña.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginClienteRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(
            new LoginClienteCommand(request.Email, request.Password, ip, userAgent), ct);

        if (!result.IsSuccess)
            return UnauthorizedResponse<LoginClienteResponseDto>(result.Errors);

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>
    /// Solicita un OTP para login por telefono.
    /// Siempre retorna exito para no revelar si el telefono existe.
    /// Requiere CustomerPortalSettings:RegistroPorTelefono:Enabled = true.
    /// </summary>
    [HttpPost("login/telefono/solicitar-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SolicitarOtpLoginTelefono(
        [FromBody] SolicitarOtpTelefonoRequest request, CancellationToken ct)
    {
        await _mediator.Send(new SolicitarOtpLoginTelefonoCommand(
            request.Telefono, _currentTenant.TenantId), ct);

        return SuccessResponse(true, "Si el telefono esta registrado, recibiras un codigo.");
    }

    /// <summary>
    /// Verifica el OTP recibido por WhatsApp/SMS.
    /// Funciona tanto para activar el registro como para hacer login.
    /// </summary>
    [HttpPost("login/telefono/verificar")]
    [AllowAnonymous]
    public async Task<IActionResult> VerificarOtpTelefono(
        [FromBody] VerificarOtpTelefonoRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(new VerificarOtpTelefonoCommand(
            request.Telefono, request.Codigo, ip, userAgent, _currentTenant.TenantId), ct);

        if (!result.IsSuccess)
            return UnauthorizedResponse<LoginClienteResponseDto>(result.Errors);

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Autentica o registra un cliente mediante OAuth externo (Google, Facebook).</summary>
    [HttpPost("login/oauth")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginOAuth([FromBody] LoginClienteOAuthRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(new LoginClienteOAuthCommand(
            request.Proveedor, request.Token, ip, userAgent, _currentTenant.TenantId), ct);

        if (!result.IsSuccess)
            return UnauthorizedResponse<LoginClienteResponseDto>(result.Errors);

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Renueva el AccessToken del cliente usando un RefreshToken valido.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenClienteRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await _mediator.Send(
            new RefreshTokenClienteCommand(request.RefreshToken, ip), ct);

        if (!result.IsSuccess)
            return UnauthorizedResponse<LoginClienteResponseDto>(result.Errors);

        return SuccessResponse(result.Value!, result.Message);
    }

    /// <summary>Cierra la sesion del cliente revocando el RefreshToken.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutClienteRequest request, CancellationToken ct)
    {
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        var result = await _mediator.Send(
            new LogoutClienteCommand(request.RefreshToken, accessToken), ct);

        if (!result.IsSuccess)
            return NotFoundResponse<object>(result.Error!);

        return SuccessResponse(true, "Sesion cerrada correctamente.");
    }

    // --- Verificacion ---

    /// <summary>Verifica el email del cliente usando el token recibido por correo.</summary>
    [HttpPost("verificar-email")]
    [Authorize]
    public async Task<IActionResult> VerificarEmail([FromBody] VerificarEmailClienteRequest request, CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;

        var result = await _mediator.Send(new VerificarEmailClienteCommand(clienteId, request.Token), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Email verificado correctamente.");
    }

    /// <summary>Verifica el telefono del cliente usando el codigo recibido por SMS (flujo email+telefono).</summary>
    [HttpPost("verificar-telefono")]
    [Authorize]
    public async Task<IActionResult> VerificarTelefono([FromBody] VerificarTelefonoClienteRequest request, CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;

        var result = await _mediator.Send(new VerificarTelefonoClienteCommand(clienteId, request.Codigo), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Telefono verificado correctamente.");
    }

    /// <summary>Reenvía el email de verificacion.</summary>
    [HttpPost("reenviar-verificacion")]
    [Authorize]
    public async Task<IActionResult> ReenviarVerificacion(CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;

        var result = await _mediator.Send(new ReenviarVerificacionEmailCommand(clienteId), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Email de verificacion reenviado correctamente.");
    }

    // --- Perfil ---

    /// <summary>Cambia la contraseña del cliente autenticado.</summary>
    [HttpPut("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordClienteRequest request, CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;

        var result = await _mediator.Send(new CambiarPasswordClienteCommand(
            clienteId, request.PasswordActual, request.NuevoPassword), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Contraseña actualizada correctamente.");
    }

    // --- Restablecimiento de contraseña ---

    /// <summary>Solicita el restablecimiento de contraseña por email.</summary>
    [HttpPost("solicitar-restablecimiento")]
    [AllowAnonymous]
    public async Task<IActionResult> SolicitarRestablecimiento(
        [FromBody] SolicitarRestablecimientoClienteRequest request, CancellationToken ct)
    {
        await _mediator.Send(new SolicitarRestablecimientoClienteCommand(
            request.Email, _currentTenant.TenantId), ct);

        return SuccessResponse(true, "Si el email existe, recibiras las instrucciones en tu correo.");
    }

    /// <summary>Restablece la contraseña usando el token recibido por email.</summary>
    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RestablecerPassword(
        [FromBody] RestablecerPasswordClienteRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RestablecerPasswordClienteCommand(
            request.Token, request.NuevoPassword), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Contraseña restablecida correctamente.");
    }

    // --- Sesiones del cliente ---

    /// <summary>Lista las sesiones activas del cliente. Requiere EnableSessionManagement = true.</summary>
    [HttpGet("mis-sesiones")]
    [Authorize]
    public async Task<IActionResult> MisSesiones(CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;
        var result = await _mediator.Send(new GetMisSesionesClienteQuery(clienteId), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(result.Value!, "Sesiones obtenidas correctamente.");
    }

    /// <summary>Cierra una sesion especifica. Requiere EnableSessionManagement = true.</summary>
    [HttpDelete("mis-sesiones/{sesionId:guid}")]
    [Authorize]
    public async Task<IActionResult> CerrarSesion(Guid sesionId, CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;
        var result = await _mediator.Send(new CerrarSesionClienteCommand(clienteId, sesionId), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Sesion cerrada correctamente.");
    }

    /// <summary>Cierra todas las sesiones excepto la actual. Requiere EnableSessionManagement = true.</summary>
    [HttpDelete("mis-sesiones/otras")]
    [Authorize]
    public async Task<IActionResult> CerrarOtrasSesiones(CancellationToken ct)
    {
        var clienteId = _currentUser.Id ?? Guid.Empty;
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = await _mediator.Send(new CerrarOtrasSesionesClienteCommand(clienteId, accessToken), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Otras sesiones cerradas correctamente.");
    }

    // --- Admin ---

    /// <summary>Lista clientes del portal. Requiere rol Admin.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ListarClientes(
        [FromQuery] EstadoUsuarioCliente? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetClientesQuery(_currentTenant.TenantId, estado, pagina, tamanoPagina), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(result.Value!, "Clientes obtenidos correctamente.");
    }

    /// <summary>Bloquea un cliente. Requiere rol Admin.</summary>
    [HttpPut("{id:guid}/bloquear")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Bloquear(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new BloquearClienteCommand(id), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Cliente bloqueado correctamente.");
    }

    /// <summary>Reactiva un cliente bloqueado. Requiere rol Admin.</summary>
    [HttpPut("{id:guid}/reactivar")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Reactivar(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReactivarClienteCommand(id), ct);

        if (!result.IsSuccess)
            return BadRequestResponse<object>(result.Error!);

        return SuccessResponse(true, "Cliente reactivado correctamente.");
    }
}
