using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;

/// <summary>
/// Factory para crear <see cref="CatalogosDbContext"/> desde la CLI de EF Core.
/// <para>
/// Uso:
/// <code>
/// dotnet ef migrations add InitialCreate_Catalogos --project src/Modules/Catalogos/CoreTemplate.Modules.Catalogos.Infrastructure --startup-project src/Host/CoreTemplate.Api
/// </code>
/// </para>
/// </summary>
public sealed class CatalogosDbContextFactory : IDesignTimeDbContextFactory<CatalogosDbContext>
{
    public CatalogosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogosDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=CoreTemplateDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Catalogos"));

        var tenantSettings = Options.Create(new TenantSettings { IsMultiTenant = false });
        var currentTenant = new NullCurrentTenant();

        return new CatalogosDbContext(optionsBuilder.Options, currentTenant, tenantSettings);
    }

    private sealed class NullCurrentTenant : ICurrentTenant
    {
        public Guid? TenantId => null;
        public bool EsMultiTenant => false;
        public bool TenantResuelto => false;
    }
}
