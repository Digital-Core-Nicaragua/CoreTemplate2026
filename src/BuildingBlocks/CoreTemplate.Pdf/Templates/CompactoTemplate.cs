using CoreTemplate.Pdf.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoreTemplate.Pdf.Templates;

/// <summary>
/// Diseño compacto con márgenes reducidos.
/// Encabezado pequeño, datos en dos columnas, sin pie elaborado.
/// Ideal para: recibos de pago, constancias cortas, tickets.
/// </summary>
public sealed class CompactoTemplate : IPdfDocumentTemplate
{
    public string Codigo => "compacto";
    public string Nombre => "Compacto";
    public string Descripcion => "A4 vertical con márgenes reducidos. Ideal para recibos, constancias cortas y tickets.";
    public string Orientacion => "Vertical";

    public byte[] Generar(PdfPlantillaData plantilla, IPdfContent contenido)
    {
        var datos = contenido.ObtenerDatos();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(h => RenderEncabezado(h, plantilla));
                page.Content().PaddingTop(8).Column(col =>
                {
                    col.Spacing(4);
                    RenderDatos(col, plantilla, datos);
                });
                page.Footer().Element(f => RenderPie(f, plantilla));
            });
        }).GeneratePdf();
    }

    private static void RenderEncabezado(IContainer header, PdfPlantillaData p)
    {
        header.BorderBottom(2).BorderColor(p.ColorAcento).PaddingBottom(6).Row(row =>
        {
            if (!string.IsNullOrWhiteSpace(p.LogoUrl) && File.Exists(p.LogoUrl))
                row.ConstantItem(50).AlignMiddle()
                    .Image(p.LogoUrl).WithRasterDpi(300);

            row.RelativeItem().AlignMiddle().PaddingLeft(8).Column(col =>
            {
                col.Item().Text(p.NombreEmpresa).FontSize(13).Bold();

                if (!string.IsNullOrWhiteSpace(p.TextoSecundario))
                    col.Item().Text(p.TextoSecundario).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void RenderDatos(ColumnDescriptor col, PdfPlantillaData p, Dictionary<string, object> datos)
    {
        // Dos columnas para aprovechar el espacio
        var lista = datos.ToList();
        for (int i = 0; i < lista.Count; i += 2)
        {
            col.Item().Row(row =>
            {
                // Columna izquierda
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().Text(lista[i].Key).SemiBold().FontSize(9)
                        .FontColor(p.ColorAcento);
                    inner.Item().Text(lista[i].Value?.ToString() ?? string.Empty).FontSize(10);
                });

                // Columna derecha (si existe)
                if (i + 1 < lista.Count)
                {
                    row.RelativeItem().Column(inner =>
                    {
                        inner.Item().Text(lista[i + 1].Key).SemiBold().FontSize(9)
                            .FontColor(p.ColorAcento);
                        inner.Item().Text(lista[i + 1].Value?.ToString() ?? string.Empty).FontSize(10);
                    });
                }
                else
                {
                    row.RelativeItem(); // celda vacía para mantener alineación
                }
            });

            col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Height(1);
        }
    }

    private static void RenderPie(IContainer footer, PdfPlantillaData p)
    {
        footer.PaddingTop(4).AlignCenter().Text(p.TextoPiePaginaRenderizado)
            .FontSize(7).FontColor(Colors.Grey.Darken1);
    }
}
