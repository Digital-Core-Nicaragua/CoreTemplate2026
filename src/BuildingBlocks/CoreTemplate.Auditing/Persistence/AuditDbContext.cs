using CoreTemplate.Auditing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Auditing.Persistence;

/// <summary>
/// DbContext dedicado para la tabla de auditoria.
/// Schema: Shared — no interfiere con los modulos.
/// </summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs", "Shared");
            e.HasKey(a => a.Id);
            e.Property(a => a.NombreEntidad).IsRequired().HasMaxLength(100);
            e.Property(a => a.EntidadId).IsRequired().HasMaxLength(50);
            e.Property(a => a.Accion).IsRequired();
            e.Property(a => a.OcurridoEn).IsRequired();
            e.Property(a => a.DireccionIp).HasMaxLength(50);
            e.Property(a => a.CorrelationId).HasMaxLength(50);

            e.HasIndex(a => new { a.NombreEntidad, a.EntidadId });
            e.HasIndex(a => a.OcurridoEn);
            e.HasIndex(a => a.UsuarioId);
            e.HasIndex(a => a.TenantId);
        });
    }
}
