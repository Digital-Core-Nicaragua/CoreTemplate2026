using CoreTemplate.SharedKernel;
using FluentAssertions;

namespace CoreTemplate.SharedKernel.Tests;

public sealed class PagedResultTests
{
    [Fact]
    public void TotalPaginas_DebeCalcularseCorrectamente()
    {
        var result = new PagedResult<string>([], 1, 10, 25);

        result.TotalPaginas.Should().Be(3);
    }

    [Fact]
    public void TotalPaginas_CuandoTotalEsExacto_NoDebeRedondearArriba()
    {
        var result = new PagedResult<string>([], 1, 10, 20);

        result.TotalPaginas.Should().Be(2);
    }

    [Fact]
    public void TotalPaginas_CuandoTotalEsCero_DebeSerCero()
    {
        var result = new PagedResult<string>([], 1, 10, 0);

        result.TotalPaginas.Should().Be(0);
    }

    [Fact]
    public void TienePaginaAnterior_EnPrimeraPagina_DebeSerFalso()
    {
        var result = new PagedResult<string>([], 1, 10, 30);

        result.TienePaginaAnterior.Should().BeFalse();
    }

    [Fact]
    public void TienePaginaAnterior_EnSegundaPagina_DebeSerVerdadero()
    {
        var result = new PagedResult<string>([], 2, 10, 30);

        result.TienePaginaAnterior.Should().BeTrue();
    }

    [Fact]
    public void TienePaginaSiguiente_EnUltimaPagina_DebeSerFalso()
    {
        var result = new PagedResult<string>([], 3, 10, 30);

        result.TienePaginaSiguiente.Should().BeFalse();
    }

    [Fact]
    public void TienePaginaSiguiente_EnPrimeraPagina_DebeSerVerdadero()
    {
        var result = new PagedResult<string>([], 1, 10, 30);

        result.TienePaginaSiguiente.Should().BeTrue();
    }

    [Fact]
    public void Items_DebeContenerLosElementosIndicados()
    {
        var items = new List<string> { "a", "b", "c" };
        var result = new PagedResult<string>(items, 1, 10, 3);

        result.Items.Should().BeEquivalentTo(items);
        result.Total.Should().Be(3);
        result.Pagina.Should().Be(1);
        result.TamanoPagina.Should().Be(10);
    }
}
