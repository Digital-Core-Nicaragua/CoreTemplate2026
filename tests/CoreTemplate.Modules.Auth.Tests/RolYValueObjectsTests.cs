using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using FluentAssertions;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class RolTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearRolCorrectamente()
    {
        var result = Rol.Crear("Admin", "Administrador del sistema", esSistema: false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nombre.Should().Be("Admin");
        result.Value.Descripcion.Should().Be("Administrador del sistema");
        result.Value.EsSistema.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConNombreVacio_DebeFallar()
    {
        var result = Rol.Crear("", "Descripción", esSistema: false);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AgregarPermiso_PermisoNuevo_DebeAgregarloCorrectamente()
    {
        var rol = Rol.Crear("Admin", "Desc", esSistema: false).Value!;
        var permisoId = Guid.NewGuid();

        var result = rol.AgregarPermiso(permisoId);

        result.IsSuccess.Should().BeTrue();
        rol.Permisos.Should().ContainSingle(p => p.PermisoId == permisoId);
    }

    [Fact]
    public void AgregarPermiso_PermisoYaAsignado_DebeFallar()
    {
        var rol = Rol.Crear("Admin", "Desc", esSistema: false).Value!;
        var permisoId = Guid.NewGuid();
        rol.AgregarPermiso(permisoId);

        var result = rol.AgregarPermiso(permisoId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void QuitarPermiso_PermisoExistente_DebeQuitarlo()
    {
        var rol = Rol.Crear("Admin", "Desc", esSistema: false).Value!;
        var permisoId = Guid.NewGuid();
        rol.AgregarPermiso(permisoId);

        var result = rol.QuitarPermiso(permisoId);

        result.IsSuccess.Should().BeTrue();
        rol.Permisos.Should().BeEmpty();
    }

    [Fact]
    public void QuitarPermiso_PermisoNoAsignado_DebeFallar()
    {
        var rol = Rol.Crear("Admin", "Desc", esSistema: false).Value!;

        var result = rol.QuitarPermiso(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PuedeEliminarse_RolNoSistema_DebeRetornarVerdadero()
    {
        var rol = Rol.Crear("Custom", "Desc", esSistema: false).Value!;

        rol.PuedeEliminarse().Should().BeTrue();
    }

    [Fact]
    public void PuedeEliminarse_RolSistema_DebeRetornarFalso()
    {
        var rol = Rol.Crear("SuperAdmin", "Desc", esSistema: true).Value!;

        rol.PuedeEliminarse().Should().BeFalse();
    }
}

public sealed class EmailTests
{
    [Theory]
    [InlineData("usuario@dominio.com")]
    [InlineData("USUARIO@DOMINIO.COM")]
    [InlineData("user.name+tag@example.co.uk")]
    public void Crear_ConEmailValido_DebeCrearloNormalizado(string email)
    {
        var result = Email.Crear(email);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Valor.Should().Be(email.ToLowerInvariant().Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sinArroba")]
    [InlineData("@sinUsuario.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@sinPunto")]
    public void Crear_ConEmailInvalido_DebeFallar(string email)
    {
        var result = Email.Crear(email);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void DosEmails_ConMismoValor_DebenSerIguales()
    {
        var a = Email.Crear("test@test.com").Value!;
        var b = Email.Crear("TEST@TEST.COM").Value!;

        a.Should().Be(b);
    }

    [Fact]
    public void Crear_EmailMayorA200Caracteres_DebeFallar()
    {
        var emailLargo = new string('a', 195) + "@b.com";

        var result = Email.Crear(emailLargo);

        result.IsSuccess.Should().BeFalse();
    }
}

public sealed class PasswordHashTests
{
    [Fact]
    public void Crear_ConHashValido_DebeCrearloCorrectamente()
    {
        var result = PasswordHash.Crear("$2a$12$hashvalido");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Valor.Should().Be("$2a$12$hashvalido");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_ConHashVacio_DebeFallar(string? hash)
    {
        var result = PasswordHash.Crear(hash!);

        result.IsSuccess.Should().BeFalse();
    }
}
