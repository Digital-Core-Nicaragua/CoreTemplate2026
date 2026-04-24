using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure.Persistence;

public static class PdfTemplatesDataSeeder
{
    private static readonly (string Codigo, string Nombre, string Modulo, string CodigoTemplate,
        string NombreEmpresa, string ColorEncabezado, string ColorTextoHeader, string ColorAcento,
        string TextoPie)[] _plantillas =
    [
        ("sistema.vertical-estandar",   "Layout Vertical Estándar",   "Sistema",      "vertical-estandar",   "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}} — {{FechaGeneracion}}"),
        ("sistema.horizontal-estandar", "Layout Horizontal Estándar", "Sistema",      "horizontal-estandar", "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}} — {{FechaGeneracion}}"),
        ("nomina.comprobante-pago",     "Comprobante de Pago",        "Nomina",       "vertical-estandar",   "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}} — Comprobante generado el {{FechaGeneracion}}"),
        ("contabilidad.factura",        "Factura",                    "Contabilidad", "vertical-estandar",   "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}} — Documento fiscal"),
        ("contabilidad.recibo",         "Recibo de Pago",             "Contabilidad", "compacto",            "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}}"),
        ("rrhh.contrato",               "Contrato de Trabajo",        "RRHH",         "vertical-estandar",   "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}} — Documento confidencial"),
        ("rrhh.constancia-laboral",     "Constancia Laboral",         "RRHH",         "compacto",            "Mi Sistema", "#1a2e5a", "#ffffff", "#4f46e5", "{{NombreEmpresa}}"),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<PdfTemplatesDbContext>();

        var pendientes = await db.Database.GetPendingMigrationsAsync();
        if (pendientes.Any()) await db.Database.MigrateAsync();

        foreach (var (codigo, nombre, modulo, codigoTemplate, nombreEmpresa,
            colorEnc, colorTxt, colorAcento, textoPie) in _plantillas)
        {
            if (await db.Plantillas.IgnoreQueryFilters()
                    .AnyAsync(p => p.Codigo == codigo && p.TenantId == null))
                continue;

            var result = PdfPlantilla.Crear(
                codigo, nombre, modulo, codigoTemplate,
                nombreEmpresa, null,
                colorEnc, colorTxt, colorAcento,
                null, textoPie, true, true, null,
                esDeSistema: true);

            if (result.IsSuccess)
                await db.Plantillas.AddAsync(result.Value!);
        }

        await db.SaveChangesAsync();
    }
}
