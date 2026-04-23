using CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.EmailTemplates.Infrastructure.Persistence.Configurations;

internal sealed class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("Plantillas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Modulo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Asunto).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CuerpoHtml).IsRequired();
        builder.Property(x => x.VariablesDisponiblesJson).HasMaxLength(2000).HasColumnName("VariablesDisponibles");

        builder.HasIndex(x => new { x.Codigo, x.TenantId }).IsUnique()
            .HasDatabaseName("IX_Plantillas_Codigo_TenantId");

        builder.Ignore(x => x.DomainEvents);
    }
}
