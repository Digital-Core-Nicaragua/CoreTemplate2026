using CoreTemplate.Logging.Abstractions;
using CoreTemplate.Modules.PdfTemplates.Application;
using CoreTemplate.Modules.PdfTemplates.Application.Abstractions;
using CoreTemplate.Modules.PdfTemplates.Domain.Repositories;
using CoreTemplate.Pdf.Abstractions;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.Extensions.Configuration;

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure.Services;

/// <summary>
/// Implementación de IModuloPdfGenerator.
/// Resuelve la plantilla de BD (tenant → global), el diseño en código,
/// y genera el PDF con QuestPDF.
/// </summary>
internal sealed class ModuloPdfGenerator(
    IPdfPlantillaRepository repo,
    IPdfTemplateFactory factory,
    IDateTimeProvider dateTime,
    IConfiguration config,
    IAppLogger logger) : IModuloPdfGenerator
{
    private readonly IAppLogger _logger = logger.ForContext<ModuloPdfGenerator>();

    public async Task<byte[]> GenerarAsync(
        string codigoPlantilla,
        Guid? tenantId,
        IPdfContent contenido,
        CancellationToken ct = default)
    {
        // 1. Resolver plantilla: tenant → global
        var plantilla = await repo.ObtenerPorCodigoAsync(codigoPlantilla, tenantId, ct)
                     ?? await repo.ObtenerPorCodigoAsync(codigoPlantilla, null, ct);

        if (plantilla is null)
            throw new InvalidOperationException(
                $"Plantilla PDF '{codigoPlantilla}' no encontrada en BD. " +
                $"Crea la plantilla via POST /api/pdf-templates o agrega al seeder.");

        if (!plantilla.EsActivo)
            throw new InvalidOperationException(
                $"Plantilla PDF '{codigoPlantilla}' está inactiva.");

        // 2. Resolver diseño en código
        var template = factory.Resolver(plantilla.CodigoTemplate);

        // 3. Construir datos corporativos
        var sistemaNombre = config["AppSettings:Nombre"] ?? "Sistema";
        var plantillaData = plantilla.ToPlantillaData(sistemaNombre, dateTime.UtcNow);

        // 4. Generar PDF
        var pdfBytes = template.Generar(plantillaData, contenido);

        _logger.Info("PDF generado: plantilla={Codigo}, diseño={Template}, tamaño={Bytes}b",
            codigoPlantilla, plantilla.CodigoTemplate, pdfBytes.Length);

        return pdfBytes;
    }
}
