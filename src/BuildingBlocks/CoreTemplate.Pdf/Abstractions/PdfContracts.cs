namespace CoreTemplate.Pdf.Abstractions;

/// <summary>
/// Datos corporativos de la plantilla resueltos desde BD.
/// Se pasan al diseño para renderizar el encabezado, pie y marca de agua.
/// </summary>
public record PdfPlantillaData(
    string NombreEmpresa,
    string? LogoUrl,
    string ColorEncabezado,
    string ColorTextoHeader,
    string ColorAcento,
    string? TextoSecundario,
    string? TextoPiePagina,
    bool MostrarNumeroPagina,
    bool MostrarFechaGeneracion,
    string? MarcaDeAgua,
    string SistemaNombre,
    DateTime FechaGeneracion)
{
    /// <summary>Texto del pie con variables ya reemplazadas.</summary>
    public string TextoPiePaginaRenderizado =>
        (TextoPiePagina ?? string.Empty)
            .Replace("{{NombreEmpresa}}", NombreEmpresa)
            .Replace("{{SistemaNombre}}", SistemaNombre)
            .Replace("{{FechaGeneracion}}", FechaGeneracion.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{AnioActual}}", FechaGeneracion.Year.ToString());
}

/// <summary>
/// Contrato que cada módulo implementa para proveer sus datos al diseño PDF.
/// </summary>
public interface IPdfContent
{
    Dictionary<string, object> ObtenerDatos();
}

/// <summary>
/// Contrato que cada diseño de PDF implementa.
/// Un diseño define la estructura visual del documento (layout, estilos, posición de elementos).
/// Los datos corporativos (logo, colores) vienen de PdfPlantillaData.
/// Los datos del negocio (empleado, factura, etc.) vienen de IPdfContent.
/// </summary>
public interface IPdfDocumentTemplate
{
    /// <summary>Código único e inmutable del diseño. Ej: "vertical-estandar"</summary>
    string Codigo { get; }

    /// <summary>Nombre descriptivo para mostrar en la UI.</summary>
    string Nombre { get; }

    /// <summary>Descripción de cuándo usar este diseño.</summary>
    string Descripcion { get; }

    /// <summary>"Vertical" o "Horizontal"</summary>
    string Orientacion { get; }

    /// <summary>
    /// Genera el PDF combinando la configuración corporativa con los datos del negocio.
    /// </summary>
    byte[] Generar(PdfPlantillaData plantilla, IPdfContent contenido);
}

/// <summary>
/// Contrato principal para generar PDFs desde cualquier módulo.
/// Resuelve la plantilla de BD, el diseño en código y genera el PDF.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Genera un PDF usando la plantilla configurada en BD para el código y tenant indicados.
    /// Jerarquía: plantilla del tenant → plantilla global → error.
    /// </summary>
    Task<byte[]> GenerarAsync(
        string codigoPlantilla,
        Guid? tenantId,
        IPdfContent contenido,
        CancellationToken ct = default);
}

/// <summary>
/// Resuelve el diseño correcto según el CodigoTemplate.
/// Registra todos los IPdfDocumentTemplate disponibles en DI.
/// </summary>
public interface IPdfTemplateFactory
{
    IPdfDocumentTemplate Resolver(string codigoTemplate);
    IReadOnlyList<IPdfDocumentTemplate> ObtenerTodos();
}
