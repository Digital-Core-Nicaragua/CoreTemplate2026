using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// Factory para crear <see cref="AuthDbContext"/> desde la CLI de EF Core.
/// Permite ejecutar <c>dotnet ef migrations add</c> y <c>dotnet ef database update</c>
/// sin necesidad de levantar la aplicación completa.
/// <para>
/// Uso:
/// <code>
/// dotnet ef migrations add InitialCreate_Auth --project src/Modules/Auth/CoreTemplate.Modules.Auth.Infrastructure --startup-project src/Host/CoreTemplate.Api
/// </code>
/// </para>
/// </summary>
public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        // Usar SQL Server por defecto para migraciones en desarrollo
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=CoreTemplateDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "Auth"));

        var tenantSettings = Options.Create(new TenantSettings { IsMultiTenant = false });
        var currentTenant = new NullCurrentTenant();

        return new AuthDbContext(optionsBuilder.Options, currentTenant, tenantSettings);
    }

    /// <summary>
    /// Implementación nula de ICurrentTenant para uso en design-time (migraciones CLI).
    /// </summary>
    private sealed class NullCurrentTenant : ICurrentTenant
    {
        public Guid? TenantId => null;
        public bool EsMultiTenant => false;
        public bool TenantResuelto => false;
    }
}
