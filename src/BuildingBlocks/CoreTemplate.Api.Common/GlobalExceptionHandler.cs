using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreTemplate.Api.Common;

/// <summary>
/// Middleware global que captura cualquier excepción no controlada y la
/// formatea como <see cref="ApiResponse{T}"/> con <c>success: false</c>.
/// <para>
/// Garantiza que ningún stack trace ni información interna sea expuesta
/// al cliente en producción. Todos los errores 500 retornan el mismo
/// formato estándar de la API.
/// </para>
/// <para>
/// Registro en Program.cs:
/// <code>
/// builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;();
/// builder.Services.AddProblemDetails();
/// // ...
/// app.UseExceptionHandler();
/// </code>
/// </para>
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Excepción no controlada en {Method} {Path}: {Message}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "Ocurrió un error interno en el servidor.",
            Data = null,
            Errors = ["Ocurrió un error inesperado. Por favor intente nuevamente."]
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
