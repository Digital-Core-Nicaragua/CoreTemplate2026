namespace CoreTemplate.Modules.PdfTemplates.Application.DTOs;

public record PdfPlantillaDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string Modulo,
    string CodigoTemplate,
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
    bool EsDeSistema,
    bool EsActivo,
    DateTime CreadoEn,
    DateTime? ModificadoEn);

public record DisenioDisponibleDto(
    string Codigo,
    string Nombre,
    string Descripcion,
    string Orientacion);
