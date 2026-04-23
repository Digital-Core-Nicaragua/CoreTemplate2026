namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Services;

public record FallbackTemplate(string Asunto, string CuerpoHtml);

/// <summary>
/// Carga plantillas HTML desde archivos embebidos cuando no existe una versión en BD.
/// Los archivos viven en Infrastructure/Templates/{codigo}.html
/// El asunto se lee de la primera línea del archivo con formato: <!-- Asunto: texto -->
/// </summary>
internal sealed class FallbackTemplateLoader
{
    private readonly string _templatesPath;

    public FallbackTemplateLoader()
    {
        _templatesPath = Path.Combine(
            AppContext.BaseDirectory, "Templates", "Email");
    }

    public FallbackTemplate? Cargar(string codigo)
    {
        var fileName = $"{codigo.Replace('.', '-')}.html";
        var filePath = Path.Combine(_templatesPath, fileName);

        if (!File.Exists(filePath)) return null;

        var contenido = File.ReadAllText(filePath);
        var asunto = ExtraerAsunto(contenido, codigo);
        return new FallbackTemplate(asunto, contenido);
    }

    private static string ExtraerAsunto(string html, string codigo)
    {
        // Busca <!-- Asunto: Mi asunto aquí --> en la primera línea
        var linea = html.Split('\n').FirstOrDefault() ?? string.Empty;
        if (linea.Contains("<!-- Asunto:") && linea.Contains("-->"))
        {
            var inicio = linea.IndexOf("<!-- Asunto:") + 12;
            var fin = linea.IndexOf("-->", inicio);
            if (fin > inicio) return linea[inicio..fin].Trim();
        }
        return codigo;
    }
}
