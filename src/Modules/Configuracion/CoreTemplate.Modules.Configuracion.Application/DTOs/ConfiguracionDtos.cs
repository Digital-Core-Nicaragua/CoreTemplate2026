namespace CoreTemplate.Modules.Configuracion.Application.DTOs;

public record ConfiguracionItemDto(
    Guid Id,
    string Clave,
    string Valor,
    string Tipo,
    string Descripcion,
    string Grupo,
    bool EsEditable,
    DateTime CreadoEn,
    DateTime? ModificadoEn);

public record ConfiguracionGrupoDto(
    string Grupo,
    IReadOnlyList<ConfiguracionItemDto> Items);
