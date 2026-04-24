using CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.PdfTemplates.Infrastructure.Persistence.Configurations;

internal sealed class PdfPlantillaConfiguration : IEntityTypeConfiguration<PdfPlantilla>
{
    public void Configure(EntityTypeBuilder<PdfPlantilla> builder)
    {
        builder.ToTable("Plantillas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Modulo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CodigoTemplate).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NombreEmpresa).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(2000);
        builder.Property(x => x.ColorEncabezado).HasMaxLength(7).IsRequired();
        builder.Property(x => x.ColorTextoHeader).HasMaxLength(7).IsRequired();
        builder.Property(x => x.ColorAcento).HasMaxLength(7).IsRequired();
        builder.Property(x => x.TextoSecundario).HasMaxLength(500);
        builder.Property(x => x.TextoPiePagina).HasMaxLength(500);
        builder.Property(x => x.MarcaDeAgua).HasMaxLength(100);

        builder.HasIndex(x => new { x.Codigo, x.TenantId }).IsUnique()
            .HasDatabaseName("IX_PdfPlantillas_Codigo_TenantId");

        builder.Ignore(x => x.DomainEvents);
    }
}
