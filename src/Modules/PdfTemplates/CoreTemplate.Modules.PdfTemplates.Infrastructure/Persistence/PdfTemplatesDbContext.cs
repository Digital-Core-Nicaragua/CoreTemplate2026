using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure.Persistence;

public sealed class PdfTemplatesDbContext(
    DbContextOptions<PdfTemplatesDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings)
    : BaseDbContext(options, currentTenant, tenantSettings)
{
    public DbSet<PdfPlantilla> Plantillas => Set<PdfPlantilla>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("PdfTemplates");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PdfTemplatesDbContext).Assembly);
    }
}
