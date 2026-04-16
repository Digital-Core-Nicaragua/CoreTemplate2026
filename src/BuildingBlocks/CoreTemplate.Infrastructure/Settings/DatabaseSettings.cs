namespace CoreTemplate.Infrastructure.Settings;

/// <summary>
/// Configuración del proveedor de base de datos.
/// Se lee desde la sección <c>DatabaseSettings</c> en <c>appsettings.json</c>.
/// <para>
/// Ejemplo de configuración para SQL Server:
/// <code>
/// {
///   "DatabaseSettings": {
///     "Provider": "SqlServer",
///     "ConnectionString": "Server=localhost;Database=MiDb;..."
///   }
/// }
/// </code>
/// </para>
/// <para>
/// Ejemplo de configuración para PostgreSQL:
/// <code>
/// {
///   "DatabaseSettings": {
///     "Provider": "PostgreSQL",
///     "ConnectionString": "Host=localhost;Database=MiDb;Username=postgres;Password=..."
///   }
/// }
/// </code>
/// </para>
/// </summary>
public sealed class DatabaseSettings
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "DatabaseSettings";

    /// <summary>
    /// Proveedor de base de datos.
    /// Valores válidos: <c>SqlServer</c>, <c>PostgreSQL</c>.
    /// </summary>
    public string Provider { get; init; } = "SqlServer";

    /// <summary>Cadena de conexión a la base de datos.</summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Indica si el proveedor configurado es SQL Server.</summary>
    public bool EsSqlServer =>
        Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

    /// <summary>Indica si el proveedor configurado es PostgreSQL.</summary>
    public bool EsPostgreSQL =>
        Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase);
}
