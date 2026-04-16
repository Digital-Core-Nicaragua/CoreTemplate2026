using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Commands.Login;
using CoreTemplate.Modules.Auth.Application.DTOs;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class LoginCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IRegistroAuditoriaRepository _auditoriaRepo = Substitute.For<IRegistroAuditoriaRepository>();
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly ISesionService _sesionService = Substitute.For<ISesionService>();
    private readonly IOptions<LockoutSettings> _lockout = Options.Create(new LockoutSettings
    {
        MaxFailedAttempts = 5,
        LockoutDurationMinutes = 15
    });
    private readonly IOptions<AuthSettings> _authSettings = Options.Create(new AuthSettings
    {
        RefreshTokenExpirationDays = 7
    });

    private LoginCommandHandler CrearHandler() => new(
        _usuarioRepo, _auditoriaRepo, _sesionRepo, _passwordService,
        _jwtService, _sesionService, _lockout, _authSettings);

    private static Usuario CrearUsuarioActivo(string email = "test@test.com")
    {
        var emailVO = Email.Crear(email).Value!;
        var hashVO = PasswordHash.Crear("$2a$12$hash").Value!;
        var usuario = Usuario.Crear(emailVO, "Test User", hashVO).Value!;
        usuario.Activar();
        usuario.ClearDomainEvents();
        return usuario;
    }

    [Fact]
    public async Task Handle_CredencialesValidas_DebeRetornarLoginResponse()
    {
        var usuario = CrearUsuarioActivo();
        var cmd = new LoginCommand("test@test.com", "password123", "127.0.0.1", "TestAgent");

        _usuarioRepo.GetByEmailAsync("test@test.com", ct: default).Returns(usuario);
        _passwordService.VerifyPassword("password123", Arg.Any<string>()).Returns(true);
        _jwtService.GenerarAccessToken(usuario).Returns("access_token");
        _jwtService.GenerarRefreshToken().Returns("refresh_token");
        _jwtService.ObtenerExpiracionAccessToken().Returns(DateTime.UtcNow.AddMinutes(15));
        _sesionService.VerificarYAplicarLimiteAsync(usuario.Id, Arg.Any<Domain.Enums.TipoUsuario>(), default)
            .Returns(true);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value.Should().BeOfType<LoginResponseDto>().Subject;
        response.AccessToken.Should().Be("access_token");
        response.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_EmailNoExiste_DebeRetornarCredencialesInvalidas()
    {
        var cmd = new LoginCommand("noexiste@test.com", "password", "127.0.0.1", "Agent");

        _usuarioRepo.GetByEmailAsync("noexiste@test.com", ct: default).ReturnsNull();

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("inválidas");
    }

    [Fact]
    public async Task Handle_PasswordIncorrecto_DebeIncrementarIntentosFallidos()
    {
        var usuario = CrearUsuarioActivo();
        var cmd = new LoginCommand("test@test.com", "wrongpassword", "127.0.0.1", "Agent");

        _usuarioRepo.GetByEmailAsync("test@test.com", ct: default).Returns(usuario);
        _passwordService.VerifyPassword("wrongpassword", Arg.Any<string>()).Returns(false);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        await _usuarioRepo.Received(1).UpdateAsync(usuario, default);
    }

    [Fact]
    public async Task Handle_CuentaBloqueada_DebeRetornarErrorBloqueo()
    {
        var usuario = CrearUsuarioActivo();
        usuario.Bloquear(DateTime.UtcNow.AddMinutes(15));
        var cmd = new LoginCommand("test@test.com", "password", "127.0.0.1", "Agent");

        _usuarioRepo.GetByEmailAsync("test@test.com", ct: default).Returns(usuario);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("bloqueada");
    }

    [Fact]
    public async Task Handle_CuentaInactiva_DebeRetornarErrorInactiva()
    {
        var emailVO = Email.Crear("test@test.com").Value!;
        var hashVO = PasswordHash.Crear("$2a$12$hash").Value!;
        var usuario = Usuario.Crear(emailVO, "Test", hashVO).Value!;
        // No activar — queda en Pendiente
        var cmd = new LoginCommand("test@test.com", "password", "127.0.0.1", "Agent");

        _usuarioRepo.GetByEmailAsync("test@test.com", ct: default).Returns(usuario);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("inactiva");
    }

    [Fact]
    public async Task Handle_UsuarioCon2FA_DebeRetornarTokenTemporal()
    {
        var usuario = CrearUsuarioActivo();
        usuario.ActivarDosFactores("secretkey", ["hash1"]);
        var cmd = new LoginCommand("test@test.com", "password123", "127.0.0.1", "Agent");

        _usuarioRepo.GetByEmailAsync("test@test.com", ct: default).Returns(usuario);
        _passwordService.VerifyPassword("password123", Arg.Any<string>()).Returns(true);
        _jwtService.GenerarTokenTemporal2FA(usuario.Id).Returns("temp_token_2fa");

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value.Should().BeOfType<Login2FARequeridoDto>().Subject;
        response.Requires2FA.Should().BeTrue();
        response.TokenTemporal.Should().Be("temp_token_2fa");
    }
}
