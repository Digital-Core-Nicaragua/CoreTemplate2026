namespace CoreTemplate.Logging.Abstractions;

/// <summary>
/// Contexto de correlacion del request HTTP actual.
/// Se resuelve como Scoped — un valor por request.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>ID unico del request. Se propaga via header X-Correlation-Id.</summary>
    string CorrelationId { get; }

    /// <summary>ID del tenant del request actual. Null si single-tenant.</summary>
    string? TenantId { get; }

    /// <summary>ID del usuario autenticado. Null si anonimo.</summary>
    string? UserId { get; }
}
