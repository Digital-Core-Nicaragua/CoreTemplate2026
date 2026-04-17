namespace CoreTemplate.Monitoring.Abstractions;

/// <summary>
/// Abstraccion para verificar el estado de salud del sistema.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>Retorna true si todos los checks estan en estado Healthy.</summary>
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
