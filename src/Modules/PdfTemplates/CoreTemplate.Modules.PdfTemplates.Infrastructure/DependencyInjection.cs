using CoreTemplate.Modules.PdfTemplates.Application;
using CoreTemplate.Modules.PdfTemplates.Application.Abstractions;
using CoreTemplate.Modules.PdfTemplates.Domain.Repositories;
using CoreTemplate.Modules.PdfTemplates.Infrastructure.Persistence;
using CoreTemplate.Modules.PdfTemplates.Infrastructure.Repositories;
using CoreTemplate.Modules.PdfTemplates.Infrastructure.Services;
using CoreTemplate.Pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPdfTemplatesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Building block PDF (QuestPDF + diseños + factory)
        services.AddPdfService();

        // Application
        services.AddPdfTemplatesApplication();

        // DbContext
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<PdfTemplatesDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "PdfTemplates"));
            else
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "PdfTemplates"));
        });

        // Repositorios y servicios
        services.AddScoped<IPdfPlantillaRepository, PdfPlantillaRepository>();
        services.AddScoped<IModuloPdfGenerator, ModuloPdfGenerator>();

        return services;
    }
}
