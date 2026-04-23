using CoreTemplate.Email.Abstractions;

namespace CoreTemplate.Modules.EmailTemplates.Application.Abstractions;

public record EnviarConPlantillaRequest(
    string CodigoTemplate,
    string Para,
    Dictionary<string, string> Variables,
    string? NombreDestinatario = null,
    IEnumerable<string>? CC = null,
    IEnumerable<EmailAdjunto>? Adjuntos = null);

public interface IEmailTemplateSender
{
    Task<EmailResult> EnviarAsync(EnviarConPlantillaRequest request, CancellationToken ct = default);
}

public record TemplateRenderResult(string AsuntoRenderizado, string CuerpoRenderizado);

public interface ITemplateRenderer
{
    Task<TemplateRenderResult> RenderizarAsync(
        string asunto,
        string cuerpoHtml,
        string layoutHtml,
        bool usarLayout,
        Dictionary<string, string> variables);
}
