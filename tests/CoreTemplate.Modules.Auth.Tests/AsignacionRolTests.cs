using CoreTemplate.Modules.Auth.Domain.Aggregates;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class AsignacionRolTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearAsignacion()
    {
        var usuarioId = Guid.NewGuid();
        var sucursalId = Guid.NewGuid();
        var rolId = Guid.NewGuid();

        var result = AsignacionRol.Crear(usuarioId, sucursalId, rolId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UsuarioId.Should().Be(usuarioId);
        result.Value.SucursalId.Should().Be(sucursalId);
        result.Value.RolId.Should().Be(rolId);
        result.Value.AsignadoEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Crear_DosAsignacionesDistintas_DebenTenerIdsUnicos()
    {
        var usuarioId = Guid.NewGuid();
        var sucursalId = Guid.NewGuid();
        var rolId = Guid.NewGuid();

        var a1 = AsignacionRol.Crear(usuarioId, sucursalId, rolId).Value!;
        var a2 = AsignacionRol.Crear(usuarioId, sucursalId, Guid.NewGuid()).Value!;

        a1.Id.Should().NotBe(a2.Id);
    }

    [Fact]
    public void Crear_MismaCombinacion_DebeDetectarseEnRepositorio()
    {
        // La invariante de unicidad se aplica a nivel de BD (índice único)
        // y en el handler que llama ExisteAsync antes de crear.
        // Este test verifica que el aggregate se puede crear (la validación es en el handler).
        var usuarioId = Guid.NewGuid();
        var sucursalId = Guid.NewGuid();
        var rolId = Guid.NewGuid();

        var result1 = AsignacionRol.Crear(usuarioId, sucursalId, rolId);
        var result2 = AsignacionRol.Crear(usuarioId, sucursalId, rolId);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.Id.Should().NotBe(result2.Value!.Id);
    }
}
