using CoreTemplate.Infrastructure.Services;
using CoreTemplate.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Infrastructure.Middleware;

/// <summary>
/// Middleware que valida la presencia del tenant en cada solicitud HTTP
/// cuando el sistema está configurado en modo multi-tenant.
/// <para>
/// Si <c>IsMultiTenant = true</c> y no se puede resolver el TenantId,
/// retorna 400 Bad Request con un mensaje descriptivo.
/// </para>
/// <para>
/// Rutas excluidas de la validación de tenant:
/// <list type="bullet">
///   <item>Swagger UI y documentación</item>
///   <item>Health checks</item>
///   <item>Endpoints de autenticación inicial (login, registro)</item>
/// </list>
/// </para>
/// <para>
/// Registro en Program.cs (solo cuando IsMultiTenant = true):
/// <code>
/// if (app.Configuration.GetValue&lt;bool&gt;("TenantSettings:IsMultiTenant"))
/// {
///     app.UseMiddleware&lt;TenantMiddleware&gt;();
/// }
/// </code>
/// </para>
/// </summary>
public sealed class TenantMiddleware(
    RequestDelegate _next,
    IOptions<TenantSettings> _settings)
{
    // Rutas que no requieren tenant
    private static readonly string[] _rutasExcluidas =
    [
        "/swagger",
        "/health",
        "/api/auth/login",
        "/api/auth/registro",
        "/api/auth/restablecer-password",
        "/api/auth/solicitar-restablecimiento"
    ];

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant)
    {
        // Si no es multi-tenant, continuar sin validar
        if (!_settings.Value.IsMultiTenant)
        {
            await _next(context);
            return;
        }

        // Verificar si la ruta está excluida
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var esRutaExcluida = _rutasExcluidas.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase));

        if (esRutaExcluida)
        {
            await _next(context);
            return;
        }

        // Validar que el tenant fue resuelto
        if (!currentTenant.TenantResuelto)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "No se pudo identificar el tenant de la solicitud.",
                data = (object?)null,
                errors = new[] { "Incluya el header 'X-Tenant-Id' con el ID del tenant." }
            });

            return;
        }

        await _next(context);
    }
}
