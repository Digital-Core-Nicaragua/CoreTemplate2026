using CoreTemplate.Email.Abstractions;
using CoreTemplate.Logging.Abstractions;
using CoreTemplate.Modules.EmailTemplates.Application.Abstractions;
using CoreTemplate.Modules.EmailTemplates.Domain.Repositories;
using CoreTemplate.SharedKernel.Abstractions;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IEmailTemplateSender"/>.
/// Orquesta la resolución de plantilla, el renderizado de variables y el envío.
/// <para>
/// Jerarquía de resolución de plantilla:
/// <list type="number">
/// <item>BD — plantilla del tenant actual (personalizada por empresa)</item>
/// <item>BD — plantilla global del sistema (TenantId = null, usa IgnoreQueryFilters)</item>
/// <item>Archivo .html del proyecto (fallback final via FallbackTemplateLoader)</item>
/// </list>
/// </para>
/// <para>
/// Jerarquía de resolución del layout (sistema.layout):
/// <list type="number">
/// <item>BD — layout del tenant actual</item>
/// <item>BD — layout global del sistema</item>
/// <item>Archivo sistema-layout.html del proyecto</item>
/// </list>
/// </para>
/// </summary>

internal sealed class EmailTemplateSender(
    IEmailTemplateRepository repo,
    ITemplateRenderer renderer,
    IEmailSender emailSender,
    ICurrentTenant currentTenant,
    FallbackTemplateLoader fallback,
    IAppLogger logger) : IEmailTemplateSender
{
    private readonly IAppLogger _logger = logger.ForContext<EmailTemplateSender>();

    public async Task<EmailResult> EnviarAsync(EnviarConPlantillaRequest request, CancellationToken ct = default)
    {
        // 1. Resolver plantilla: BD (tenant) → BD (global) → archivo fallback
        var template = await repo.ObtenerPorCodigoAsync(request.CodigoTemplate, currentTenant.TenantId, ct)
                    ?? await repo.ObtenerPorCodigoAsync(request.CodigoTemplate, null, ct);

        string asunto, cuerpoHtml;
        bool usarLayout;

        if (template is not null && template.EsActivo)
        {
            asunto = template.Asunto;
            cuerpoHtml = template.CuerpoHtml;
            usarLayout = template.UsarLayout;
        }
        else
        {
            var fallbackContent = fallback.Cargar(request.CodigoTemplate);
            if (fallbackContent is null)
            {
                _logger.Warning("Plantilla '{Codigo}' no encontrada en BD ni en archivos.", request.CodigoTemplate);
                return EmailResult.Fallo($"Plantilla '{request.CodigoTemplate}' no encontrada.");
            }
            asunto = fallbackContent.Asunto;
            cuerpoHtml = fallbackContent.CuerpoHtml;
            usarLayout = true;
        }

        // 2. Resolver layout
        var layoutHtml = string.Empty;
        if (usarLayout)
        {
            var layoutTemplate = await repo.ObtenerPorCodigoAsync("sistema.layout", null, ct);
            layoutHtml = layoutTemplate?.CuerpoHtml ?? fallback.Cargar("sistema.layout")?.CuerpoHtml ?? string.Empty;
        }

        // 3. Renderizar
        var rendered = await renderer.RenderizarAsync(asunto, cuerpoHtml, layoutHtml, usarLayout, request.Variables);

        // 4. Enviar
        var mensaje = new EmailMessage(
            request.Para,
            rendered.AsuntoRenderizado,
            rendered.CuerpoRenderizado,
            request.NombreDestinatario,
            request.CC,
            request.Adjuntos);

        var result = await emailSender.EnviarAsync(mensaje, ct);

        if (!result.Exitoso)
            _logger.Warning("Fallo al enviar plantilla '{Codigo}' a {Para}: {Error}",
                request.CodigoTemplate, request.Para, result.MensajeError ?? string.Empty);

        return result;
    }
}
