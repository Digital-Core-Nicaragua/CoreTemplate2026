using CoreTemplate.Storage.Abstractions;
using CoreTemplate.Storage.Settings;
using CoreTemplate.Storage.Validation;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Modules.Archivos.Tests;

public class ArchivoValidatorTests
{
    private readonly ArchivoValidator _validator;

    public ArchivoValidatorTests()
    {
        var settings = Options.Create(new StorageSettings
        {
            MaxTamanioMB = 5,
            TiposPermitidos = ["application/pdf", "image/jpeg", "image/png"]
        });
        _validator = new ArchivoValidator(settings);
    }

    [Fact]
    public void Validar_PdfValido_RetornaNull()
    {
        var request = new SubirArchivoRequest(
            new MemoryStream(new byte[100]),
            "documento.pdf", "rrhh/cv", "application/pdf");

        var result = _validator.Validar(request);

        result.Should().BeNull();
    }

    [Fact]
    public void Validar_TipoNoPermitido_RetornaError()
    {
        var request = new SubirArchivoRequest(
            new MemoryStream(new byte[100]),
            "script.exe", "rrhh/cv", "application/x-msdownload");

        var result = _validator.Validar(request);

        result.Should().NotBeNull();
        result!.Exitoso.Should().BeFalse();
        result.Error.Should().Contain("no permitido");
    }

    [Fact]
    public void Validar_TamanioExcedido_RetornaError()
    {
        var tamanioExcedido = 6 * 1024 * 1024; // 6 MB > 5 MB límite
        var stream = new MemoryStream(new byte[tamanioExcedido]);

        var request = new SubirArchivoRequest(
            stream, "grande.pdf", "rrhh/cv", "application/pdf");

        var result = _validator.Validar(request);

        result.Should().NotBeNull();
        result!.Exitoso.Should().BeFalse();
        result.Error.Should().Contain("tamaño");
    }

    [Fact]
    public void Validar_ExtensionInconsistente_RetornaError()
    {
        var request = new SubirArchivoRequest(
            new MemoryStream(new byte[100]),
            "imagen.exe", "rrhh/cv", "image/jpeg");

        var result = _validator.Validar(request);

        result.Should().NotBeNull();
        result!.Exitoso.Should().BeFalse();
    }
}
