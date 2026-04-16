using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;

/// <summary>
/// Seeder del módulo Catálogos.
/// Crea 3 ítems de ejemplo para demostrar el patrón de catálogo.
/// </summary>
public static class CatalogosDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<CatalogosDbContext>();
        await db.Database.MigrateAsync();

        if (await db.CatalogoItems.AnyAsync())
        {
            return;
        }

        var items = new[]
        {
            CatalogoItem.Crear("ITEM_001", "Ítem de ejemplo 1", "Primer ítem de catálogo de ejemplo"),
            CatalogoItem.Crear("ITEM_002", "Ítem de ejemplo 2", "Segundo ítem de catálogo de ejemplo"),
            CatalogoItem.Crear("ITEM_003", "Ítem de ejemplo 3", "Tercer ítem de catálogo de ejemplo — inactivo")
        };

        foreach (var result in items)
        {
            if (result.IsSuccess)
            {
                await db.CatalogoItems.AddAsync(result.Value!);
            }
        }

        await db.SaveChangesAsync();

        // Desactivar el tercer ítem como ejemplo
        var tercero = await db.CatalogoItems.FirstOrDefaultAsync(i => i.Codigo == "ITEM_003");
        if (tercero is not null)
        {
            tercero.Desactivar();
            await db.SaveChangesAsync();
        }
    }
}
