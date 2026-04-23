namespace CoreTemplate.Modules.EmailTemplates.Application.DTOs;

public record EmailTemplateDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string Modulo,
    string Asunto,
    string CuerpoHtml,
    IReadOnlyList<string> VariablesDisponibles,
    bool UsarLayout,
    bool EsDeSistema,
    bool EsActivo,
    DateTime CreadoEn,
    DateTime? ModificadoEn);

public record PreviewResultDto(string AsuntoRenderizado, string CuerpoRenderizado);
