using CoreTemplate.Modules.Configuracion.Domain.Aggregates;

namespace CoreTemplate.Modules.Configuracion.Domain.Repositories;

public interface IConfiguracionItemRepository
{
    Task<ConfiguracionItem?> ObtenerPorClaveAsync(string clave, Guid? tenantId = null, CancellationToken ct = default);
    Task<IReadOnlyList<ConfiguracionItem>> ListarAsync(string? grupo = null, Guid? tenantId = null, CancellationToken ct = default);
    Task<bool> ExisteClaveAsync(string clave, Guid? tenantId = null, CancellationToken ct = default);
    Task GuardarAsync(ConfiguracionItem item, CancellationToken ct = default);
    Task ActualizarAsync(ConfiguracionItem item, CancellationToken ct = default);
}
