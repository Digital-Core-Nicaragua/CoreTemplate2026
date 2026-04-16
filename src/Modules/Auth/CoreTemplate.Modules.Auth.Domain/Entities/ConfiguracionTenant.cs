using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Entities;

/// <summary>
/// Configuración específica de un tenant.
/// Permite sobrescribir configuraciones globales por tenant.
/// Solo relevante cuando TenantSettings:IsMultiTenant = true y
/// TenantSettings:EnableSessionLimitsPerTenant = true.
/// </summary>
public sealed class ConfiguracionTenant : Entity<Guid>
{
    /// <summary>ID del tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Límite de sesiones simultáneas para este tenant.
    /// Null = usar el límite global de AuthSettings.
    /// </summary>
    public int? MaxSesionesSimultaneas { get; private set; }

    /// <summary>Fecha de última modificación.</summary>
    public DateTime ModificadoEn { get; private set; }

    private ConfiguracionTenant() { }

    public static ConfiguracionTenant Crear(Guid tenantId, int? maxSesiones = null) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        MaxSesionesSimultaneas = maxSesiones,
        ModificadoEn = DateTime.UtcNow
    };

    public void ActualizarLimiteSesiones(int? maxSesiones)
    {
        MaxSesionesSimultaneas = maxSesiones;
        ModificadoEn = DateTime.UtcNow;
    }
}
