using CoreTemplate.SharedKernel;

namespace CoreTemplate.Modules.Notificaciones.Domain.Events;

public record NotificacionCreada(Guid NotificacionId, Guid UsuarioId, string Tipo) : IDomainEvent;
public record NotificacionLeida(Guid NotificacionId, Guid UsuarioId, DateTime LeidaEn) : IDomainEvent;
public record TodasNotificacionesLeidas(Guid UsuarioId, int Cantidad) : IDomainEvent;
