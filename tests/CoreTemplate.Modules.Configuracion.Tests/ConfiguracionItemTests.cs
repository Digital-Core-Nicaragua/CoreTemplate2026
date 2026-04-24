using CoreTemplate.Modules.Configuracion.Domain.Aggregates;
using CoreTemplate.Modules.Configuracion.Domain.Enums;

namespace CoreTemplate.Modules.Configuracion.Tests;

public class ConfiguracionItemTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var result = ConfiguracionItem.Crear(
            "sistema.nombre", "Mi Sistema", TipoValor.String,
            "Nombre del sistema", "Sistema");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Clave.Should().Be("sistema.nombre");
        result.Value.Valor.Should().Be("Mi Sistema");
        result.Value.EsEditable.Should().BeTrue();
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Crear_SinClave_RetornaFallo()
    {
        var result = ConfiguracionItem.Crear(
            "", "valor", TipoValor.String, "desc", "grupo");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_ClavesSinPunto_RetornaFallo()
    {
        var result = ConfiguracionItem.Crear(
            "sinpunto", "valor", TipoValor.String, "desc", "grupo");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("formato");
    }

    [Fact]
    public void Crear_ClaveEnMinusculas()
    {
        var result = ConfiguracionItem.Crear(
            "SISTEMA.NOMBRE", "valor", TipoValor.String, "desc", "grupo");

        result.Value!.Clave.Should().Be("sistema.nombre");
    }

    [Fact]
    public void Actualizar_ItemEditable_ActualizaValor()
    {
        var item = ConfiguracionItem.Crear(
            "sistema.nombre", "Valor original", TipoValor.String,
            "desc", "grupo").Value!;
        item.ClearDomainEvents();

        var result = item.Actualizar("Nuevo valor", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        item.Valor.Should().Be("Nuevo valor");
        item.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Actualizar_ItemNoEditable_RetornaFallo()
    {
        var item = ConfiguracionItem.Crear(
            "sistema.nombre", "valor", TipoValor.String,
            "desc", "grupo", esEditable: false).Value!;

        var result = item.Actualizar("nuevo", Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("editable");
    }

    [Fact]
    public void Actualizar_RegistraModificadoPor()
    {
        var item = ConfiguracionItem.Crear(
            "sistema.nombre", "valor", TipoValor.String,
            "desc", "grupo").Value!;
        var userId = Guid.NewGuid();

        item.Actualizar("nuevo", userId);

        item.ModificadoPor.Should().Be(userId);
        item.ModificadoEn.Should().NotBeNull();
    }

    [Fact]
    public void Crear_ConTenantId_AsignaTenantCorrectamente()
    {
        var tenantId = Guid.NewGuid();
        var result = ConfiguracionItem.Crear(
            "sistema.nombre", "Empresa A", TipoValor.String,
            "desc", "grupo", tenantId: tenantId);

        result.Value!.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Crear_SinTenantId_EsParametroGlobal()
    {
        var result = ConfiguracionItem.Crear(
            "sistema.nombre", "valor", TipoValor.String,
            "desc", "grupo");

        result.Value!.TenantId.Should().BeNull();
    }
}
