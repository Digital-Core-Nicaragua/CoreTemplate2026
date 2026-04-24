using CoreTemplate.Pdf.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoreTemplate.Pdf.Templates;

/// <summary>
/// Diseño A4 horizontal.
/// Igual que Vertical Estándar pero en orientación horizontal.
/// Ideal para: reportes con muchas columnas, tablas anchas, listados.
/// </summary>
public sealed class HorizontalEstandarTemplate : IPdfDocumentTemplate
{
    public string Codigo => "horizontal-estandar";
    public string Nombre => "Horizontal Estándar";
    public string Descripcion => "A4 horizontal. Ideal para reportes con muchas columnas o tablas anchas.";
    public string Orientacion => "Horizontal";

    public byte[] Generar(PdfPlantillaData plantilla, IPdfContent contenido)
    {
        var datos = contenido.ObtenerDatos();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(h => RenderEncabezado(h, plantilla));
                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(6);
                    RenderDatos(col, datos);
                });
                page.Footer().Element(f => RenderPie(f, plantilla));
            });
        }).GeneratePdf();
    }

    private static void RenderEncabezado(IContainer header, PdfPlantillaData p)
    {
        header.Background(p.ColorEncabezado).Padding(10).Row(row =>
        {
            if (!string.IsNullOrWhiteSpace(p.LogoUrl) && File.Exists(p.LogoUrl))
                row.ConstantItem(70).AlignMiddle()
                    .Image(p.LogoUrl).WithRasterDpi(300);

            row.RelativeItem().AlignMiddle().PaddingLeft(10).Column(col =>
            {
                col.Item().Text(p.NombreEmpresa)
                    .FontColor(p.ColorTextoHeader).FontSize(14).Bold();

                if (!string.IsNullOrWhiteSpace(p.TextoSecundario))
                    col.Item().Text(p.TextoSecundario)
                        .FontColor(p.ColorTextoHeader).FontSize(9);
            });
        });
    }

    private static void RenderDatos(ColumnDescriptor col, Dictionary<string, object> datos)
    {
        // En horizontal los datos se muestran en tabla con columnas dinámicas
        var claves = datos.Keys.ToList();
        var valores = datos.Values.ToList();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                foreach (var _ in claves)
                    c.RelativeColumn();
            });

            // Encabezados
            foreach (var clave in claves)
                table.Cell().Background(Colors.Grey.Lighten3)
                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(clave).SemiBold().FontSize(10);

            // Valores
            foreach (var valor in valores)
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(valor?.ToString() ?? string.Empty).FontSize(10);
        });
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
