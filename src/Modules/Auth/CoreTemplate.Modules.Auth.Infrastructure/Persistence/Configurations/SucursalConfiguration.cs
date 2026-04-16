using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("Sucursales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Codigo).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(s => s.EsActiva).IsRequired().HasDefaultValue(true);

        builder.HasIndex(s => new { s.TenantId, s.Codigo }).IsUnique()
            .HasDatabaseName("IX_Sucursales_TenantId_Codigo");

        builder.Ignore(s => s.DomainEvents);
    }
}

internal sealed class UsuarioSucursalConfiguration : IEntityTypeConfiguration<UsuarioSucursal>
{
    public void Configure(EntityTypeBuilder<UsuarioSucursal> builder)
    {
        builder.ToTable("UsuarioSucursales");
        builder.HasKey(us => us.Id);

        builder.Property(us => us.EsPrincipal).IsRequired().HasDefaultValue(false);
        builder.Property(us => us.AsignadoEn).IsRequired();

        builder.HasIndex(us => new { us.UsuarioId, us.SucursalId }).IsUnique()
            .HasDatabaseName("IX_UsuarioSucursales_UsuarioId_SucursalId");
    }
}
