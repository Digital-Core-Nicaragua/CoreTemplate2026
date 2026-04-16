using CoreTemplate.Modules.Auth.Application.Abstractions;
using StackExchange.Redis;

namespace CoreTemplate.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Implementación Redis de la blacklist de tokens.
/// Válida para múltiples instancias. TTL gestionado por Redis.
/// Usar en producción.
/// </summary>
internal sealed class RedisTokenBlacklistService(IConnectionMultiplexer _redis) : ITokenBlacklistService
{
    private const string Prefix = "token_blacklist:";
    private readonly IDatabase _db = _redis.GetDatabase();

    public async Task AgregarAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        await _db.StringSetAsync(Prefix + jti, "1", ttl);
    }

    public async Task<bool> EstaEnBlacklistAsync(string jti, CancellationToken ct = default)
    {
        return await _db.KeyExistsAsync(Prefix + jti);
    }
}
