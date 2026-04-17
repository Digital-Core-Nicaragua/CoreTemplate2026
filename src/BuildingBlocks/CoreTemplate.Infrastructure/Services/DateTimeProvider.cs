using CoreTemplate.SharedKernel.Abstractions;

namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IDateTimeProvider"/> que usa el reloj del sistema en UTC.
/// <para>
/// Registrada como Scoped en DI para poder reemplazarla por un mock en tests.
/// </para>
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime Now(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return UtcNow;

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(UtcNow, timeZone);
    }
}
