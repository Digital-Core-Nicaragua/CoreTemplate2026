using CoreTemplate.Modules.PdfTemplates.Application.Commands;
using CoreTemplate.Modules.PdfTemplates.Application.DTOs;
using CoreTemplate.Modules.PdfTemplates.Domain.Repositories;
using CoreTemplate.Pdf.Abstractions;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Abstractions;
using MediatR;

namespace CoreTemplate.Modules.PdfTemplates.Application.Queries;

// ─── Listar ───────────────────────────────────────────────────────────────────

public record GetPdfPlantillasQuery(string? Modulo = null, bool? SoloActivos = null)
    : IRequest<Result<IReadOnlyList<PdfPlantillaDto>>>;

internal sealed class GetPdfPlantillasHandler(IPdfPlantillaRepository repo)
    : IRequestHandler<GetPdfPlantillasQuery, Result<IReadOnlyList<PdfPlantillaDto>>>
{
    public async Task<Result<IReadOnlyList<PdfPlantillaDto>>> Handle(GetPdfPlantillasQuery q, CancellationToken ct)
    {
        var lista = await repo.ListarAsync(q.Modulo, q.SoloActivos, ct);
        return Result<IReadOnlyList<PdfPlantillaDto>>.Success(
            lista.Select(p => p.ToDto()).ToList());
    }
}

// ─── Por ID ───────────────────────────────────────────────────────────────────

public record GetPdfPlantillaByIdQuery(Guid Id) : IRequest<Result<PdfPlantillaDto>>;

internal sealed class GetPdfPlantillaByIdHandler(IPdfPlantillaRepository repo)
    : IRequestHandler<GetPdfPlantillaByIdQuery, Result<PdfPlantillaDto>>
{
    public async Task<Result<PdfPlantillaDto>> Handle(GetPdfPlantillaByIdQuery q, CancellationToken ct)
    {
        var p = await repo.ObtenerPorIdAsync(q.Id, ct);
        return p is null
            ? Result<PdfPlantillaDto>.Failure("Plantilla no encontrada.")
            : Result<PdfPlantillaDto>.Success(p.ToDto());
    }
}

// ─── Diseños disponibles ──────────────────────────────────────────────────────

public record GetDiseniosDisponiblesQuery : IRequest<Result<IReadOnlyList<DisenioDisponibleDto>>>;

internal sealed class GetDiseniosDisponiblesHandler(IPdfTemplateFactory factory)
    : IRequestHandler<GetDiseniosDisponiblesQuery, Result<IReadOnlyList<DisenioDisponibleDto>>>
{
    public Task<Result<IReadOnlyList<DisenioDisponibleDto>>> Handle(
        GetDiseniosDisponiblesQuery q, CancellationToken ct)
    {
        var disenios = factory.ObtenerTodos()
            .Select(t => new DisenioDisponibleDto(t.Codigo, t.Nombre, t.Descripcion, t.Orientacion))
            .ToList();

        return Task.FromResult(
            Result<IReadOnlyList<DisenioDisponibleDto>>.Success(disenios));
    }
}

// ─── Preview ──────────────────────────────────────────────────────────────────

public record PreviewPdfQuery(Guid Id, Dictionary<string, object> Datos)
    : IRequest<Result<byte[]>>;

internal sealed class PreviewPdfHandler(
    IPdfPlantillaRepository repo,
    IPdfTemplateFactory factory,
    IDateTimeProvider dateTime)
    : IRequestHandler<PreviewPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(PreviewPdfQuery q, CancellationToken ct)
    {
        var plantilla = await repo.ObtenerPorIdAsync(q.Id, ct);
        if (plantilla is null) return Result<byte[]>.Failure("Plantilla no encontrada.");

        IPdfDocumentTemplate template;
        try { template = factory.Resolver(plantilla.CodigoTemplate); }
        catch (InvalidOperationException ex) { return Result<byte[]>.Failure(ex.Message); }

        var plantillaData = plantilla.ToPlantillaData("CoreTemplate", dateTime.UtcNow);
        var contenido = new DiccionarioContent(q.Datos);
        var pdfBytes = template.Generar(plantillaData, contenido);

        return Result<byte[]>.Success(pdfBytes);
    }
}

// ─── Contenido de diccionario para preview ────────────────────────────────────

internal sealed class DiccionarioContent(Dictionary<string, object> datos) : IPdfContent
{
    public Dictionary<string, object> ObtenerDatos() => datos;
}

// ─── Extensión de mapeo a PdfPlantillaData ────────────────────────────────────
// Ver PdfPlantillaMappingExtensions.cs
