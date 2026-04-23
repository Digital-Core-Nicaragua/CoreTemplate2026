using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Archivos.Domain.Events;

public record ArchivoSubido(
    Guid ArchivoId,
    string Url,
    string RutaAlmacenada,
    string Proveedor,
    long TamanioBytes) : IDomainEvent;

public record ArchivoEliminado(
    Guid ArchivoId,
    string RutaAlmacenada,
    string Proveedor) : IDomainEvent;
