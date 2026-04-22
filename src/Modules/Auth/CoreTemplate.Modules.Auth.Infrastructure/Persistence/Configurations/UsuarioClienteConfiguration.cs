using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF para el aggregate UsuarioCliente.
/// Tabla: Auth.UsuariosCliente
/// Solo se usa cuando CustomerPortalSettings:EnableCustomerPortal = true.
/// </summary>
internal sealed class UsuarioClienteConfiguration : IEntityTypeConfiguration<UsuarioCliente>
{
    public void Configure(EntityTypeBuilder<UsuarioCliente> builder)
    {
        builder.ToTable("UsuariosCliente");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Telefono).HasMaxLength(30);
        builder.Property(c => c.PasswordHash).HasMaxLength(500);
        builder.Property(c => c.Estado).IsRequired().HasConversion<string>();
        builder.Property(c => c.EmailVerificado).IsRequired().HasDefaultValue(false);
        builder.Property(c => c.TelefonoVerificado).IsRequired().HasDefaultValue(false);
        builder.Property(c => c.TokenVerificacionEmail).HasMaxLength(100);
        builder.Property(c => c.CodigoVerificacionTelefono).HasMaxLength(10);
        builder.Property(c => c.IntentosFallidos).IsRequired().HasDefaultValue(0);
        builder.Property(c => c.CreadoEn).IsRequired();
        builder.Property(c => c.TokenRestablecimiento).HasMaxLength(100);
        builder.Property(c => c.TokenRestablecimientoExpiraEn);

        // Email como string nullable — sin conversion, EF lo mapea directamente
        builder.Property(c => c.Email)
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired(false);

        // TipoRegistro
        builder.Property(c => c.TipoRegistro).IsRequired().HasConversion<int>();

        // Índice único filtrado: Email + TenantId (solo cuando Email no es null)
        builder.HasIndex(c => new { c.Email, c.TenantId })
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL")
            .HasDatabaseName("IX_UsuariosCliente_Email_TenantId");

        // Índice único filtrado: Telefono + TenantId (solo cuando Telefono no es null)
        builder.HasIndex(c => new { c.Telefono, c.TenantId })
            .IsUnique()
            .HasFilter("[Telefono] IS NOT NULL")
            .HasDatabaseName("IX_UsuariosCliente_Telefono_TenantId");

        builder.HasIndex(c => c.TenantId).HasDatabaseName("IX_UsuariosCliente_TenantId");

        // Colección de proveedores OAuth — se mapea como tabla separada
        builder.OwnsMany(c => c.Proveedores, proveedor =>
        {
            proveedor.ToTable("UsuarioClienteProveedores");
            proveedor.WithOwner().HasForeignKey("UsuarioClienteId");
            proveedor.HasKey("UsuarioClienteId", nameof(ProveedorOAuthVinculado.Proveedor));

            proveedor.Property(p => p.Proveedor).IsRequired().HasConversion<string>();
            proveedor.Property(p => p.ExternalId).IsRequired().HasMaxLength(200);
            proveedor.Property(p => p.Email).IsRequired().HasMaxLength(200);
            proveedor.Property(p => p.VinculadoEn).IsRequired();

            // Índice para búsqueda por proveedor + externalId (login recurrente OAuth)
            proveedor.HasIndex(
                nameof(ProveedorOAuthVinculado.Proveedor),
                nameof(ProveedorOAuthVinculado.ExternalId))
                .HasDatabaseName("IX_UsuarioClienteProveedores_Proveedor_ExternalId");
        });

        builder.Ignore(c => c.DomainEvents);
    }
}
