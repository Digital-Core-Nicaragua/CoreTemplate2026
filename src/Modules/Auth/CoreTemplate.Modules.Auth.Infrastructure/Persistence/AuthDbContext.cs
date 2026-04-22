using CoreTemplate.Auditing.Interceptors;
using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.SharedKernel.Abstractions;
using CoreTemplate.Infrastructure.Settings;
using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Auth.Infrastructure.Persistence;

/// <summary>
/// DbContext del módulo Auth.
/// Hereda <see cref="BaseDbContext"/> para soporte multi-tenant configurable.
/// Usa el schema "Auth" para separar las tablas del módulo.
/// </summary>
public sealed class AuthDbContext(
    DbContextOptions<AuthDbContext> options,
    ICurrentTenant currentTenant,
    IOptions<TenantSettings> tenantSettings,
    AuditSaveChangesInterceptor? auditInterceptor = null)
    : BaseDbContext(options, currentTenant, tenantSettings, auditInterceptor)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<AsignacionRol> AsignacionesRol => Set<AsignacionRol>();
    public DbSet<Accion> Acciones => Set<Accion>();
    public DbSet<ConfiguracionTenant> ConfiguracionesTenant => Set<ConfiguracionTenant>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    /// <summary>
    /// Clientes del portal externo.
    /// Solo se usa cuando CustomerPortalSettings:EnableCustomerPortal = true.
    /// </summary>
    public DbSet<UsuarioCliente> UsuariosCliente => Set<UsuarioCliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Siempre llamar base primero — aplica QueryFilters de tenant
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Auth");

        // Configurar acceso por backing field para todas las entidades
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
