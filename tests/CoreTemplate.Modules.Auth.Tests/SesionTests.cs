using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class SesionTests
{
    private static Sesion CrearSesion(DateTime? expiraEn = null) =>
        Sesion.Crear(
            Guid.NewGuid(), null,
            "hash123",
            expiraEn ?? DateTime.UtcNow.AddDays(7),
            CanalAcceso.Web,
            "127.0.0.1",
            "TestAgent");

    [Fact]
    public void Crear_DebeCrearSesionActiva()
    {
        var sesion = CrearSesion();

        sesion.EsActiva.Should().BeTrue();
        sesion.EsValida.Should().BeTrue();
        sesion.Canal.Should().Be(CanalAcceso.Web);
        sesion.Ip.Should().Be("127.0.0.1");
    }

    [Fact]
    public void Revocar_DebeMarcarSesionComoInactiva()
    {
        var sesion = CrearSesion();

        sesion.Revocar();

        sesion.EsActiva.Should().BeFalse();
        sesion.EsValida.Should().BeFalse();
    }

    [Fact]
    public void Renovar_DebeActualizarHashYExpiracion()
    {
        var sesion = CrearSesion();
        var nuevaExpiracion = DateTime.UtcNow.AddDays(14);

        sesion.Renovar("nuevohash", nuevaExpiracion);

        sesion.RefreshTokenHash.Should().Be("nuevohash");
        sesion.ExpiraEn.Should().Be(nuevaExpiracion);
        sesion.EsActiva.Should().BeTrue();
    }

    [Fact]
    public void EsValida_SesionExpirada_DebeRetornarFalso()
    {
        var sesion = CrearSesion(expiraEn: DateTime.UtcNow.AddSeconds(-1));

        sesion.EsValida.Should().BeFalse();
    }

    [Fact]
    public void EsValida_SesionRevocada_DebeRetornarFalso()
    {
        var sesion = CrearSesion();
        sesion.Revocar();

        sesion.EsValida.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConCanalesDistintos_DebeRegistrarCanalCorrecto()
    {
        var sesionMobile = Sesion.Crear(
            Guid.NewGuid(), null, "hash", DateTime.UtcNow.AddDays(7),
            CanalAcceso.Mobile, "127.0.0.1", "MobileAgent");

        sesionMobile.Canal.Should().Be(CanalAcceso.Mobile);
    }
}
