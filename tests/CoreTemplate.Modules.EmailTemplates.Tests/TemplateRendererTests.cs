using CoreTemplate.Modules.EmailTemplates.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace CoreTemplate.Modules.EmailTemplates.Tests;

public class TemplateRendererTests
{
    private readonly TemplateRenderer _renderer;

    public TemplateRendererTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:Nombre"] = "CoreTemplate",
                ["AppSettings:Url"] = "https://localhost:5001",
                ["AppSettings:LogoUrl"] = ""
            })
            .Build();

        _renderer = new TemplateRenderer(config);
    }

    [Fact]
    public async Task Renderizar_ReemplazaVariablesEnAsunto()
    {
        var result = await _renderer.RenderizarAsync(
            "Bienvenido a {{SistemaNombre}}",
            "<p>Hola</p>", string.Empty, false,
            new Dictionary<string, string>());

        result.AsuntoRenderizado.Should().Be("Bienvenido a CoreTemplate");
    }

    [Fact]
    public async Task Renderizar_ReemplazaVariablesPersonalizadas()
    {
        var result = await _renderer.RenderizarAsync(
            "Hola {{NombreUsuario}}",
            "<p>Tu token: {{Token}}</p>", string.Empty, false,
            new Dictionary<string, string>
            {
                ["NombreUsuario"] = "Juan",
                ["Token"] = "ABC123"
            });

        result.AsuntoRenderizado.Should().Be("Hola Juan");
        result.CuerpoRenderizado.Should().Contain("ABC123");
    }

    [Fact]
    public async Task Renderizar_ConLayout_EnvuelveCuerpoEnLayout()
    {
        var layout = "<html><body>{{Contenido}}</body></html>";

        var result = await _renderer.RenderizarAsync(
            "Asunto", "<p>Contenido del correo</p>",
            layout, true,
            new Dictionary<string, string>());

        result.CuerpoRenderizado.Should().Contain("<html>");
        result.CuerpoRenderizado.Should().Contain("Contenido del correo");
    }

    [Fact]
    public async Task Renderizar_SinLayout_RetornaCuerpoDirecto()
    {
        var result = await _renderer.RenderizarAsync(
            "Asunto", "<p>Solo el cuerpo</p>",
            string.Empty, false,
            new Dictionary<string, string>());

        result.CuerpoRenderizado.Should().Be("<p>Solo el cuerpo</p>");
    }

    [Fact]
    public async Task Renderizar_InyectaVariablesGlobalesAutomaticamente()
    {
        var result = await _renderer.RenderizarAsync(
            "{{SistemaNombre}} — {{AnioActual}}",
            "<p>{{SistemaUrl}}</p>", string.Empty, false,
            new Dictionary<string, string>());

        result.AsuntoRenderizado.Should().Contain("CoreTemplate");
        result.AsuntoRenderizado.Should().Contain(DateTime.UtcNow.Year.ToString());
        result.CuerpoRenderizado.Should().Contain("https://localhost:5001");
    }
}
