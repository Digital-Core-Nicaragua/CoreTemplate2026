using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Modules.PdfTemplates.Domain.Events;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.PdfTemplates.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa la configuración corporativa de una plantilla PDF.
/// Almacena los datos de marca (logo, colores, textos) que el diseño usa para renderizar.
/// El diseño en sí (estructura visual) vive en código C# como IPdfDocumentTemplate.
/// Implementa IHasTenant para aislamiento automático por tenant.
/// </summary>
public sealed class PdfPlantilla : AggregateRoot<Guid>, IHasTenant
{
    public Guid? TenantId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Modulo { get; private set; } = string.Empty;

    /// <summary>Código del diseño en código C#. Ej: "vertical-estandar", "moderno"</summary>
    public string CodigoTemplate { get; private set; } = string.Empty;

    // ─── Marca corporativa ────────────────────────────────────────────────────
    public string NombreEmpresa { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }
    public string ColorEncabezado { get; private set; } = "#1a2e5a";
    public string ColorTextoHeader { get; private set; } = "#ffffff";
    public string ColorAcento { get; private set; } = "#4f46e5";
    public string? TextoSecundario { get; private set; }

    // ─── Pie de página ────────────────────────────────────────────────────────
    public string? TextoPiePagina { get; private set; }
    public bool MostrarNumeroPagina { get; private set; } = true;
    public bool MostrarFechaGeneracion { get; private set; } = true;

    // ─── Opciones ─────────────────────────────────────────────────────────────
    public string? MarcaDeAgua { get; private set; }
    public bool EsDeSistema { get; private set; }
    public bool EsActivo { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime? ModificadoEn { get; private set; }
    public Guid? ModificadoPor { get; private set; }

    private PdfPlantilla() { }

    public static Result<PdfPlantilla> Crear(
        string codigo, string nombre, string modulo, string codigoTemplate,
        string nombreEmpresa, string? logoUrl,
        string colorEncabezado, string colorTextoHeader, string colorAcento,
        string? textoSecundario, string? textoPiePagina,
        bool mostrarNumeroPagina, bool mostrarFechaGeneracion,
        string? marcaDeAgua, bool esDeSistema = false, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Result<PdfPlantilla>.Failure("El código es requerido.");
        if (string.IsNullOrWhiteSpace(codigoTemplate))
            return Result<PdfPlantilla>.Failure("El código de diseño es requerido.");
        if (string.IsNullOrWhiteSpace(nombreEmpresa))
            return Result<PdfPlantilla>.Failure("El nombre de empresa es requerido.");

        var plantilla = new PdfPlantilla
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo.Trim().ToLowerInvariant(),
            Nombre = nombre.Trim(),
            Modulo = modulo.Trim(),
            CodigoTemplate = codigoTemplate.Trim().ToLowerInvariant(),
            NombreEmpresa = nombreEmpresa.Trim(),
            LogoUrl = logoUrl?.Trim(),
            ColorEncabezado = colorEncabezado,
            ColorTextoHeader = colorTextoHeader,
            ColorAcento = colorAcento,
            TextoSecundario = textoSecundario?.Trim(),
            TextoPiePagina = textoPiePagina?.Trim(),
            MostrarNumeroPagina = mostrarNumeroPagina,
            MostrarFechaGeneracion = mostrarFechaGeneracion,
            MarcaDeAgua = marcaDeAgua?.Trim(),
            EsDeSistema = esDeSistema,
            EsActivo = true,
            CreadoEn = DateTime.UtcNow
        };

        plantilla.RaiseDomainEvent(new PlantillaPdfCreada(plantilla.Id, plantilla.Codigo, plantilla.Modulo));
        return Result<PdfPlantilla>.Success(plantilla);
    }

    public Result Actualizar(
        string nombre, string codigoTemplate, string nombreEmpresa, string? logoUrl,
        string colorEncabezado, string colorTextoHeader, string colorAcento,
        string? textoSecundario, string? textoPiePagina,
        bool mostrarNumeroPagina, bool mostrarFechaGeneracion,
        string? marcaDeAgua, Guid modificadoPor)
    {
        if (string.IsNullOrWhiteSpace(codigoTemplate))
            return Result.Failure("El código de diseño es requerido.");
        if (string.IsNullOrWhiteSpace(nombreEmpresa))
            return Result.Failure("El nombre de empresa es requerido.");

        Nombre = nombre.Trim();
        CodigoTemplate = codigoTemplate.Trim().ToLowerInvariant();
        NombreEmpresa = nombreEmpresa.Trim();
        LogoUrl = logoUrl?.Trim();
        ColorEncabezado = colorEncabezado;
        ColorTextoHeader = colorTextoHeader;
        ColorAcento = colorAcento;
        TextoSecundario = textoSecundario?.Trim();
        TextoPiePagina = textoPiePagina?.Trim();
        MostrarNumeroPagina = mostrarNumeroPagina;
        MostrarFechaGeneracion = mostrarFechaGeneracion;
        MarcaDeAgua = marcaDeAgua?.Trim();
        ModificadoEn = DateTime.UtcNow;
        ModificadoPor = modificadoPor;

        RaiseDomainEvent(new PlantillaPdfActualizada(Id, Codigo, modificadoPor));
        return Result.Success();
    }

    public Result Activar()
    {
        if (EsActivo) return Result.Failure("La plantilla ya está activa.");
        EsActivo = true;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new PlantillaPdfActivada(Id, Codigo));
        return Result.Success();
    }

    public Result Desactivar()
    {
        if (!EsActivo) return Result.Failure("La plantilla ya está inactiva.");
        EsActivo = false;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new PlantillaPdfDesactivada(Id, Codigo));
        return Result.Success();
    }
}
