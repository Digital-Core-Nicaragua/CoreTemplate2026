using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using CoreTemplate.Pdf.Abstractions;
using CoreTemplate.Pdf.Services;
using CoreTemplate.Pdf.Templates;

namespace CoreTemplate.Modules.PdfTemplates.Tests;

public class PdfPlantillaTests
{
    private static PdfPlantilla CrearPlantilla(Guid? tenantId = null) =>
        PdfPlantilla.Crear(
            "nomina.comprobante-pago", "Comprobante de Pago",
            "Nomina", "vertical-estandar",
            "Mi Empresa S.A.", null,
            "#1a2e5a", "#ffffff", "#4f46e5",
            "RUC: 001-000000-0000",
            "{{NombreEmpresa}} — {{FechaGeneracion}}",
            true, true, null, false, tenantId).Value!;

    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var result = PdfPlantilla.Crear(
            "nomina.comprobante-pago", "Comprobante",
            "Nomina", "vertical-estandar",
            "Mi Empresa", null,
            "#1a2e5a", "#ffffff", "#4f46e5",
            null, null, true, true, null);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Codigo.Should().Be("nomina.comprobante-pago");
        result.Value.EsActivo.Should().BeTrue();
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Crear_SinCodigo_RetornaFallo()
    {
        var result = PdfPlantilla.Crear(
            "", "Nombre", "Modulo", "vertical-estandar",
            "Empresa", null, "#000", "#fff", "#000",
            null, null, true, true, null);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_SinCodigoTemplate_RetornaFallo()
    {
        var result = PdfPlantilla.Crear(
            "nomina.test", "Nombre", "Modulo", "",
            "Empresa", null, "#000", "#fff", "#000",
            null, null, true, true, null);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_CodigoEnMinusculas()
    {
        var result = PdfPlantilla.Crear(
            "NOMINA.COMPROBANTE", "Nombre", "Modulo", "vertical-estandar",
            "Empresa", null, "#000", "#fff", "#000",
            null, null, true, true, null);

        result.Value!.Codigo.Should().Be("nomina.comprobante");
        result.Value.CodigoTemplate.Should().Be("vertical-estandar");
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaYPublicaEvento()
    {
        var plantilla = CrearPlantilla();
        plantilla.ClearDomainEvents();

        var result = plantilla.Actualizar(
            "Nuevo nombre", "moderno", "Nueva Empresa", null,
            "#2a3e6a", "#ffffff", "#5f56e5",
            null, null, true, true, null, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        plantilla.NombreEmpresa.Should().Be("Nueva Empresa");
        plantilla.CodigoTemplate.Should().Be("moderno");
        plantilla.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Desactivar_PlantillaActiva_DesactivaCorrectamente()
    {
        var plantilla = CrearPlantilla();
        var result = plantilla.Desactivar();

        result.IsSuccess.Should().BeTrue();
        plantilla.EsActivo.Should().BeFalse();
    }

    [Fact]
    public void Activar_PlantillaInactiva_ActivaCorrectamente()
    {
        var plantilla = CrearPlantilla();
        plantilla.Desactivar();
        plantilla.ClearDomainEvents();

        var result = plantilla.Activar();

        result.IsSuccess.Should().BeTrue();
        plantilla.EsActivo.Should().BeTrue();
        plantilla.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Crear_ConTenantId_AsignaTenantCorrectamente()
    {
        var tenantId = Guid.NewGuid();
        var plantilla = CrearPlantilla(tenantId);

        plantilla.TenantId.Should().Be(tenantId);
    }
}

public class PdfTemplateFactoryTests
{
    private readonly PdfTemplateFactory _factory;

    public PdfTemplateFactoryTests()
    {
        var templates = new List<IPdfDocumentTemplate>
        {
            new VerticalEstandarTemplate(),
            new HorizontalEstandarTemplate(),
            new CompactoTemplate(),
            new ModernoTemplate()
        };
        _factory = new PdfTemplateFactory(templates);
    }

    [Theory]
    [InlineData("vertical-estandar")]
    [InlineData("horizontal-estandar")]
    [InlineData("compacto")]
    [InlineData("moderno")]
    public void Resolver_DisenioRegistrado_RetornaTemplate(string codigo)
    {
        var template = _factory.Resolver(codigo);
        template.Should().NotBeNull();
        template.Codigo.Should().Be(codigo);
    }

    [Fact]
    public void Resolver_DisenioNoRegistrado_LanzaExcepcion()
    {
        var act = () => _factory.Resolver("disenio-inexistente");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*disenio-inexistente*");
    }

    [Fact]
    public void ObtenerTodos_Retorna4DiseniosBase()
    {
        var todos = _factory.ObtenerTodos();
        todos.Should().HaveCount(4);
    }

    [Fact]
    public void Resolver_EsCaseInsensitive()
    {
        var template = _factory.Resolver("VERTICAL-ESTANDAR");
        template.Codigo.Should().Be("vertical-estandar");
    }
}

public class PdfPlantillaDataTests
{
    [Fact]
    public void TextoPiePaginaRenderizado_ReemplazaVariables()
    {
        var data = new PdfPlantillaData(
            "Empresa ABC", null, "#000", "#fff", "#000",
            null, "{{NombreEmpresa}} — {{FechaGeneracion}}",
            true, true, null, "CoreTemplate",
            new DateTime(2025, 1, 15, 10, 30, 0));

        data.TextoPiePaginaRenderizado.Should().Contain("Empresa ABC");
        data.TextoPiePaginaRenderizado.Should().Contain("15/01/2025");
    }

    [Fact]
    public void TextoPiePaginaRenderizado_SinTexto_RetornaCadenaVacia()
    {
        var data = new PdfPlantillaData(
            "Empresa", null, "#000", "#fff", "#000",
            null, null, true, true, null,
            "Sistema", DateTime.UtcNow);

        data.TextoPiePaginaRenderizado.Should().BeEmpty();
    }
}
