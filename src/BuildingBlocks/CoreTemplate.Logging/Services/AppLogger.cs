using CoreTemplate.Logging.Abstractions;
using Serilog;

namespace CoreTemplate.Logging.Services;

/// <summary>
/// Implementacion de <see cref="IAppLogger"/> sobre Serilog.
/// </summary>
internal sealed class AppLogger(ICorrelationContext correlationContext) : IAppLogger
{
    private ILogger _logger = Log.Logger;

    public IAppLogger ForContext<T>()
    {
        _logger = Log.ForContext<T>();
        return this;
    }

    public void Info(string mensaje, params object[] args) =>
        _logger
            .ForContext("CorrelationId", correlationContext.CorrelationId)
            .ForContext("UserId", correlationContext.UserId)
            .Information(mensaje, args);

    public void Warning(string mensaje, params object[] args) =>
        _logger
            .ForContext("CorrelationId", correlationContext.CorrelationId)
            .ForContext("UserId", correlationContext.UserId)
            .Warning(mensaje, args);

    public void Error(Exception ex, string mensaje, params object[] args) =>
        _logger
            .ForContext("CorrelationId", correlationContext.CorrelationId)
            .ForContext("UserId", correlationContext.UserId)
            .Error(ex, mensaje, args);

    public void Debug(string mensaje, params object[] args) =>
        _logger
            .ForContext("CorrelationId", correlationContext.CorrelationId)
            .ForContext("UserId", correlationContext.UserId)
            .Debug(mensaje, args);
}
