using CoreTemplate.Modules.EmailTemplates.Application.Abstractions;
using CoreTemplate.Modules.EmailTemplates.Application.Commands;
using CoreTemplate.Modules.EmailTemplates.Application.DTOs;
using CoreTemplate.Modules.EmailTemplates.Domain.Repositories;
using CoreTemplate.SharedKernel;
using MediatR;

namespace CoreTemplate.Modules.EmailTemplates.Application.Queries;

// ─── Listar ───────────────────────────────────────────────────────────────────

public record GetPlantillasQuery(string? Modulo = null, bool? SoloActivos = null)
    : IRequest<Result<IReadOnlyList<EmailTemplateDto>>>;

internal sealed class GetPlantillasHandler(IEmailTemplateRepository repo)
    : IRequestHandler<GetPlantillasQuery, Result<IReadOnlyList<EmailTemplateDto>>>
{
    public async Task<Result<IReadOnlyList<EmailTemplateDto>>> Handle(GetPlantillasQuery q, CancellationToken ct)
    {
        var lista = await repo.ListarAsync(q.Modulo, q.SoloActivos, ct);
        return Result<IReadOnlyList<EmailTemplateDto>>.Success(lista.Select(t => t.ToDto()).ToList());
    }
}

// ─── Por ID ───────────────────────────────────────────────────────────────────

public record GetPlantillaByIdQuery(Guid Id) : IRequest<Result<EmailTemplateDto>>;

internal sealed class GetPlantillaByIdHandler(IEmailTemplateRepository repo)
    : IRequestHandler<GetPlantillaByIdQuery, Result<EmailTemplateDto>>
{
    public async Task<Result<EmailTemplateDto>> Handle(GetPlantillaByIdQuery q, CancellationToken ct)
    {
        var template = await repo.ObtenerPorIdAsync(q.Id, ct);
        return template is null
            ? Result<EmailTemplateDto>.Failure("Plantilla no encontrada.")
            : Result<EmailTemplateDto>.Success(template.ToDto());
    }
}

// ─── Preview ──────────────────────────────────────────────────────────────────

public record PreviewPlantillaQuery(Guid Id, Dictionary<string, string> Variables)
    : IRequest<Result<PreviewResultDto>>;

internal sealed class PreviewPlantillaHandler(
    IEmailTemplateRepository repo,
    ITemplateRenderer renderer,
    IEmailTemplateRepository templateRepo)
    : IRequestHandler<PreviewPlantillaQuery, Result<PreviewResultDto>>
{
    public async Task<Result<PreviewResultDto>> Handle(PreviewPlantillaQuery q, CancellationToken ct)
    {
        var template = await repo.ObtenerPorIdAsync(q.Id, ct);
        if (template is null) return Result<PreviewResultDto>.Failure("Plantilla no encontrada.");

        var layout = string.Empty;
        if (template.UsarLayout)
        {
            var layoutTemplate = await templateRepo.ObtenerPorCodigoAsync("sistema.layout", null, ct);
            layout = layoutTemplate?.CuerpoHtml ?? string.Empty;
        }

        var rendered = await renderer.RenderizarAsync(
            template.Asunto, template.CuerpoHtml, layout, template.UsarLayout, q.Variables);

        return Result<PreviewResultDto>.Success(
            new PreviewResultDto(rendered.AsuntoRenderizado, rendered.CuerpoRenderizado));
    }
}
