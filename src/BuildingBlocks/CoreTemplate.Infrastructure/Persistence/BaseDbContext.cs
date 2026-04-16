using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Infrastructure.Persistence;

/// <summary>
/// DbContext base para todos los módulos del sistema.
/// <para>
/// Provee soporte multi-tenant configurable mediante <c>QueryFilter</c> global
/// que filtra automáticamente por <see cref="IHasTenant.TenantId"/> en todas
/// las entidades que implementen <see cref="IHasTenant"/>.
/// </para>
/// <para>
/// Cuando <c>IsMultiTenant = false</c> en configuración, los QueryFilters
/// de tenant no se aplican y el sistema opera como single-tenant.
/// </para>
/// <para>
/// Cada módulo hereda de esta clase y agrega sus propios <c>DbSet</c>
/// y configuraciones de entidades:
/// <code>
/// public sealed class AuthDbContext(
///     DbContextOptions&lt;AuthDbContext&gt; options,
///     ICurrentTenant currentTenant,
///     IOptions&lt;TenantSettings&gt; tenantSettings)
///     : BaseDbContext(options, currentTenant, tenantSettings)
/// {
///     public DbSet&lt;Usuario&gt; Usuarios => Set&lt;Usuario&gt;();
///
///     protected override void OnModelCreating(ModelBuilder modelBuilder)
///     {
///         base.OnModelCreating(modelBuilder); // ← Siempre llamar base primero
///         modelBuilder.HasDefaultSchema("Auth");
///         modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
///     }
/// }
/// </code>
/// </para>
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly TenantSettings _tenantSettings;

    /// <summary>
    /// Constructor para uso en producción con inyección de dependencias.
    /// </summary>
    protected BaseDbContext(
        DbContextOptions options,
        ICurrentTenant currentTenant,
        IOptions<TenantSettings> tenantSettings) : base(options)
    {
        _currentTenant = currentTenant;
        _tenantSettings = tenantSettings.Value;
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar QueryFilter global de tenant a todas las entidades que implementen IHasTenant
        if (_tenantSettings.IsMultiTenant)
        {
            AplicarQueryFiltersTenant(modelBuilder);
        }
    }

    /// <summary>
    /// Aplica un QueryFilter global por TenantId a todas las entidades
    /// que implementen <see cref="IHasTenant"/> en este DbContext.
    /// </summary>
    private void AplicarQueryFiltersTenant(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // Crear expresión lambda: e => e.TenantId == _currentTenant.TenantId
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(IHasTenant.TenantId));
            var tenantIdValue = System.Linq.Expressions.Expression.Constant(_currentTenant.TenantId, typeof(Guid?));
            var equals = System.Linq.Expressions.Expression.Equal(tenantIdProperty, tenantIdValue);
            var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
