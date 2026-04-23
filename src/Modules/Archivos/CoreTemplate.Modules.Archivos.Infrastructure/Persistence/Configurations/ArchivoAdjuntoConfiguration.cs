using CoreTemplate.Modules.Archivos.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Archivos.Infrastructure.Persistence.Configurations;

internal sealed class ArchivoAdjuntoConfiguration : IEntityTypeConfiguration<ArchivoAdjunto>
{
    public void Configure(EntityTypeBuilder<ArchivoAdjunto> builder)
    {
        builder.ToTable("Archivos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NombreOriginal).HasMaxLength(500).IsRequired();
        builder.Property(x => x.NombreAlmacenado).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RutaAlmacenada).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Proveedor).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Contexto).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ModuloOrigen).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.ModuloOrigen, x.EntidadId });
        builder.HasIndex(x => x.RutaAlmacenada);
        builder.HasIndex(x => new { x.TenantId, x.ModuloOrigen })
            .HasDatabaseName("IX_Archivos_TenantId_ModuloOrigen");

        builder.Ignore(x => x.DomainEvents);
    }
}
