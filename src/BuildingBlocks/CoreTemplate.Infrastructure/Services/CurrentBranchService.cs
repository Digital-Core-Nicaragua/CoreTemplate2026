using Microsoft.AspNetCore.Http;

namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Resuelve la sucursal activa desde el claim branch_id del JWT.
/// </summary>
internal sealed class CurrentBranchService(IHttpContextAccessor _httpContextAccessor) : ICurrentBranch
{
    public Guid? BranchId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("branch_id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
