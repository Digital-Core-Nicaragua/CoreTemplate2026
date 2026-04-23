using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Archivos.Domain.Aggregates;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Archivos.Infrastructure.Persistence;

public sealed class ArchivosDbContext(
    DbContextOptions<ArchivosDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings)
    : BaseDbContext(options, currentTenant, tenantSettings)
{
    public DbSet<ArchivoAdjunto> Archivos => Set<ArchivoAdjunto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Archivos");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArchivosDbContext).Assembly);
    }
}
