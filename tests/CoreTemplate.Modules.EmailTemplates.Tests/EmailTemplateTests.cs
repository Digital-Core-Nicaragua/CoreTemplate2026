using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;

namespace CoreTemplate.Modules.EmailTemplates.Tests;

public class EmailTemplateTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var result = EmailTemplate.Crear(
            "auth.test", "Test", "Auth",
            "Asunto {{SistemaNombre}}", "<p>Hola {{NombreUsuario}}</p>",
            ["NombreUsuario"]);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Codigo.Should().Be("auth.test");
        result.Value.EsActivo.Should().BeTrue();
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Crear_SinCodigo_RetornaFallo()
    {
        var result = EmailTemplate.Crear("", "Test", "Auth", "Asunto", "<p>Body</p>");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_SinAsunto_RetornaFallo()
    {
        var result = EmailTemplate.Crear("auth.test", "Test", "Auth", "", "<p>Body</p>");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaYPublicaEvento()
    {
        var template = EmailTemplate.Crear(
            "auth.test", "Test", "Auth", "Asunto original", "<p>Body</p>").Value!;
        template.ClearDomainEvents();

        var result = template.Actualizar("Nuevo asunto", "<p>Nuevo body</p>", null, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        template.Asunto.Should().Be("Nuevo asunto");
        template.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Desactivar_PlantillaActiva_DesactivaCorrectamente()
    {
        var template = EmailTemplate.Crear(
            "auth.test", "Test", "Auth", "Asunto", "<p>Body</p>").Value!;

        var result = template.Desactivar();

        result.IsSuccess.Should().BeTrue();
        template.EsActivo.Should().BeFalse();
    }

    [Fact]
    public void Desactivar_PlantillaYaInactiva_RetornaFallo()
    {
        var template = EmailTemplate.Crear(
            "auth.test", "Test", "Auth", "Asunto", "<p>Body</p>").Value!;
        template.Desactivar();

        var result = template.Desactivar();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Activar_PlantillaInactiva_ActivaCorrectamente()
    {
        var template = EmailTemplate.Crear(
            "auth.test", "Test", "Auth", "Asunto", "<p>Body</p>").Value!;
        template.Desactivar();
        template.ClearDomainEvents();

        var result = template.Activar();

        result.IsSuccess.Should().BeTrue();
        template.EsActivo.Should().BeTrue();
        template.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Codigo_SiempreEnMinusculas()
    {
        var template = EmailTemplate.Crear(
            "AUTH.RESET-PASSWORD", "Test", "Auth", "Asunto", "<p>Body</p>").Value!;

        template.Codigo.Should().Be("auth.reset-password");
    }
}
