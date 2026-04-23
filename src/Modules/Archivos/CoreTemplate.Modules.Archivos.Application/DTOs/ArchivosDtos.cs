namespace CoreTemplate.Modules.Archivos.Application.DTOs;

public record ArchivoAdjuntoDto(
    Guid Id,
    string NombreOriginal,
    string ContentType,
    long TamanioBytes,
    string Proveedor,
    string Contexto,
    string ModuloOrigen,
    Guid? EntidadId,
    DateTime FechaSubida);

public record ArchivoUrlDto(Guid Id, string Url);
