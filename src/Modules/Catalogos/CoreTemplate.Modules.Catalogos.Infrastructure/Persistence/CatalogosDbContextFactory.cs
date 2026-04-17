using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;

/// <summary>
/// Factory para crear <see cref="CatalogosDbContext"/> desde la CLI de EF Core.
/// Lee la connection string desde appsettings.json del proyecto Host.
/// </summary>
public sealed class CatalogosDbContextFactory : IDesignTimeDbContextFactory<CatalogosDbContext>
{
    public CatalogosDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró DatabaseSettings:ConnectionString.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        var optionsBuilder = new DbContextOptionsBuilder<CatalogosDbContext>();

        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Catalogos"));
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Catalogos"));
        }

        var tenantSettings = Options.Create(new TenantSettings { IsMultiTenant = false });
        var currentTenant = new NullCurrentTenant();

        return new CatalogosDbContext(optionsBuilder.Options, currentTenant, tenantSettings);
    }

    private static IConfiguration BuildConfiguration()
    {
        var basePath = FindAppsettingsPath();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
    }

    private static string FindAppsettingsPath()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "src", "Host", "CoreTemplate.Api"),
            Directory.GetCurrentDirectory(),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(Path.Combine(path, "appsettings.json")))
                return path;
        }

        return Directory.GetCurrentDirectory();
    }

    private sealed class NullCurrentTenant : ICurrentTenant
    {
        public Guid? TenantId => null;
        public bool EsMultiTenant => false;
        public bool TenantResuelto => false;
    }
}
