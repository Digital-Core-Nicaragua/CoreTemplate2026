using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Application.Commands.Registro;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class RegistrarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IRolRepository _rolRepo = Substitute.For<IRolRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IOptions<PasswordPolicySettings> _policy = Options.Create(new PasswordPolicySettings
    {
        MinLength = 8,
        RequireUppercase = true,
        RequireLowercase = true,
        RequireDigit = true,
        RequireSpecialChar = false
    });

    private RegistrarUsuarioCommandHandler CrearHandler() => new(
        _usuarioRepo, _rolRepo, _passwordService, _policy);

    [Fact]
    public async Task Handle_DatosValidos_DebeRegistrarUsuario()
    {
        var cmd = new RegistrarUsuarioCommand("nuevo@test.com", "Nuevo Usuario", "Password1", "Password1");

        _usuarioRepo.ExistsByEmailAsync("nuevo@test.com", null, default).Returns(false);
        _passwordService.ValidarPolitica("Password1").Returns([]);
        _passwordService.HashPassword("Password1").Returns("$2a$12$hash");
        _rolRepo.GetAllAsync(null, default).Returns([]);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _usuarioRepo.Received(1).AddAsync(Arg.Any<Domain.Aggregates.Usuario>(), default);
    }

    [Fact]
    public async Task Handle_EmailYaRegistrado_DebeFallar()
    {
        var cmd = new RegistrarUsuarioCommand("existente@test.com", "Usuario", "Password1", "Password1");

        _usuarioRepo.ExistsByEmailAsync("existente@test.com", null, default).Returns(true);
        _passwordService.ValidarPolitica("Password1").Returns([]);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email");
        await _usuarioRepo.DidNotReceive().AddAsync(Arg.Any<Domain.Aggregates.Usuario>(), default);
    }

    [Fact]
    public async Task Handle_PasswordNoSatisfacePolitica_DebeFallar()
    {
        var cmd = new RegistrarUsuarioCommand("nuevo@test.com", "Usuario", "weak", "weak");

        _usuarioRepo.ExistsByEmailAsync("nuevo@test.com", null, default).Returns(false);
        _passwordService.ValidarPolitica("weak").Returns(["La contraseña debe tener al menos 8 caracteres."]);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("8 caracteres"));
    }

    [Fact]
    public async Task Handle_ConRolUserExistente_DebeAsignarlo()
    {
        var cmd = new RegistrarUsuarioCommand("nuevo@test.com", "Usuario", "Password1", "Password1");
        var rolUser = Domain.Aggregates.Rol.Crear("User", "Usuario estándar", esSistema: true).Value!;

        _usuarioRepo.ExistsByEmailAsync("nuevo@test.com", null, default).Returns(false);
        _passwordService.ValidarPolitica("Password1").Returns([]);
        _passwordService.HashPassword("Password1").Returns("$2a$12$hash");
        _rolRepo.GetAllAsync(null, default).Returns([rolUser]);

        Domain.Aggregates.Usuario? usuarioGuardado = null;
        await _usuarioRepo.AddAsync(Arg.Do<Domain.Aggregates.Usuario>(u => usuarioGuardado = u), default);

        var handler = CrearHandler();
        await handler.Handle(cmd, default);

        usuarioGuardado.Should().NotBeNull();
        usuarioGuardado!.Roles.Should().ContainSingle(r => r.RolId == rolUser.Id);
    }
}
