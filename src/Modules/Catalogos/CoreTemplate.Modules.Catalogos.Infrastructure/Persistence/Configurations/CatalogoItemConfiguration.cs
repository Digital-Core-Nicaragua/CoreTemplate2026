using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Catalogos.Infrastructure.Persistence.Configurations;

internal sealed class CatalogoItemConfiguration : IEntityTypeConfiguration<CatalogoItem>
{
    public void Configure(EntityTypeBuilder<CatalogoItem> builder)
    {
        builder.ToTable("CatalogoItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Descripcion).HasMaxLength(500);
        builder.Property(i => i.EsActivo).IsRequired().HasDefaultValue(true);
        builder.Property(i => i.CreadoEn).IsRequired();

        // Índice único por TenantId + Codigo
        builder.HasIndex(i => new { i.TenantId, i.Codigo }).IsUnique()
            .HasDatabaseName("IX_CatalogoItems_TenantId_Codigo");

        builder.Ignore(i => i.DomainEvents);
    }
}
