using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Configuracion.Domain.Aggregates;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Configuracion.Infrastructure.Persistence;

public sealed class ConfiguracionDbContext(
    DbContextOptions<ConfiguracionDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings)
    : BaseDbContext(options, currentTenant, tenantSettings)
{
    public DbSet<ConfiguracionItem> Items => Set<ConfiguracionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Configuracion");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfiguracionDbContext).Assembly);
    }
}
