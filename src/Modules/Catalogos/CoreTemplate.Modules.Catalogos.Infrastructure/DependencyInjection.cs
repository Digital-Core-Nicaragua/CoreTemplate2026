using CoreTemplate.Modules.Catalogos.Application;
using CoreTemplate.Modules.Catalogos.Domain.Repositories;
using CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;
using CoreTemplate.Modules.Catalogos.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Catalogos.Infrastructure;

/// <summary>
/// Registro de dependencias de la capa Infrastructure del módulo Catálogos.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCatalogosInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<CatalogosDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Catalogos"));
            }
            else
            {
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Catalogos"));
            }
        });

        services.AddScoped<ICatalogoItemRepository, CatalogoItemRepository>();

        return services;
    }

    /// <summary>
    /// Registra el modulo Catalogos completo: Application + Infrastructure.
    /// Usar este metodo desde Program.cs.
    /// </summary>
    public static IServiceCollection AddCatalogosModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCatalogosApplication(configuration);
        services.AddCatalogosInfrastructure(configuration);
        return services;
    }
}
