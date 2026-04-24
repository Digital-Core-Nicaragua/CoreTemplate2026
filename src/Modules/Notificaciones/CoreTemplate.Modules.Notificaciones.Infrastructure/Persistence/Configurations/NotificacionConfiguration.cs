using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Notificaciones.Infrastructure.Persistence.Configurations;

internal sealed class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Mensaje).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(500);

        builder.HasIndex(x => new { x.UsuarioId, x.EsLeida })
            .HasDatabaseName("IX_Notificaciones_UsuarioId_EsLeida");

        builder.HasIndex(x => new { x.TenantId, x.UsuarioId })
            .HasDatabaseName("IX_Notificaciones_TenantId_UsuarioId");

        builder.Ignore(x => x.DomainEvents);
    }
}
