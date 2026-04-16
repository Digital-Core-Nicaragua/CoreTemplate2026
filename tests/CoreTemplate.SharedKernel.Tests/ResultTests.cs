using CoreTemplate.SharedKernel;
using FluentAssertions;

namespace CoreTemplate.SharedKernel.Tests;

public sealed class ResultTests
{
    // ─── Result (sin valor) ───────────────────────────────────────────────────

    [Fact]
    public void Success_DebeCrearResultadoExitoso()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_DebeCrearResultadoFallido()
    {
        var result = Result.Failure("Error de prueba.");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Error de prueba.");
    }

    // ─── Result<T> ────────────────────────────────────────────────────────────

    [Fact]
    public void SuccessGenerico_DebeCrearResultadoExitosoConValor()
    {
        var result = Result<int>.Success(42, "Operación exitosa.");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Message.Should().Be("Operación exitosa.");
        result.Errors.Should().BeEmpty();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void FailureGenerico_DebeCrearResultadoFallidoConErrores()
    {
        var result = Result<int>.Failure("Error 1.", "Error 2.");

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(default);
        result.Errors.Should().HaveCount(2);
        result.Error.Should().Be("Error 1.");
        result.Message.Should().Be("Error 1.");
    }

    [Fact]
    public void FailureGenerico_ConUnSoloError_DebeRetornarloComoError()
    {
        var result = Result<string>.Failure("Único error.");

        result.Error.Should().Be("Único error.");
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void SuccessGenerico_SinMensaje_DebeUsarMensajeVacio()
    {
        var result = Result<bool>.Success(true);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().BeEmpty();
    }
}
