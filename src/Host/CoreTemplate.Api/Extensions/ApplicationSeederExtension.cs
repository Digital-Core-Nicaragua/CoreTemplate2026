using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;
using CoreTemplate.Modules.EmailTemplates.Infrastructure.Persistence;

namespace CoreTemplate.Api.Extensions;

/// <summary>
/// Extensión que ejecuta migraciones pendientes y seed de datos iniciales
/// al arrancar la aplicación en modo Development.
/// </summary>
public static class ApplicationSeederExtension
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        await AuthDataSeeder.SeedAsync(scope.ServiceProvider);
        await CatalogosDataSeeder.SeedAsync(scope.ServiceProvider);
        await EmailTemplatesDataSeeder.SeedAsync(scope.ServiceProvider);
    }
}
