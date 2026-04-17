using System.Security.Claims;
using CoreTemplate.Logging.Abstractions;
using CoreTemplate.Logging.Middleware;
using Microsoft.AspNetCore.Http;

namespace CoreTemplate.Logging.Services;

/// <summary>
/// Implementacion de <see cref="ICorrelationContext"/> que resuelve los valores
/// desde el HttpContext del request actual.
/// </summary>
internal sealed class CorrelationContext(IHttpContextAccessor httpContextAccessor) : ICorrelationContext
{
    public string CorrelationId =>
        httpContextAccessor.HttpContext?.Items[CorrelationMiddleware.CorrelationIdKey]?.ToString()
        ?? Guid.NewGuid().ToString();

    public string? TenantId =>
        httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault()
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");

    public string? UserId =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
}
