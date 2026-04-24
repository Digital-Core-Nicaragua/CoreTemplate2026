namespace CoreTemplate.Modules.Notificaciones.Application.DTOs;

public record NotificacionDto(
    Guid Id,
    string Titulo,
    string Mensaje,
    string Tipo,
    string? Url,
    bool EsLeida,
    DateTime CreadaEn,
    DateTime? LeidaEn);

public record ConteoNoLeidasDto(int Count);
