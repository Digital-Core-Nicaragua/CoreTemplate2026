using CoreTemplate.SharedKernel.Domain;
using FluentAssertions;

namespace CoreTemplate.SharedKernel.Tests;

// Value Object de prueba
file sealed class Dinero : ValueObject
{
    public decimal Monto { get; }
    public string Moneda { get; }

    public Dinero(decimal monto, string moneda)
    {
        Monto = monto;
        Moneda = moneda;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Monto;
        yield return Moneda;
    }
}

public sealed class ValueObjectTests
{
    [Fact]
    public void DosValueObjects_ConMismosValores_DebenSerIguales()
    {
        var a = new Dinero(100m, "USD");
        var b = new Dinero(100m, "USD");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void DosValueObjects_ConDistintosValores_NoDebenSerIguales()
    {
        var a = new Dinero(100m, "USD");
        var b = new Dinero(200m, "USD");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void DosValueObjects_ConDistintaMoneda_NoDebenSerIguales()
    {
        var a = new Dinero(100m, "USD");
        var b = new Dinero(100m, "EUR");

        a.Should().NotBe(b);
    }

    [Fact]
    public void ValueObject_ComparadoConNull_NoDebeSerIgual()
    {
        var a = new Dinero(100m, "USD");

        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_ComparadoConDistintoTipo_NoDebeSerIgual()
    {
        var a = new Dinero(100m, "USD");

        a.Equals("no soy un ValueObject").Should().BeFalse();
    }
}
