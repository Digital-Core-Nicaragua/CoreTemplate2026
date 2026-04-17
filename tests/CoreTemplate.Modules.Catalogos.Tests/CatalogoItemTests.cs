using CoreTemplate.Modules.Catalogos.Application.Commands;
using CoreTemplate.Modules.Catalogos.Domain.Aggregates;
using CoreTemplate.Modules.Catalogos.Domain.Events;
using CoreTemplate.Modules.Catalogos.Domain.Repositories;
using CoreTemplate.SharedKernel.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace CoreTemplate.Modules.Catalogos.Tests;

public sealed class CatalogoItemTests
{
    // ─── Crear ────────────────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearItemActivo()
    {
        var result = CatalogoItem.Crear("COD001", "Ítem de prueba", "Descripción");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Codigo.Should().Be("COD001");
        result.Value.Nombre.Should().Be("Ítem de prueba");
        result.Value.Descripcion.Should().Be("Descripción");
        result.Value.EsActivo.Should().BeTrue();
    }

    [Fact]
    public void Crear_CodigoDebeNormalizarseAMayusculas()
    {
        var result = CatalogoItem.Crear("cod001", "Nombre");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Codigo.Should().Be("COD001");
    }

    [Fact]
    public void Crear_DebeDispararEventoCatalogoItemCreado()
    {
        var result = CatalogoItem.Crear("COD001", "Nombre");

        result.Value!.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CatalogoItemCreadoEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConCodigoVacio_DebeFallar(string codigo)
    {
        var result = CatalogoItem.Crear(codigo, "Nombre");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConCodigoMayorA50Caracteres_DebeFallar()
    {
        var codigoLargo = new string('A', 51);

        var result = CatalogoItem.Crear(codigoLargo, "Nombre");

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConNombreVacio_DebeFallar(string nombre)
    {
        var result = CatalogoItem.Crear("COD001", nombre);

        result.IsSuccess.Should().BeFalse();
    }

    // ─── Activar / Desactivar ─────────────────────────────────────────────────

    [Fact]
    public void Desactivar_ItemActivo_DebeDesactivarlo()
    {
        var item = CatalogoItem.Crear("COD001", "Nombre").Value!;
        item.ClearDomainEvents();

        var result = item.Desactivar();

        result.IsSuccess.Should().BeTrue();
        item.EsActivo.Should().BeFalse();
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CatalogoItemDesactivadoEvent>();
    }

    [Fact]
    public void Desactivar_ItemYaInactivo_DebeFallar()
    {
        var item = CatalogoItem.Crear("COD001", "Nombre").Value!;
        item.Desactivar();

        var result = item.Desactivar();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Activar_ItemInactivo_DebeActivarlo()
    {
        var item = CatalogoItem.Crear("COD001", "Nombre").Value!;
        item.Desactivar();
        item.ClearDomainEvents();

        var result = item.Activar();

        result.IsSuccess.Should().BeTrue();
        item.EsActivo.Should().BeTrue();
        item.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CatalogoItemActivadoEvent>();
    }

    [Fact]
    public void Activar_ItemYaActivo_DebeFallar()
    {
        var item = CatalogoItem.Crear("COD001", "Nombre").Value!;

        var result = item.Activar();

        result.IsSuccess.Should().BeFalse();
    }
}

public sealed class CrearCatalogoItemCommandHandlerTests
{
    private readonly ICatalogoItemRepository _repo = Substitute.For<ICatalogoItemRepository>();
    private readonly ICurrentTenant _currentTenant = Substitute.For<ICurrentTenant>();

    private CrearCatalogoItemCommandHandler CrearHandler() => new(_repo, _currentTenant);

    [Fact]
    public async Task Handle_DatosValidos_DebeCrearItem()
    {
        var cmd = new CrearCatalogoItemCommand("COD001", "Ítem de prueba", "Descripción");

        _repo.ExistsByCodigoAsync("COD001", null, default).Returns(false);
        _currentTenant.TenantId.Returns((Guid?)null);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<CatalogoItem>(), default);
    }

    [Fact]
    public async Task Handle_CodigoYaExiste_DebeFallar()
    {
        var cmd = new CrearCatalogoItemCommand("COD001", "Ítem", null);

        _repo.ExistsByCodigoAsync("COD001", null, default).Returns(true);
        _currentTenant.TenantId.Returns((Guid?)null);

        var handler = CrearHandler();
        var result = await handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("código");
        await _repo.DidNotReceive().AddAsync(Arg.Any<CatalogoItem>(), default);
    }

    [Fact]
    public async Task Handle_ConTenantId_DebeAsignarloCorrecto()
    {
        var tenantId = Guid.NewGuid();
        var cmd = new CrearCatalogoItemCommand("COD001", "Ítem", null);

        _repo.ExistsByCodigoAsync("COD001", tenantId, default).Returns(false);
        _currentTenant.TenantId.Returns(tenantId);

        CatalogoItem? itemGuardado = null;
        await _repo.AddAsync(Arg.Do<CatalogoItem>(i => itemGuardado = i), default);

        var handler = CrearHandler();
        await handler.Handle(cmd, default);

        itemGuardado.Should().NotBeNull();
        itemGuardado!.TenantId.Should().Be(tenantId);
    }
}
