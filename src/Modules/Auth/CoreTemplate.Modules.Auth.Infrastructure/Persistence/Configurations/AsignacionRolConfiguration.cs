using CoreTemplate.Modules.Auth.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class AsignacionRolConfiguration : IEntityTypeConfiguration<AsignacionRol>
{
    public void Configure(EntityTypeBuilder<AsignacionRol> builder)
    {
        builder.ToTable("AsignacionesRol");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AsignadoEn).IsRequired();

        builder.HasIndex(a => new { a.UsuarioId, a.SucursalId, a.RolId }).IsUnique()
            .HasDatabaseName("IX_AsignacionesRol_UsuarioId_SucursalId_RolId");

        builder.HasIndex(a => new { a.UsuarioId, a.SucursalId })
            .HasDatabaseName("IX_AsignacionesRol_UsuarioId_SucursalId");

        builder.Ignore(a => a.DomainEvents);
    }
}
