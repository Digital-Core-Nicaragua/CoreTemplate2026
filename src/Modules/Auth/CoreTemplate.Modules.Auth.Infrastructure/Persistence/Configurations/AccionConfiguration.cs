using CoreTemplate.Modules.Auth.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class AccionConfiguration : IEntityTypeConfiguration<Accion>
{
    public void Configure(EntityTypeBuilder<Accion> builder)
    {
        builder.ToTable("Acciones");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Codigo).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Modulo).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Descripcion).HasMaxLength(500);
        builder.Property(a => a.EsActiva).IsRequired().HasDefaultValue(true);

        builder.HasIndex(a => a.Codigo).IsUnique()
            .HasDatabaseName("IX_Acciones_Codigo");
        builder.HasIndex(a => a.Modulo)
            .HasDatabaseName("IX_Acciones_Modulo");

        builder.Ignore(a => a.DomainEvents);
    }
}
