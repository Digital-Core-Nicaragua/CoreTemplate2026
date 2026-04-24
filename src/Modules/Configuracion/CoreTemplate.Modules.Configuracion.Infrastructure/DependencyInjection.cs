using CoreTemplate.Modules.Configuracion.Application;
using CoreTemplate.Modules.Configuracion.Application.Abstractions;
using CoreTemplate.Modules.Configuracion.Domain.Repositories;
using CoreTemplate.Modules.Configuracion.Infrastructure.Persistence;
using CoreTemplate.Modules.Configuracion.Infrastructure.Repositories;
using CoreTemplate.Modules.Configuracion.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Configuracion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddConfiguracionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddConfiguracionApplication();

        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<ConfiguracionDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Configuracion"));
            else
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Configuracion"));
        });

        services.AddMemoryCache();
        services.AddScoped<IConfiguracionItemRepository, ConfiguracionItemRepository>();
        services.AddScoped<IConfiguracionService, ConfiguracionService>();

        return services;
    }
}
