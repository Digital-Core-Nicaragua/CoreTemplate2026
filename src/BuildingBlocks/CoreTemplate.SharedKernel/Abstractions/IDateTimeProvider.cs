namespace CoreTemplate.SharedKernel.Abstractions;

/// <summary>
/// Proveedor centralizado de fecha y hora del sistema.
/// <para>
/// Garantiza que toda la aplicación trabaje en UTC internamente.
/// Evita el uso directo de <c>DateTime.UtcNow</c> en el dominio y handlers,
/// lo que permite controlar el tiempo en tests.
/// </para>
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Obtiene la fecha y hora actual en UTC.
    /// Toda la persistencia en base de datos debe guardarse en UTC.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Obtiene la fecha y hora actual convertida a la zona horaria indicada.
    /// </summary>
    /// <param name="timeZoneId">
    /// Identificador de zona horaria del sistema operativo.
    /// Ejemplo: "Central America Standard Time", "UTC"
    /// </param>
    DateTime Now(string timeZoneId);
}
