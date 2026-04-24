using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Configuracion.Domain.Events;

public record ConfiguracionCreada(Guid ItemId, string Clave, string Grupo) : IDomainEvent;
public record ConfiguracionActualizada(Guid ItemId, string Clave, string ValorAnterior, string ValorNuevo, Guid ModificadoPor) : IDomainEvent;
