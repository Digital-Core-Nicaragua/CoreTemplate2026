namespace CoreTemplate.Modules.PdfTemplates.Api.Contracts;

public record CrearPdfPlantillaRequest(
    string Codigo, string Nombre, string Modulo, string CodigoTemplate,
    string NombreEmpresa, string? LogoUrl,
    string ColorEncabezado = "#1a2e5a",
    string ColorTextoHeader = "#ffffff",
    string ColorAcento = "#4f46e5",
    string? TextoSecundario = null,
    string? TextoPiePagina = null,
    bool MostrarNumeroPagina = true,
    bool MostrarFechaGeneracion = true,
    string? MarcaDeAgua = null);

public record ActualizarPdfPlantillaRequest(
    string Nombre, string CodigoTemplate,
    string NombreEmpresa, string? LogoUrl,
    string ColorEncabezado,
    string ColorTextoHeader,
    string ColorAcento,
    string? TextoSecundario,
    string? TextoPiePagina,
    bool MostrarNumeroPagina,
    bool MostrarFechaGeneracion,
    string? MarcaDeAgua);

public record PreviewPdfRequest(Dictionary<string, object> Datos);
