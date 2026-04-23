using CoreTemplate.Modules.EmailTemplates.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Services;

/// <summary>
/// Renderiza plantillas de correo reemplazando variables {{Variable}} por sus valores.
/// <para>
/// Variables globales inyectadas automáticamente en cada renderizado
/// (no es necesario pasarlas en el diccionario de variables):
/// <list type="bullet">
/// <item><c>{{SistemaNombre}}</c> — de AppSettings:Nombre</item>
/// <item><c>{{SistemaUrl}}</c> — de AppSettings:Url</item>
/// <item><c>{{SistemaLogoUrl}}</c> — de AppSettings:LogoUrl</item>
/// <item><c>{{AnioActual}}</c> — año actual UTC</item>
/// <item><c>{{FechaActual}}</c> — fecha actual UTC formateada dd/MM/yyyy</item>
/// </list>
/// </para>
/// <para>
/// Si <c>usarLayout = true</c>, el cuerpo renderizado se inserta en
/// <c>{{Contenido}}</c> del layout antes de retornar el HTML final.
/// </para>
/// </summary>

internal sealed class TemplateRenderer(IConfiguration config) : ITemplateRenderer
{
    public Task<TemplateRenderResult> RenderizarAsync(
        string asunto,
        string cuerpoHtml,
        string layoutHtml,
        bool usarLayout,
        Dictionary<string, string> variables)
    {
        var vars = new Dictionary<string, string>(variables, StringComparer.OrdinalIgnoreCase)
        {
            ["SistemaNombre"] = config["AppSettings:Nombre"] ?? "Sistema",
            ["SistemaUrl"] = config["AppSettings:Url"] ?? string.Empty,
            ["SistemaLogoUrl"] = config["AppSettings:LogoUrl"] ?? string.Empty,
            ["AnioActual"] = DateTime.UtcNow.Year.ToString(),
            ["FechaActual"] = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };

        var asuntoRenderizado = Reemplazar(asunto, vars);
        var cuerpoRenderizado = Reemplazar(cuerpoHtml, vars);

        if (usarLayout && !string.IsNullOrWhiteSpace(layoutHtml))
        {
            vars["Contenido"] = cuerpoRenderizado;
            cuerpoRenderizado = Reemplazar(layoutHtml, vars);
        }

        return Task.FromResult(new TemplateRenderResult(asuntoRenderizado, cuerpoRenderizado));
    }

    private static string Reemplazar(string template, Dictionary<string, string> vars)
    {
        foreach (var (key, value) in vars)
            template = template.Replace($"{{{{{key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        return template;
    }
}
