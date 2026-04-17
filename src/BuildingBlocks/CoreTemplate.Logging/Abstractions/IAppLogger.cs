namespace CoreTemplate.Logging.Abstractions;

/// <summary>
/// Abstraccion de logging estructurado para usar en handlers y servicios de Application.
/// Evita la dependencia directa a ILogger&lt;T&gt; de Microsoft en capas de dominio/aplicacion.
/// </summary>
public interface IAppLogger
{
    void Info(string mensaje, params object[] args);
    void Warning(string mensaje, params object[] args);
    void Error(Exception ex, string mensaje, params object[] args);
    void Debug(string mensaje, params object[] args);

    /// <summary>Crea una instancia con contexto de tipo T para enriquecer los logs.</summary>
    IAppLogger ForContext<T>();
}
