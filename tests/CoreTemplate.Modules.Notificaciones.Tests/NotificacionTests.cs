using CoreTemplate.Modules.Notificaciones.Domain.Aggregates;
using CoreTemplate.Modules.Notificaciones.Application.Commands;
using CoreTemplate.Modules.Notificaciones.Domain.Repositories;
using CoreTemplate.Notifications.Abstractions;

namespace CoreTemplate.Modules.Notificaciones.Tests;

public class NotificacionTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var usuarioId = Guid.NewGuid();
        var result = Notificacion.Crear(
            usuarioId, "Tu comprobante está listo",
            "El comprobante de enero 2025 está disponible.",
            TipoNotificacion.Exito, "/nomina/comprobantes/1");

        result.IsSuccess.Should().BeTrue();
        result.Value!.UsuarioId.Should().Be(usuarioId);
        result.Value.EsLeida.Should().BeFalse();
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Crear_SinTitulo_RetornaFallo()
    {
        var result = Notificacion.Crear(
            Guid.NewGuid(), "", "Mensaje",
            TipoNotificacion.Info);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void MarcarComoLeida_NotificacionNoLeida_MarcaCorrectamente()
    {
        var notificacion = Notificacion.Crear(
            Guid.NewGuid(), "Título", "Mensaje",
            TipoNotificacion.Info).Value!;
        notificacion.ClearDomainEvents();

        var result = notificacion.MarcarComoLeida();

        result.IsSuccess.Should().BeTrue();
        notificacion.EsLeida.Should().BeTrue();
        notificacion.LeidaEn.Should().NotBeNull();
        notificacion.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void MarcarComoLeida_NotificacionYaLeida_RetornaFallo()
    {
        var notificacion = Notificacion.Crear(
            Guid.NewGuid(), "Título", "Mensaje",
            TipoNotificacion.Info).Value!;
        notificacion.MarcarComoLeida();

        var result = notificacion.MarcarComoLeida();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConTenantId_AsignaTenantCorrectamente()
    {
        var tenantId = Guid.NewGuid();
        var result = Notificacion.Crear(
            Guid.NewGuid(), "Título", "Mensaje",
            TipoNotificacion.Info, null, false, tenantId);

        result.Value!.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Crear_EntregadaEnTiempoReal_SeRegistraCorrectamente()
    {
        var result = Notificacion.Crear(
            Guid.NewGuid(), "Título", "Mensaje",
            TipoNotificacion.Exito, null,
            entregadaEnTiempoReal: true);

        result.Value!.EntregadaEnTiempoReal.Should().BeTrue();
    }
}

public class EnviarNotificacionHandlerTests
{
    [Fact]
    public async Task Handle_EnviaYPersiste_CuandoUsuarioConectado()
    {
        var repo = Substitute.For<INotificacionRepository>();
        var sender = Substitute.For<INotificationSender>();

        sender.EnviarAsync(Arg.Any<NotificationMessage>(), Arg.Any<CancellationToken>())
            .Returns(new NotificationResult(true, EntregadaEnTiempoReal: true));

        var handler = new EnviarNotificacionHandler(repo, sender);
        var cmd = new EnviarNotificacionCommand(
            Guid.NewGuid(), "Título", "Mensaje", TipoNotificacion.Info);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await repo.Received(1).GuardarAsync(Arg.Any<Notificacion>(), Arg.Any<CancellationToken>());
        await sender.Received(1).EnviarAsync(Arg.Any<NotificationMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PersisteSiempreAunqueSenderFalle()
    {
        var repo = Substitute.For<INotificacionRepository>();
        var sender = Substitute.For<INotificationSender>();

        // Simula usuario desconectado — no entregada en tiempo real pero sin error
        sender.EnviarAsync(Arg.Any<NotificationMessage>(), Arg.Any<CancellationToken>())
            .Returns(new NotificationResult(true, EntregadaEnTiempoReal: false));

        var handler = new EnviarNotificacionHandler(repo, sender);
        var cmd = new EnviarNotificacionCommand(
            Guid.NewGuid(), "Título", "Mensaje", TipoNotificacion.Info);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // Siempre persiste en BD independientemente de la entrega en tiempo real
        await repo.Received(1).GuardarAsync(Arg.Any<Notificacion>(), Arg.Any<CancellationToken>());
    }
}
