using CoreTemplate.Pdf.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoreTemplate.Pdf.Templates;

/// <summary>
/// Diseño moderno con banda lateral de color corporativo.
/// Encabezado limpio con logo a la derecha, banda vertical izquierda del color de acento.
/// Ideal para: documentos formales, contratos, reportes ejecutivos.
/// </summary>
public sealed class ModernoTemplate : IPdfDocumentTemplate
{
    public string Codigo => "moderno";
    public string Nombre => "Moderno";
    public string Descripcion => "A4 vertical con banda lateral de color corporativo. Ideal para contratos y documentos formales.";
    public string Orientacion => "Vertical";

    public byte[] Generar(PdfPlantillaData plantilla, IPdfContent contenido)
    {
        var datos = contenido.ObtenerDatos();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(1.5f, Unit.Centimetre);
                page.MarginBottom(1.5f, Unit.Centimetre);
                page.MarginLeft(0, Unit.Centimetre);
                page.MarginRight(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(h => RenderEncabezado(h, plantilla));
                page.Content().Row(row =>
                {
                    // Banda lateral izquierda
                    row.ConstantItem(12).Background(plantilla.ColorAcento);

                    // Contenido principal
                    row.RelativeItem().PaddingLeft(15).PaddingTop(10).Column(col =>
                    {
                        col.Spacing(8);
                        RenderDatos(col, plantilla, datos);
                    });
                });
                page.Footer().Element(f => RenderPie(f, plantilla));
            });
        }).GeneratePdf();
    }

    private static void RenderEncabezado(IContainer header, PdfPlantillaData p)
    {
        header.Row(row =>
        {
            // Banda lateral en encabezado
            row.ConstantItem(12).Background(p.ColorEncabezado);

            row.RelativeItem().PaddingLeft(15).PaddingBottom(10)
                .BorderBottom(2).BorderColor(p.ColorEncabezado).Row(innerRow =>
                {
                    innerRow.RelativeItem().AlignMiddle().Column(col =>
                    {
                        col.Item().Text(p.NombreEmpresa).FontSize(18).Bold()
                            .FontColor(p.ColorEncabezado);

                        if (!string.IsNullOrWhiteSpace(p.TextoSecundario))
                            col.Item().Text(p.TextoSecundario).FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                    });

                    if (!string.IsNullOrWhiteSpace(p.LogoUrl) && File.Exists(p.LogoUrl))
                        innerRow.ConstantItem(90).AlignRight().AlignMiddle()
                            .Image(p.LogoUrl).WithRasterDpi(300);
                });
        });
    }

    private static void RenderDatos(ColumnDescriptor col, PdfPlantillaData p, Dictionary<string, object> datos)
    {
        foreach (var (clave, valor) in datos)
        {
            col.Item().Column(inner =>
            {
                inner.Item().Text(clave).FontSize(9).SemiBold()
                    .FontColor(p.ColorAcento);
                inner.Item().Text(valor?.ToString() ?? string.Empty).FontSize(11);
                inner.Item().PaddingTop(2).BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten3).Height(1);
            });
        }
    }

    private static void RenderPie(IContainer footer, PdfPlantillaData p)
    {
        footer.Row(row =>
        {
            row.ConstantItem(12).Background(p.ColorAcento);
            row.RelativeItem().PaddingLeft(15).PaddingTop(5)
                .BorderTop(1).BorderColor(Colors.Grey.Lighten2).Row(innerRow =>
                {
                    innerRow.RelativeItem().Text(p.TextoPiePaginaRenderizado)
                        .FontSize(8).FontColor(Colors.Grey.Darken1);

                    if (p.MostrarNumeroPagina)
                        innerRow.ConstantItem(80).AlignRight().Text(t =>
                        {
                            t.Span("Pág. ").FontSize(8);
                            t.CurrentPageNumber().FontSize(8);
                            t.Span(" / ").FontSize(8);
                            t.TotalPages().FontSize(8);
                        });
                });
        });
    }
}
