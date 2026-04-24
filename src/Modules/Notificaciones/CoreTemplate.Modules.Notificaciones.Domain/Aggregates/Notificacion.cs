using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Modules.Notificaciones.Domain.Events;
using CoreTemplate.Notifications.Abstractions;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Notificaciones.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa una notificación persistida en BD.
/// Usa TipoNotificacion del building block CoreTemplate.Notifications.
/// Implementa IHasTenant para aislamiento multi-tenant automático.
/// </summary>
public sealed class Notificacion : AggregateRoot<Guid>, IHasTenant
{
    public Guid? TenantId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Mensaje { get; private set; } = string.Empty;
    public TipoNotificacion Tipo { get; private set; }
    public string? Url { get; private set; }
    public bool EsLeida { get; private set; }
    public bool EntregadaEnTiempoReal { get; private set; }
    public DateTime CreadaEn { get; private set; }
    public DateTime? LeidaEn { get; private set; }

    private Notificacion() { }

    public static Result<Notificacion> Crear(
        Guid usuarioId, string titulo, string mensaje,
        TipoNotificacion tipo, string? url = null,
        bool entregadaEnTiempoReal = false, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            return Result<Notificacion>.Failure("El título es requerido.");

        var notificacion = new Notificacion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UsuarioId = usuarioId,
            Titulo = titulo.Trim(),
            Mensaje = mensaje?.Trim() ?? string.Empty,
            Tipo = tipo,
            Url = url?.Trim(),
            EsLeida = false,
            EntregadaEnTiempoReal = entregadaEnTiempoReal,
            CreadaEn = DateTime.UtcNow
        };

        notificacion.RaiseDomainEvent(new NotificacionCreada(
            notificacion.Id, notificacion.UsuarioId, notificacion.Tipo.ToString()));

        return Result<Notificacion>.Success(notificacion);
    }

    public Result MarcarComoLeida()
    {
        if (EsLeida) return Result.Failure("La notificación ya está leída.");
        EsLeida = true;
        LeidaEn = DateTime.UtcNow;
        RaiseDomainEvent(new NotificacionLeida(Id, UsuarioId, LeidaEn.Value));
        return Result.Success();
    }
}
