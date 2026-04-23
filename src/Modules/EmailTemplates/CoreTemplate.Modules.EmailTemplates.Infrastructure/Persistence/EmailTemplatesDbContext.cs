using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Persistence;

public sealed class EmailTemplatesDbContext(
    DbContextOptions<EmailTemplatesDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings)
    : BaseDbContext(options, currentTenant, tenantSettings)
{
    public DbSet<EmailTemplate> Plantillas => Set<EmailTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("EmailTemplates");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmailTemplatesDbContext).Assembly);
    }
}
