using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.TipoUsuario).IsRequired().HasConversion<string>();
        builder.Property(u => u.Estado).IsRequired().HasConversion<string>();
        builder.Property(u => u.IntentosFallidos).IsRequired().HasDefaultValue(0);
        builder.Property(u => u.TwoFactorActivo).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.TwoFactorSecretKey).HasMaxLength(500);
        builder.Property(u => u.CreadoEn).IsRequired();

        // Value Object Email
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(200);

            email.HasIndex(e => e.Valor).IsUnique()
                .HasDatabaseName("IX_Usuarios_Email");
        });

        // Value Object PasswordHash
        builder.OwnsOne(u => u.PasswordHash, hash =>
        {
            hash.Property(h => h.Valor)
                .HasColumnName("PasswordHash")
                .IsRequired()
                .HasMaxLength(500);
        });

        builder.HasIndex(u => u.TenantId).HasDatabaseName("IX_Usuarios_TenantId");

        // Colecciones — usar backing fields con UsePropertyAccessMode
        builder.HasMany(u => u.Roles)
            .WithOne()
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.TokensRestablecimiento)
            .WithOne()
            .HasForeignKey(tr => tr.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.CodigosRecuperacion)
            .WithOne()
            .HasForeignKey(cr => cr.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sucursales)
            .WithOne()
            .HasForeignKey(us => us.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(u => u.DomainEvents);
    }
}

internal sealed class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuarioRoles");
        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.AsignadoEn).IsRequired();
        builder.HasIndex(ur => new { ur.UsuarioId, ur.RolId }).IsUnique();
    }
}


internal sealed class TokenRestablecimientoConfiguration : IEntityTypeConfiguration<TokenRestablecimiento>
{
    public void Configure(EntityTypeBuilder<TokenRestablecimiento> builder)
    {
        builder.ToTable("TokensRestablecimiento");
        builder.HasKey(tr => tr.Id);
        builder.Property(tr => tr.Token).IsRequired().HasMaxLength(500);
        builder.HasIndex(tr => tr.Token).IsUnique();
    }
}

internal sealed class CodigoRecuperacion2FAConfiguration : IEntityTypeConfiguration<CodigoRecuperacion2FA>
{
    public void Configure(EntityTypeBuilder<CodigoRecuperacion2FA> builder)
    {
        builder.ToTable("CodigosRecuperacion2FA");
        builder.HasKey(cr => cr.Id);
        builder.Property(cr => cr.CodigoHash).IsRequired().HasMaxLength(500);
    }
}
