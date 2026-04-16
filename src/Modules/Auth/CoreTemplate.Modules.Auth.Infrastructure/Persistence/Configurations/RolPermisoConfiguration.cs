using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Descripcion).HasMaxLength(500);
        builder.Property(r => r.EsSistema).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.CreadoEn).IsRequired();

        builder.HasIndex(r => new { r.TenantId, r.Nombre }).IsUnique()
            .HasDatabaseName("IX_Roles_TenantId_Nombre");

        builder.HasMany(r => r.Permisos)
            .WithOne()
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(r => r.DomainEvents);
    }
}

internal sealed class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("RolPermisos");
        builder.HasKey(rp => rp.Id);
        builder.HasIndex(rp => new { rp.RolId, rp.PermisoId }).IsUnique();
    }
}

internal sealed class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("Permisos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descripcion).HasMaxLength(500);
        builder.Property(p => p.Modulo).IsRequired().HasMaxLength(100);
        builder.Property(p => p.CreadoEn).IsRequired();

        builder.HasIndex(p => p.Codigo).IsUnique()
            .HasDatabaseName("IX_Permisos_Codigo");

        builder.Ignore(p => p.DomainEvents);
    }
}

internal sealed class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("RegistrosAuditoria");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Email).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Evento).IsRequired().HasConversion<string>();
        builder.Property(r => r.Ip).IsRequired().HasMaxLength(50);
        builder.Property(r => r.UserAgent).HasMaxLength(500);
        builder.Property(r => r.Detalle).HasMaxLength(1000);
        builder.Property(r => r.CreadoEn).IsRequired();

        builder.HasIndex(r => r.UsuarioId).HasDatabaseName("IX_RegistrosAuditoria_UsuarioId");
        builder.HasIndex(r => r.CreadoEn).HasDatabaseName("IX_RegistrosAuditoria_CreadoEn");
    }
}
