using CoreTemplate.Modules.Auth.Domain.Aggregates;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Events;
using CoreTemplate.Modules.Auth.Domain.ValueObjects;
using FluentAssertions;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class UsuarioTests
{
    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static Usuario CrearUsuarioActivo(string email = "test@test.com")
    {
        var emailVO = Email.Crear(email).Value!;
        var hashVO = PasswordHash.Crear("$2a$12$hash").Value!;
        var usuario = Usuario.Crear(emailVO, "Test User", hashVO).Value!;
        usuario.Activar();
        usuario.ClearDomainEvents();
        return usuario;
    }

    // ─── Crear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearUsuarioEnEstadoPendiente()
    {
        var email = Email.Crear("nuevo@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;

        var result = Usuario.Crear(email, "Nuevo Usuario", hash);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Estado.Should().Be(EstadoUsuario.Pendiente);
        result.Value.Email.Valor.Should().Be("nuevo@test.com");
        result.Value.Nombre.Should().Be("Nuevo Usuario");
        result.Value.IntentosFallidos.Should().Be(0);
        result.Value.TwoFactorActivo.Should().BeFalse();
    }

    [Fact]
    public void Crear_DebeDispararEventoUsuarioRegistrado()
    {
        var email = Email.Crear("nuevo@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;

        var result = Usuario.Crear(email, "Nuevo Usuario", hash);

        result.Value!.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UsuarioRegistradoEvent>();
    }

    [Fact]
    public void Crear_ConNombreVacio_DebeFallar()
    {
        var email = Email.Crear("test@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;

        var result = Usuario.Crear(email, "", hash);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Crear_ConNombreMayorA100Caracteres_DebeFallar()
    {
        var email = Email.Crear("test@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;
        var nombreLargo = new string('a', 101);

        var result = Usuario.Crear(email, nombreLargo, hash);

        result.IsSuccess.Should().BeFalse();
    }

    // ─── Activar / Desactivar ─────────────────────────────────────────────────

    [Fact]
    public void Activar_UsuarioPendiente_DebeActivarloCorrectamente()
    {
        var email = Email.Crear("test@test.com").Value!;
        var hash = PasswordHash.Crear("$2a$12$hash").Value!;
        var usuario = Usuario.Crear(email, "Test", hash).Value!;

        var result = usuario.Activar();

        result.IsSuccess.Should().BeTrue();
        usuario.Estado.Should().Be(EstadoUsuario.Activo);
    }

    [Fact]
    public void Activar_UsuarioYaActivo_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();

        var result = usuario.Activar();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Desactivar_UsuarioActivo_DebeDesactivarlo()
    {
        var usuario = CrearUsuarioActivo();

        var result = usuario.Desactivar();

        result.IsSuccess.Should().BeTrue();
        usuario.Estado.Should().Be(EstadoUsuario.Inactivo);
    }

    [Fact]
    public void Desactivar_UsuarioYaInactivo_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();
        usuario.Desactivar();

        var result = usuario.Desactivar();

        result.IsSuccess.Should().BeFalse();
    }

    // ─── Bloqueo ──────────────────────────────────────────────────────────────

    [Fact]
    public void IncrementarIntentosFallidos_AlLlegarAlLimite_DebeBloquearCuenta()
    {
        var usuario = CrearUsuarioActivo();

        for (int i = 0; i < 5; i++)
        {
            usuario.IncrementarIntentosFallidos(maxIntentos: 5, minutosBloqueado: 15);
        }

        usuario.Estado.Should().Be(EstadoUsuario.Bloqueado);
        usuario.BloqueadoHasta.Should().NotBeNull();
        usuario.BloqueadoHasta!.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void IncrementarIntentosFallidos_SinLlegarAlLimite_NoDebeBloquear()
    {
        var usuario = CrearUsuarioActivo();

        usuario.IncrementarIntentosFallidos(maxIntentos: 5, minutosBloqueado: 15);
        usuario.IncrementarIntentosFallidos(maxIntentos: 5, minutosBloqueado: 15);

        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.IntentosFallidos.Should().Be(2);
    }

    [Fact]
    public void Desbloquear_UsuarioBloqueado_DebeDesbloquearlo()
    {
        var usuario = CrearUsuarioActivo();
        usuario.Bloquear(DateTime.UtcNow.AddMinutes(15));

        var result = usuario.Desbloquear();

        result.IsSuccess.Should().BeTrue();
        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.BloqueadoHasta.Should().BeNull();
        usuario.IntentosFallidos.Should().Be(0);
    }

    [Fact]
    public void Desbloquear_UsuarioNoBloquedo_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();

        var result = usuario.Desbloquear();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PuedeAutenticarse_UsuarioActivo_DebeRetornarVerdadero()
    {
        var usuario = CrearUsuarioActivo();

        usuario.PuedeAutenticarse().Should().BeTrue();
    }

    [Fact]
    public void PuedeAutenticarse_UsuarioInactivo_DebeRetornarFalso()
    {
        var usuario = CrearUsuarioActivo();
        usuario.Desactivar();

        usuario.PuedeAutenticarse().Should().BeFalse();
    }

    [Fact]
    public void PuedeAutenticarse_UsuarioBloqueadoConTiempoExpirado_DebeDesbloquearAutomaticamente()
    {
        var usuario = CrearUsuarioActivo();
        // Bloquear con fecha ya pasada
        usuario.Bloquear(DateTime.UtcNow.AddSeconds(-1));

        var puede = usuario.PuedeAutenticarse();

        puede.Should().BeTrue();
        usuario.Estado.Should().Be(EstadoUsuario.Activo);
    }

    // ─── Contraseña ───────────────────────────────────────────────────────────

    [Fact]
    public void CambiarPassword_DebeActualizarHash()
    {
        var usuario = CrearUsuarioActivo();
        var nuevoHash = PasswordHash.Crear("$2a$12$nuevohash").Value!;

        usuario.CambiarPassword(nuevoHash);

        usuario.PasswordHash.Valor.Should().Be("$2a$12$nuevohash");
    }

    // ─── Roles ────────────────────────────────────────────────────────────────

    [Fact]
    public void AsignarRol_RolNuevo_DebeAsignarlo()
    {
        var usuario = CrearUsuarioActivo();
        var rolId = Guid.NewGuid();

        var result = usuario.AsignarRol(rolId);

        result.IsSuccess.Should().BeTrue();
        usuario.Roles.Should().ContainSingle(r => r.RolId == rolId);
    }

    [Fact]
    public void AsignarRol_RolYaAsignado_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();
        var rolId = Guid.NewGuid();
        usuario.AsignarRol(rolId);

        var result = usuario.AsignarRol(rolId);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void QuitarRol_ConUnSoloRol_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();
        var rolId = Guid.NewGuid();
        usuario.AsignarRol(rolId);

        var result = usuario.QuitarRol(rolId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("al menos un rol");
    }

    [Fact]
    public void QuitarRol_ConMultiplesRoles_DebeQuitarloCorrectamente()
    {
        var usuario = CrearUsuarioActivo();
        var rolId1 = Guid.NewGuid();
        var rolId2 = Guid.NewGuid();
        usuario.AsignarRol(rolId1);
        usuario.AsignarRol(rolId2);

        var result = usuario.QuitarRol(rolId1);

        result.IsSuccess.Should().BeTrue();
        usuario.Roles.Should().ContainSingle(r => r.RolId == rolId2);
    }

    // ─── 2FA ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ActivarDosFactores_DebeActivarloConCodigosRecuperacion()
    {
        var usuario = CrearUsuarioActivo();
        var codigos = new[] { "hash1", "hash2", "hash3" };

        var result = usuario.ActivarDosFactores("secretkey123", codigos);

        result.IsSuccess.Should().BeTrue();
        usuario.TwoFactorActivo.Should().BeTrue();
        usuario.TwoFactorSecretKey.Should().Be("secretkey123");
        usuario.CodigosRecuperacion.Should().HaveCount(3);
    }

    [Fact]
    public void ActivarDosFactores_YaActivo_DebeFallar()
    {
        var usuario = CrearUsuarioActivo();
        usuario.ActivarDosFactores("secretkey", ["hash1"]);

        var result = usuario.ActivarDosFactores("secretkey2", ["hash2"]);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void DesactivarDosFactores_Activo_DebeDesactivarlo()
    {
        var usuario = CrearUsuarioActivo();
        usuario.ActivarDosFactores("secretkey", ["hash1"]);

        var result = usuario.DesactivarDosFactores();

        result.IsSuccess.Should().BeTrue();
        usuario.TwoFactorActivo.Should().BeFalse();
        usuario.TwoFactorSecretKey.Should().BeNull();
    }
}
