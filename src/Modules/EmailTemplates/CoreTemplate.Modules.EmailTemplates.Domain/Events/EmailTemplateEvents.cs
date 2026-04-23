using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.EmailTemplates.Domain.Events;

public record PlantillaCreada(Guid TemplateId, string Codigo, string Modulo) : IDomainEvent;
public record PlantillaActualizada(Guid TemplateId, string Codigo, Guid ModificadoPor) : IDomainEvent;
public record PlantillaActivada(Guid TemplateId, string Codigo) : IDomainEvent;
public record PlantillaDesactivada(Guid TemplateId, string Codigo) : IDomainEvent;
