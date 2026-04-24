using CoreTemplate.Pdf.Abstractions;

namespace CoreTemplate.Modules.PdfTemplates.Application.Abstractions;

/// <summary>
/// Servicio público para generar PDFs desde cualquier módulo consumidor.
/// Resuelve la plantilla de BD (tenant → global) y el diseño en código,
/// luego genera el PDF con QuestPDF.
/// </summary>
public interface IModuloPdfGenerator
{
    Task<byte[]> GenerarAsync(
        string codigoPlantilla,
        Guid? tenantId,
        IPdfContent contenido,
        CancellationToken ct = default);
}
