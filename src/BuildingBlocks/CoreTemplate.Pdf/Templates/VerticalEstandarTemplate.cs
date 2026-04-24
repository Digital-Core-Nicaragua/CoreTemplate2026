using CoreTemplate.Pdf.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoreTemplate.Pdf.Templates;

/// <summary>
/// Diseño A4 vertical clásico.
/// Encabezado con logo y nombre de empresa, área de contenido con tabla de datos,
/// pie de página con texto y numeración.
/// Ideal para: comprobantes de pago, facturas, constancias, contratos.
/// </summary>
public sealed class VerticalEstandarTemplate : IPdfDocumentTemplate
{
    public string Codigo => "vertical-estandar";
    public string Nombre => "Vertical Estándar";
    public string Descripcion => "A4 vertical. Encabezado con logo, tabla de datos, pie de página. Ideal para comprobantes, facturas y constancias.";
    public string Orientacion => "Vertical";

    public byte[] Generar(PdfPlantillaData plantilla, IPdfContent contenido)
    {
        var datos = contenido.ObtenerDatos();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(h => RenderEncabezado(h, plantilla));
                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(6);
                    RenderMarcaDeAgua(col, plantilla);
                    RenderDatos(col, plantilla, datos);
                });
                page.Footer().Element(f => RenderPie(f, plantilla));
            });
        }).GeneratePdf();
    }

    private static void RenderEncabezado(IContainer header, PdfPlantillaData p)
    {
        header.Background(p.ColorEncabezado).Padding(12).Row(row =>
        {
            if (!string.IsNullOrWhiteSpace(p.LogoUrl) && File.Exists(p.LogoUrl))
                row.ConstantItem(80).AlignMiddle()
                    .Image(p.LogoUrl).WithRasterDpi(300);

            row.RelativeItem().AlignMiddle().PaddingLeft(10).Column(col =>
            {
                col.Item().Text(p.NombreEmpresa)
                    .FontColor(p.ColorTextoHeader).FontSize(15).Bold();

                if (!string.IsNullOrWhiteSpace(p.TextoSecundario))
                    col.Item().Text(p.TextoSecundario)
                        .FontColor(p.ColorTextoHeader).FontSize(9);
            });
        });
    }

    private static void RenderDatos(ColumnDescriptor col, PdfPlantillaData p, Dictionary<string, object> datos)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(160);
                c.RelativeColumn();
            });

            foreach (var (clave, valor) in datos)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                    .Padding(6).Text(clave).SemiBold().FontSize(10);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                    .Padding(6).Text(valor?.ToString() ?? string.Empty).FontSize(10);
            }
        });
    }

    private static void RenderMarcaDeAgua(ColumnDescriptor col, PdfPlantillaData p)
    {
        if (string.IsNullOrWhiteSpace(p.MarcaDeAgua)) return;

        col.Item().AlignCenter().Text(p.MarcaDeAgua)
            .FontSize(48).Bold()
            .FontColor(Colors.Grey.Lighten2);
    }

    private static void RenderPie(IContainer footer, PdfPlantillaData p)
    {
        footer.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text(p.TextoPiePaginaRenderizado)
                .FontSize(8).FontColor(Colors.Grey.Darken1);

            if (p.MostrarNumeroPagina)
                row.ConstantItem(80).AlignRight().Text(t =>
                {
                    t.Span("Pág. ").FontSize(8);
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" / ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
        });
    }
}
