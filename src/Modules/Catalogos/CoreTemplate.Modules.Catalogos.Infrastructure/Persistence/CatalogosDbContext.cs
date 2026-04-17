using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Persistence;

/// <summary>
/// DbContext del módulo Catálogos.
/// Hereda <see cref="BaseDbContext"/> para soporte multi-tenant configurable.
/// Usa el schema "Catalogos" para separar las tablas del módulo.
/// </summary>
public sealed class CatalogosDbContext(
    DbContextOptions<CatalogosDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings)
    : BaseDbContext(options, currentTenant, tenantSettings)
{
    public DbSet<CatalogoItem> CatalogoItems => Set<CatalogoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Catalogos");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogosDbContext).Assembly);
    }
}
