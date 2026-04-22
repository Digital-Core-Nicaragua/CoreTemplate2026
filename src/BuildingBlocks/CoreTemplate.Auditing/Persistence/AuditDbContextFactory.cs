using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CoreTemplate.Auditing.Persistence;

/// <summary>
/// Factory para crear <see cref="AuditDbContext"/> desde la CLI de EF Core.
/// </summary>
public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var connectionString = configuration["DatabaseSettings:ConnectionString"]
            ?? throw new InvalidOperationException("No se encontró DatabaseSettings:ConnectionString.");

        var provider = configuration["DatabaseSettings:Provider"] ?? "SqlServer";

        var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();

        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Shared"));
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Shared"));
        }

        return new AuditDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "src", "Host", "CoreTemplate.Api"),
            Directory.GetCurrentDirectory(),
        };

        var basePath = candidates.FirstOrDefault(p => File.Exists(Path.Combine(p, "appsettings.json")))
            ?? Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
    }
}
