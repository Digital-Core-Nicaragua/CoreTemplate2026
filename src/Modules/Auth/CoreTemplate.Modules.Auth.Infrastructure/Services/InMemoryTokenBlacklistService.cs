using CoreTemplate.Modules.Auth.Application.Abstractions;
using System.Collections.Concurrent;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementación InMemory de la blacklist de tokens.
/// Válida para un solo servidor. Se pierde al reiniciar.
/// Usar en desarrollo o sistemas con una sola instancia.
/// </summary>
internal sealed class InMemoryTokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

    public Task AgregarAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        var expira = DateTime.UtcNow.Add(ttl);
        _blacklist[jti] = expira;
        LimpiarExpirados();
        return Task.CompletedTask;
    }

    public Task<bool> EstaEnBlacklistAsync(string jti, CancellationToken ct = default)
    {
        if (_blacklist.TryGetValue(jti, out var expira))
        {
            if (DateTime.UtcNow < expira)
            {
                return Task.FromResult(true);
            }
            _blacklist.TryRemove(jti, out _);
        }
        return Task.FromResult(false);
    }

    private void LimpiarExpirados()
    {
        var ahora = DateTime.UtcNow;
        foreach (var kvp in _blacklist.Where(x => x.Value <= ahora))
        {
            _blacklist.TryRemove(kvp.Key, out _);
        }
    }
}
