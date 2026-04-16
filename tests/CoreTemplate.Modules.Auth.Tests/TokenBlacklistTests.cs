using CoreTemplate.Modules.Auth.Infrastructure.Services;

namespace CoreTemplate.Modules.Auth.Tests;

public sealed class TokenBlacklistTests
{
    [Fact]
    public async Task Agregar_YVerificar_DebeEncontrarToken()
    {
        var service = new InMemoryTokenBlacklistService();
        var jti = Guid.NewGuid().ToString();

        await service.AgregarAsync(jti, TimeSpan.FromMinutes(15));
        var estaEnBlacklist = await service.EstaEnBlacklistAsync(jti);

        estaEnBlacklist.Should().BeTrue();
    }

    [Fact]
    public async Task Verificar_TokenNoAgregado_DebeRetornarFalso()
    {
        var service = new InMemoryTokenBlacklistService();

        var estaEnBlacklist = await service.EstaEnBlacklistAsync("jti-inexistente");

        estaEnBlacklist.Should().BeFalse();
    }

    [Fact]
    public async Task Verificar_TokenExpirado_DebeRetornarFalso()
    {
        var service = new InMemoryTokenBlacklistService();
        var jti = Guid.NewGuid().ToString();

        // TTL de 1 milisegundo — ya expiró al verificar
        await service.AgregarAsync(jti, TimeSpan.FromMilliseconds(1));
        await Task.Delay(10);

        var estaEnBlacklist = await service.EstaEnBlacklistAsync(jti);

        estaEnBlacklist.Should().BeFalse();
    }

    [Fact]
    public async Task Agregar_MultiplesTokens_DebeGestionarlosIndependientemente()
    {
        var service = new InMemoryTokenBlacklistService();
        var jti1 = Guid.NewGuid().ToString();
        var jti2 = Guid.NewGuid().ToString();

        await service.AgregarAsync(jti1, TimeSpan.FromMinutes(15));

        (await service.EstaEnBlacklistAsync(jti1)).Should().BeTrue();
        (await service.EstaEnBlacklistAsync(jti2)).Should().BeFalse();
    }
}
