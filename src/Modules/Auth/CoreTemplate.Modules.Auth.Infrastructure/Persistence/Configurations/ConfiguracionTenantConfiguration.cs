using CoreTemplate.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class ConfiguracionTenantConfiguration : IEntityTypeConfiguration<ConfiguracionTenant>
{
    public void Configure(EntityTypeBuilder<ConfiguracionTenant> builder)
    {
        builder.ToTable("ConfiguracionesTenant");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.MaxSesionesSimultaneas).IsRequired(false);
        builder.Property(c => c.ModificadoEn).IsRequired();

        builder.HasIndex(c => c.TenantId).IsUnique()
            .HasDatabaseName("IX_ConfiguracionesTenant_TenantId");
    }
}
