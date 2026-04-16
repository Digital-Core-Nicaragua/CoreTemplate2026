using System.Security.Claims;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="ICurrentTenant"/> que resuelve el tenant
/// de la solicitud actual usando la estrategia configurada en <c>TenantSettings</c>.
/// <para>
/// Estrategias de resolución (en orden de prioridad):
/// <list type="number">
///   <item>Header HTTP: <c>X-Tenant-Id</c></item>
///   <item>Claim JWT: <c>tenant_id</c></item>
///   <item>Subdominio: <c>tenant1.miapp.com</c></item>
/// </list>
/// </para>
/// </summary>
internal sealed class CurrentTenantService(
    IHttpContextAccessor _httpContextAccessor,
    IOptions<TenantSettings> _settings) : ICurrentTenant
{
    private readonly TenantSettings _tenantSettings = _settings.Value;

    /// <inheritdoc/>
    public bool EsMultiTenant => _tenantSettings.IsMultiTenant;

    /// <inheritdoc/>
    public Guid? TenantId => EsMultiTenant ? ResolverTenantId() : null;

    /// <inheritdoc/>
    public bool TenantResuelto => TenantId.HasValue;

    private Guid? ResolverTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        // 1. Header HTTP: X-Tenant-Id
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue)
            && Guid.TryParse(headerValue, out var tenantIdFromHeader))
        {
            return tenantIdFromHeader;
        }

        // 2. Claim JWT: tenant_id
        var tenantClaim = context.User?.FindFirstValue("tenant_id");
        if (Guid.TryParse(tenantClaim, out var tenantIdFromClaim))
        {
            return tenantIdFromClaim;
        }

        // 3. Subdominio: tenant1.miapp.com → "tenant1"
        var host = context.Request.Host.Host;
        var parts = host.Split('.');
        if (parts.Length >= 3)
        {
            // El subdominio es el primer segmento — buscar por nombre requeriría
            // una consulta a BD, por lo que esta estrategia solo aplica si el
            // subdominio ES el TenantId en formato Guid
            if (Guid.TryParse(parts[0], out var tenantIdFromSubdomain))
            {
                return tenantIdFromSubdomain;
            }
        }

        return null;
    }
}
