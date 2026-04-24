using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Notificaciones.Infrastructure.Persistence;

public sealed class NotificacionesDbContext(
    DbContextOptions<NotificacionesDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings)
    : BaseDbContext(options, currentTenant, tenantSettings)
{
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Notificaciones");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificacionesDbContext).Assembly);
    }
}
