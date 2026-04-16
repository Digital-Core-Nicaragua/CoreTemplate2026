using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class TipoUsuarioTests
{
    private static Usuario CrearUsuario(TipoUsuario tipo = TipoUsuario.Humano)
    {
        var email = Email.Crear("test@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;
        return Usuario.Crear(email, "Test", hash, tipoUsuario: tipo).Value!;
    }

    [Fact]
    public void Crear_SinEspecificarTipo_DebeSerHumano()
    {
        var email = Email.Crear("test@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;

        var usuario = Usuario.Crear(email, "Test", hash).Value!;

        usuario.TipoUsuario.Should().Be(TipoUsuario.Humano);
    }

    [Fact]
    public void Crear_ComoSistema_DebeAsignarTipoSistema()
    {
        var usuario = CrearUsuario(TipoUsuario.Sistema);

        usuario.TipoUsuario.Should().Be(TipoUsuario.Sistema);
    }

    [Fact]
    public void Crear_ComoIntegracion_DebeAsignarTipoIntegracion()
    {
        var usuario = CrearUsuario(TipoUsuario.Integracion);

        usuario.TipoUsuario.Should().Be(TipoUsuario.Integracion);
    }

    [Theory]
    [InlineData(TipoUsuario.Humano)]
    [InlineData(TipoUsuario.Sistema)]
    [InlineData(TipoUsuario.Integracion)]
    public void Crear_CualquierTipo_DebeCrearseCorrectamente(TipoUsuario tipo)
    {
        var usuario = CrearUsuario(tipo);

        usuario.TipoUsuario.Should().Be(tipo);
        usuario.Estado.Should().Be(EstadoUsuario.Pendiente);
    }
}
