using CoreTemplate.Modules.Configuracion.Application.Abstractions;
using CoreTemplate.Modules.Configuracion.Domain.Repositories;
using CoreTemplate.SharedKernel.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CoreTemplate.Modules.Configuracion.Infrastructure.Services;

/// <summary>
/// Implementación de IConfiguracionService con cache IMemoryCache (TTL 10 minutos).
/// Jerarquía: tenant actual → global (TenantId = null) → valorPorDefecto.
/// Nunca lanza excepciones al consumidor.
/// </summary>
internal sealed class ConfiguracionService(
    IConfiguracionItemRepository repo,
    ICurrentTenant currentTenant,
    IMemoryCache cache) : IConfiguracionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public async Task<string> ObtenerStringAsync(string clave, string valorPorDefecto = "", CancellationToken ct = default)
    {
        var valor = await ObtenerValorAsync(clave, ct);
        return valor ?? valorPorDefecto;
    }

    public async Task<int> ObtenerIntAsync(string clave, int valorPorDefecto = 0, CancellationToken ct = default)
    {
        var valor = await ObtenerValorAsync(clave, ct);
        return int.TryParse(valor, out var result) ? result : valorPorDefecto;
    }

    public async Task<decimal> ObtenerDecimalAsync(string clave, decimal valorPorDefecto = 0, CancellationToken ct = default)
    {
        var valor = await ObtenerValorAsync(clave, ct);
        return decimal.TryParse(valor, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : valorPorDefecto;
    }

    public async Task<bool> ObtenerBoolAsync(string clave, bool valorPorDefecto = false, CancellationToken ct = default)
    {
        var valor = await ObtenerValorAsync(clave, ct);
        return bool.TryParse(valor, out var result) ? result : valorPorDefecto;
    }

    public async Task<T?> ObtenerJsonAsync<T>(string clave, CancellationToken ct = default) where T : class
    {
        var valor = await ObtenerValorAsync(clave, ct);
        if (string.IsNullOrWhiteSpace(valor)) return null;
        try { return JsonSerializer.Deserialize<T>(valor); }
        catch { return null; }
    }

    public async Task ActualizarAsync(string clave, string valor, Guid modificadoPor, CancellationToken ct = default)
    {
        var item = await repo.ObtenerPorClaveAsync(clave, currentTenant.TenantId, ct)
                ?? await repo.ObtenerPorClaveAsync(clave, null, ct);

        if (item is null) return;

        item.Actualizar(valor, modificadoPor);
        await repo.ActualizarAsync(item, ct);
        InvalidarCache(clave);
    }

    public void InvalidarCache(string clave)
    {
        cache.Remove(CacheKey(clave, currentTenant.TenantId));
        cache.Remove(CacheKey(clave, null));
    }

    // ─── Privado ──────────────────────────────────────────────────────────────

    private async Task<string?> ObtenerValorAsync(string clave, CancellationToken ct)
    {
        var tenantKey = CacheKey(clave, currentTenant.TenantId);

        if (cache.TryGetValue(tenantKey, out string? cached))
            return cached;

        // Buscar para el tenant actual
        var item = currentTenant.TenantId is not null
            ? await repo.ObtenerPorClaveAsync(clave, currentTenant.TenantId, ct)
            : null;

        // Fallback a global
        item ??= await repo.ObtenerPorClaveAsync(clave, null, ct);

        var valor = item?.Valor;
        cache.Set(tenantKey, valor, CacheTtl);
        return valor;
    }

    private static string CacheKey(string clave, Guid? tenantId) =>
        $"cfg:{tenantId?.ToString() ?? "global"}:{clave}";
}
