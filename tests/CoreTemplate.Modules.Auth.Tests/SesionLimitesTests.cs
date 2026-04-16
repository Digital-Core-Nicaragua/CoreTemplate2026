using CoreTemplate.Modules.Auth.Application.Abstractions;
using CoreTemplate.Modules.Auth.Domain.Enums;
using CoreTemplate.Modules.Auth.Domain.Repositories;
using CoreTemplate.Modules.Auth.Infrastructure.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class SesionLimitesTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IConfiguracionTenantRepository _configTenantRepo = Substitute.For<IConfiguracionTenantRepository>();
    private readonly CoreTemplate.Infrastructure.Services.ICurrentTenant _currentTenant =
        Substitute.For<CoreTemplate.Infrastructure.Services.ICurrentTenant>();

    private SesionService CrearServicio(int maxSesiones = 3,
        AccionAlLlegarLimiteSesiones accion = AccionAlLlegarLimiteSesiones.CerrarMasAntigua)
    {
        var authSettings = Options.Create(new AuthSettings
        {
            MaxSesionesSimultaneas = maxSesiones,
            AccionAlLlegarLimiteSesiones = accion
        });
        var tenantSettings = Options.Create(new CoreTemplate.Infrastructure.Settings.TenantSettings
        {
            IsMultiTenant = false,
            EnableSessionLimitsPerTenant = false
        });
        return new SesionService(_sesionRepo, _configTenantRepo, _currentTenant, authSettings, tenantSettings);
    }

    [Fact]
    public async Task VerificarLimite_BajoLimite_DebePermitirNuevaSesion()
    {
        var usuarioId = Guid.NewGuid();
        _sesionRepo.ContarActivasAsync(usuarioId, default).Returns(2);

        var servicio = CrearServicio(maxSesiones: 3);
        var resultado = await servicio.VerificarYAplicarLimiteAsync(usuarioId, TipoUsuario.Humano);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task VerificarLimite_AlLimite_CerrarMasAntigua_DebePermitirYCerrarAntigua()
    {
        var usuarioId = Guid.NewGuid();
        var sesionAntigua = CoreTemplate.Modules.Auth.Domain.Aggregates.Sesion.Crear(
            usuarioId, null, "hash", DateTime.UtcNow.AddDays(7),
            CoreTemplate.Modules.Auth.Domain.Enums.CanalAcceso.Web, "127.0.0.1", "Agent");

        _sesionRepo.ContarActivasAsync(usuarioId, default).Returns(3);
        _sesionRepo.GetMasAntiguaActivaAsync(usuarioId, default).Returns(sesionAntigua);

        var servicio = CrearServicio(maxSesiones: 3, accion: AccionAlLlegarLimiteSesiones.CerrarMasAntigua);
        var resultado = await servicio.VerificarYAplicarLimiteAsync(usuarioId, TipoUsuario.Humano);

        resultado.Should().BeTrue();
        sesionAntigua.EsActiva.Should().BeFalse();
        await _sesionRepo.Received(1).UpdateAsync(sesionAntigua, default);
    }

    [Fact]
    public async Task VerificarLimite_AlLimite_BloquearNuevoLogin_DebeRechazar()
    {
        var usuarioId = Guid.NewGuid();
        _sesionRepo.ContarActivasAsync(usuarioId, default).Returns(3);

        var servicio = CrearServicio(maxSesiones: 3, accion: AccionAlLlegarLimiteSesiones.BloquearNuevoLogin);
        var resultado = await servicio.VerificarYAplicarLimiteAsync(usuarioId, TipoUsuario.Humano);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task VerificarLimite_UsuarioSistema_SiemprePermite()
    {
        var usuarioId = Guid.NewGuid();
        _sesionRepo.ContarActivasAsync(usuarioId, default).Returns(100);

        var servicio = CrearServicio(maxSesiones: 1);
        var resultado = await servicio.VerificarYAplicarLimiteAsync(usuarioId, TipoUsuario.Sistema);

        resultado.Should().BeTrue();
        await _sesionRepo.DidNotReceive().ContarActivasAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerificarLimite_UsuarioIntegracion_SiemprePermite()
    {
        var usuarioId = Guid.NewGuid();

        var servicio = CrearServicio(maxSesiones: 1);
        var resultado = await servicio.VerificarYAplicarLimiteAsync(usuarioId, TipoUsuario.Integracion);

        resultado.Should().BeTrue();
    }
}
