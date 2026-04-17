using Microsoft.AspNetCore.Http;

namespace CoreTemplate.Logging.Middleware;

/// <summary>
/// Middleware que genera o propaga el header X-Correlation-Id en cada request HTTP.
/// <para>
/// Si el cliente envia el header X-Correlation-Id, se reutiliza.
/// Si no, se genera un nuevo Guid.
/// El valor se almacena en HttpContext.Items y se retorna en la respuesta.
/// </para>
/// </summary>
public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public const string CorrelationIdKey = "CorrelationId";
    public const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[CorrelationIdKey] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        await next(context);
    }
}
