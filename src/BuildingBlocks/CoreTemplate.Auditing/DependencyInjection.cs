using CoreTemplate.Auditing.Abstractions;
using CoreTemplate.Auditing.Interceptors;
using CoreTemplate.Auditing.Persistence;
using CoreTemplate.Auditing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Auditing;

/// <summary>
/// Registro de dependencias del building block Auditing.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra IAuditContext, IAuditService, AuditSaveChangesInterceptor y AuditDbContext.
    /// </summary>
    public static IServiceCollection AddCoreAuditing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontro la cadena de conexion.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        services.AddDbContext<AuditDbContext>(options =>
        {
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Shared"));
            else
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Shared"));
        });

        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        return services;
    }
}
