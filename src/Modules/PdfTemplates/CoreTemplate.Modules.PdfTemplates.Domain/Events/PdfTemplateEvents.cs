using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.PdfTemplates.Domain.Events;

public record PlantillaPdfCreada(Guid PlantillaId, string Codigo, string Modulo) : IDomainEvent;
public record PlantillaPdfActualizada(Guid PlantillaId, string Codigo, Guid ModificadoPor) : IDomainEvent;
public record PlantillaPdfActivada(Guid PlantillaId, string Codigo) : IDomainEvent;
public record PlantillaPdfDesactivada(Guid PlantillaId, string Codigo) : IDomainEvent;
