using CoreTemplate.Modules.Archivos.Domain.Aggregates;

namespace CoreTemplate.Modules.Archivos.Tests;

public class ArchivoAdjuntoTests
{
    private static ArchivoAdjunto CrearArchivo() =>
        ArchivoAdjunto.Crear(
            "cv-juan.pdf", "guid.pdf", "rrhh/cv/guid.pdf",
            "https://server/archivos/rrhh/cv/guid.pdf",
            "application/pdf", 102400, "Local",
            "rrhh/candidatos/cv", "RRHH",
            Guid.NewGuid()).Value!;

    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var result = ArchivoAdjunto.Crear(
            "cv-juan.pdf", "guid.pdf", "rrhh/cv/guid.pdf",
            "https://server/archivos/rrhh/cv/guid.pdf",
            "application/pdf", 102400, "Local",
            "rrhh/candidatos/cv", "RRHH",
            Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value!.NombreOriginal.Should().Be("cv-juan.pdf");
        result.Value.EsActivo.Should().BeTrue();
        result.Value.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Crear_SinNombreOriginal_RetornaFallo()
    {
        var result = ArchivoAdjunto.Crear(
            "", "guid.pdf", "rrhh/cv/guid.pdf",
            "https://url", "application/pdf", 100,
            "Local", "rrhh/cv", "RRHH", Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Eliminar_ArchivoActivo_DesactivaYPublicaEvento()
    {
        var archivo = CrearArchivo();
        archivo.ClearDomainEvents();

        var result = archivo.Eliminar();

        result.IsSuccess.Should().BeTrue();
        archivo.EsActivo.Should().BeFalse();
        archivo.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Eliminar_ArchivoYaEliminado_RetornaFallo()
    {
        var archivo = CrearArchivo();
        archivo.Eliminar();

        var result = archivo.Eliminar();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ActualizarUrl_UrlValida_ActualizaCorrectamente()
    {
        var archivo = CrearArchivo();
        var nuevaUrl = "https://s3.amazonaws.com/bucket/rrhh/cv/guid.pdf?X-Amz=...";

        var result = archivo.ActualizarUrl(nuevaUrl);

        result.IsSuccess.Should().BeTrue();
        archivo.Url.Should().Be(nuevaUrl);
    }

    [Fact]
    public void ActualizarUrl_UrlVacia_RetornaFallo()
    {
        var archivo = CrearArchivo();

        var result = archivo.ActualizarUrl("");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConTenantId_AsignaTenantCorrectamente()
    {
        var tenantId = Guid.NewGuid();
        var result = ArchivoAdjunto.Crear(
            "doc.pdf", "guid.pdf", "ruta/guid.pdf",
            "https://url", "application/pdf", 100,
            "Local", "contexto", "RRHH",
            Guid.NewGuid(), null, tenantId);

        result.Value!.TenantId.Should().Be(tenantId);
    }
}
