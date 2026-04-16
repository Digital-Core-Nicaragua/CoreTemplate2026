using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class SucursalTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearSucursalActiva()
    {
        var result = Sucursal.Crear("SUC001", "Sucursal Central");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Codigo.Should().Be("SUC001");
        result.Value.Nombre.Should().Be("Sucursal Central");
        result.Value.EsActiva.Should().BeTrue();
    }

    [Fact]
    public void Crear_CodigoSeConvierteAMayusculas()
    {
        var result = Sucursal.Crear("suc001", "Sucursal");

        result.Value!.Codigo.Should().Be("SUC001");
    }

    [Fact]
    public void Crear_CodigoVacio_DebeFallar()
    {
        var result = Sucursal.Crear("", "Sucursal");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_NombreVacio_DebeFallar()
    {
        var result = Sucursal.Crear("SUC001", "");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Desactivar_SucursalActiva_DebeDesactivarla()
    {
        var sucursal = Sucursal.Crear("SUC001", "Sucursal").Value!;

        var result = sucursal.Desactivar();

        result.IsSuccess.Should().BeTrue();
        sucursal.EsActiva.Should().BeFalse();
    }

    [Fact]
    public void Activar_SucursalInactiva_DebeActivarla()
    {
        var sucursal = Sucursal.Crear("SUC001", "Sucursal").Value!;
        sucursal.Desactivar();

        var result = sucursal.Activar();

        result.IsSuccess.Should().BeTrue();
        sucursal.EsActiva.Should().BeTrue();
    }

    // ─── Invariantes UsuarioSucursal ──────────────────────────────────────────

    private static Usuario CrearUsuarioActivo()
    {
        var email = Email.Crear("test@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;
        var usuario = Usuario.Crear(email, "Test", hash).Value!;
        usuario.Activar();
        return usuario;
    }

    [Fact]
    public void AsignarSucursal_Primera_DebeSerPrincipal()
    {
        var usuario = CrearUsuarioActivo();
        var sucursalId = Guid.NewGuid();

        usuario.AsignarSucursal(sucursalId);

        usuario.Sucursales.Should().ContainSingle();
        usuario.Sucursales[0].EsPrincipal.Should().BeTrue();
    }

    [Fact]
    public void AsignarSucursal_Segunda_NoDebeSerPrincipal()
    {
        var usuario = CrearUsuarioActivo();
        usuario.AsignarSucursal(Guid.NewGuid());

        var sucursal2 = Guid.NewGuid();
        usuario.AsignarSucursal(sucursal2);

        usuario.Sucursales.Should().HaveCount(2);
        usuario.Sucursales.Single(s => s.SucursalId == sucursal2).EsPrincipal.Should().BeFalse();
    }

    [Fact]
    public void AsignarSucursal_Duplicada_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();
        var sucursalId = Guid.NewGuid();
        usuario.AsignarSucursal(sucursalId);

        var result = usuario.AsignarSucursal(sucursalId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void RemoverSucursal_UnicaSucursal_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();
        var sucursalId = Guid.NewGuid();
        usuario.AsignarSucursal(sucursalId);

        var result = usuario.RemoverSucursal(sucursalId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("al menos una sucursal");
    }

    [Fact]
    public void RemoverSucursal_Principal_DebeAsignarNuevaPrincipal()
    {
        var usuario = CrearUsuarioActivo();
        var suc1 = Guid.NewGuid();
        var suc2 = Guid.NewGuid();
        usuario.AsignarSucursal(suc1);
        usuario.AsignarSucursal(suc2);

        usuario.RemoverSucursal(suc1);

        usuario.Sucursales.Should().ContainSingle();
        usuario.Sucursales[0].EsPrincipal.Should().BeTrue();
    }

    [Fact]
    public void CambiarSucursalPrincipal_DebeActualizarPrincipal()
    {
        var usuario = CrearUsuarioActivo();
        var suc1 = Guid.NewGuid();
        var suc2 = Guid.NewGuid();
        usuario.AsignarSucursal(suc1);
        usuario.AsignarSucursal(suc2);

        var result = usuario.CambiarSucursalPrincipal(suc2);

        result.IsSuccess.Should().BeTrue();
        usuario.Sucursales.Single(s => s.SucursalId == suc2).EsPrincipal.Should().BeTrue();
        usuario.Sucursales.Single(s => s.SucursalId == suc1).EsPrincipal.Should().BeFalse();
    }
}
