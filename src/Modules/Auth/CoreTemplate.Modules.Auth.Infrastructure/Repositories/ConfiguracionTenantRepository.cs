using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreTemplate.Modules.Auth.Infrastructure.Repositories;

internal sealed class ConfiguracionTenantRepository(AuthDbContext _db) : IConfiguracionTenantRepository
{
    public Task<ConfiguracionTenant?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default) =>
        _db.ConfiguracionesTenant.FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);

    public async Task AddAsync(ConfiguracionTenant config, CancellationToken ct = default)
    {
        await _db.ConfiguracionesTenant.AddAsync(config, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ConfiguracionTenant config, CancellationToken ct = default)
    {
        _db.ConfiguracionesTenant.Update(config);
        await _db.SaveChangesAsync(ct);
    }
}
