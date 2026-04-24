using CoreTemplate.Pdf.Abstractions;
using CoreTemplate.Pdf.Services;
using CoreTemplate.Pdf.Templates;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace CoreTemplate.Pdf;

public static class DependencyInjection
{
    public static IServiceCollection AddPdfService(this IServiceCollection services)
    {
        // Licencia QuestPDF Community (gratis para proyectos < $1M USD/año)
        QuestPDF.Settings.License = LicenseType.Community;

        // Diseños disponibles — agregar nuevos diseños aquí
        services.AddSingleton<IPdfDocumentTemplate, VerticalEstandarTemplate>();
        services.AddSingleton<IPdfDocumentTemplate, HorizontalEstandarTemplate>();
        services.AddSingleton<IPdfDocumentTemplate, CompactoTemplate>();
        services.AddSingleton<IPdfDocumentTemplate, ModernoTemplate>();

        // Factory que resuelve el diseño por CodigoTemplate
        services.AddSingleton<IPdfTemplateFactory, PdfTemplateFactory>();

        return services;
    }
}
