using CoreTemplate.Modules.Auth.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("Sesiones");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RefreshTokenHash).IsRequired().HasMaxLength(64);
        builder.Property(s => s.Canal).IsRequired().HasConversion<string>();
        builder.Property(s => s.Dispositivo).HasMaxLength(200);
        builder.Property(s => s.Ip).IsRequired().HasMaxLength(50);
        builder.Property(s => s.UserAgent).HasMaxLength(500);
        builder.Property(s => s.EsActiva).IsRequired().HasDefaultValue(true);

        builder.HasIndex(s => s.RefreshTokenHash).IsUnique()
            .HasDatabaseName("IX_Sesiones_RefreshTokenHash");
        builder.HasIndex(s => new { s.UsuarioId, s.EsActiva })
            .HasDatabaseName("IX_Sesiones_UsuarioId_EsActiva");

        builder.Ignore(s => s.DomainEvents);
    }
}
