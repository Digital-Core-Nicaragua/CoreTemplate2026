using CoreTemplate.Modules.Archivos.Application;
using CoreTemplate.Modules.Archivos.Domain.Repositories;
using CoreTemplate.Modules.Archivos.Infrastructure.Persistence;
using CoreTemplate.Modules.Archivos.Infrastructure.Repositories;
using CoreTemplate.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Archivos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddArchivosModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Building block Storage
        services.AddStorageService(configuration);

        // Application
        services.AddArchivosApplication(configuration);

        // DbContext
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<ArchivosDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Archivos"));
            else
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Archivos"));
        });

        services.AddScoped<IArchivoAdjuntoRepository, ArchivoAdjuntoRepository>();

        return services;
    }
}
