using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using CoreTemplate.Pdf.Abstractions;

namespace CoreTemplate.Modules.PdfTemplates.Application;

/// <summary>
/// Extensiones de mapeo compartidas entre Application e Infrastructure.
/// </summary>
public static class PdfPlantillaMappingExtensions
{
    public static PdfPlantillaData ToPlantillaData(
        this PdfPlantilla p, string sistemaNombre, DateTime fechaGeneracion) =>
        new(p.NombreEmpresa, p.LogoUrl, p.ColorEncabezado, p.ColorTextoHeader, p.ColorAcento,
            p.TextoSecundario, p.TextoPiePagina, p.MostrarNumeroPagina, p.MostrarFechaGeneracion,
            p.MarcaDeAgua, sistemaNombre, fechaGeneracion);
}
