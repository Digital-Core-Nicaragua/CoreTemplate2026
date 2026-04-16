using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="ICurrentUser"/> que resuelve el usuario
/// autenticado desde los claims del JWT en el <see cref="IHttpContextAccessor"/>.
/// </summary>
internal sealed class CurrentUserService(IHttpContextAccessor _httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc/>
    public Guid? Id
    {
        get
        {
            var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue("sub");

            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                         ?? User?.FindFirstValue("email");

    /// <inheritdoc/>
    public string? Nombre => User?.FindFirstValue(ClaimTypes.Name)
                          ?? User?.FindFirstValue("name");

    /// <inheritdoc/>
    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? [];

    /// <inheritdoc/>
    public bool EstaAutenticado => User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc/>
    public bool TieneRol(string rol) =>
        User?.IsInRole(rol) ?? false;

    /// <inheritdoc/>
    public bool TienePermiso(string permiso) =>
        User?.HasClaim("permission", permiso) ?? false;
}
