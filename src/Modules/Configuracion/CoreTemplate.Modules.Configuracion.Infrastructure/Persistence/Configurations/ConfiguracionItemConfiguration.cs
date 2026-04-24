using CoreTemplate.Modules.Configuracion.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Configuracion.Infrastructure.Persistence.Configurations;

internal sealed class ConfiguracionItemConfiguration : IEntityTypeConfiguration<ConfiguracionItem>
{
    public void Configure(EntityTypeBuilder<ConfiguracionItem> builder)
    {
        builder.ToTable("Items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Clave).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Valor).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Grupo).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.Clave, x.TenantId }).IsUnique()
            .HasDatabaseName("IX_Configuracion_Clave_TenantId");

        builder.Ignore(x => x.DomainEvents);
    }
}
