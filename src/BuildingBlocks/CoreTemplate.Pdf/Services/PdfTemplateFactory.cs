using CoreTemplate.Pdf.Abstractions;

namespace CoreTemplate.Pdf.Services;

/// <summary>
/// Resuelve el IPdfDocumentTemplate correcto según el CodigoTemplate.
/// Todos los diseños se registran en DI como IEnumerable&lt;IPdfDocumentTemplate&gt;.
/// Si se solicita un código no registrado → InvalidOperationException (fail-fast).
/// </summary>
internal sealed class PdfTemplateFactory(IEnumerable<IPdfDocumentTemplate> templates) : IPdfTemplateFactory
{
    private readonly Dictionary<string, IPdfDocumentTemplate> _templates =
        templates.ToDictionary(t => t.Codigo, StringComparer.OrdinalIgnoreCase);

    public IPdfDocumentTemplate Resolver(string codigoTemplate)
    {
        if (_templates.TryGetValue(codigoTemplate, out var template))
            return template;

        throw new InvalidOperationException(
            $"Diseño de PDF '{codigoTemplate}' no está registrado. " +
            $"Diseños disponibles: {string.Join(", ", _templates.Keys)}. " +
            $"Ver guía: docs/PdfTemplates/03-Guias/01-AGREGAR-NUEVO-DISENIO.md");
    }

    public IReadOnlyList<IPdfDocumentTemplate> ObtenerTodos() =>
        _templates.Values.ToList().AsReadOnly();
}
