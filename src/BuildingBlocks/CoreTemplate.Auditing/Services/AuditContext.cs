using System.Security.Claims;
using CoreTemplate.Auditing.Abstractions;
using CoreTemplate.Logging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CoreTemplate.Auditing.Services;

/// <summary>
/// Implementacion de <see cref="IAuditContext"/> que resuelve los datos
/// del actor desde el HttpContext y el ICorrelationContext.
/// </summary>
internal sealed class AuditContext(
    IHttpContextAccessor httpContextAccessor,
    ICorrelationContext correlationContext) : IAuditContext
{
    private HttpContext? Context => httpContextAccessor.HttpContext;

    public Guid? UsuarioId
    {
        get
        {
            var claim = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context?.User?.FindFirstValue("sub");
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var claim = Context?.User?.FindFirstValue("tenant_id")
                     ?? Context?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? DireccionIp =>
        Context?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        Context?.Request.Headers.UserAgent.ToString();

    public string? CorrelationId => correlationContext.CorrelationId;
}
