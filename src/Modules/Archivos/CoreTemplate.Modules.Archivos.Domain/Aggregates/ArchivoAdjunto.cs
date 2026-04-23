using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Modules.Archivos.Domain.Events;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Archivos.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa los metadatos de un archivo almacenado en el sistema.
/// <para>
/// Este aggregate NO almacena el contenido del archivo — solo sus metadatos.
/// El contenido físico lo gestiona el building block <c>CoreTemplate.Storage</c>
/// a través de <c>IStorageService</c>.
/// </para>
/// <para>
/// Implementa <see cref="IHasTenant"/> para que el <c>BaseDbContext</c> aplique
/// automáticamente el filtro de tenant en modo multi-tenant. Cada empresa
/// solo ve sus propios archivos.
/// </para>
/// <para>
/// Los módulos consumidores (RRHH, Contabilidad, Nómina) guardan el <c>Id</c>
/// de este aggregate en sus propias entidades y consultan la URL cuando la necesitan.
/// </para>
/// </summary>
public sealed class ArchivoAdjunto : AggregateRoot<Guid>, IHasTenant
{
    /// <summary>ID del tenant propietario. Null si el sistema opera en modo single-tenant.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Nombre original del archivo tal como lo subió el usuario. Ej: "cv-juan-perez.pdf"</summary>
    public string NombreOriginal { get; private set; } = string.Empty;

    /// <summary>Nombre generado para almacenamiento (GUID + extensión). Garantiza unicidad. Ej: "3f2a1b4c-....pdf"</summary>
    public string NombreAlmacenado { get; private set; } = string.Empty;

    /// <summary>
    /// Ruta interna del archivo en el proveedor. Ej: "rrhh/candidatos/cv/3f2a1b4c-....pdf"
    /// Guardar este valor en la entidad del módulo consumidor para obtener la URL posteriormente.
    /// </summary>
    public string RutaAlmacenada { get; private set; } = string.Empty;

    /// <summary>
    /// URL de acceso al archivo. Para S3 es una URL firmada que puede expirar.
    /// Usar <c>GetArchivoUrlQuery</c> para obtener siempre una URL vigente.
    /// </summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>Tipo MIME del archivo. Ej: "application/pdf", "image/jpeg"</summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>Tamaño del archivo en bytes.</summary>
    public long TamanioBytes { get; private set; }

    /// <summary>Proveedor que almacenó el archivo: "Local", "S3" o "Firebase".</summary>
    public string Proveedor { get; private set; } = string.Empty;

    /// <summary>
    /// Carpeta lógica donde se almacenó el archivo.
    /// Ej: "rrhh/candidatos/cv", "contabilidad/facturas/2025"
    /// </summary>
    public string Contexto { get; private set; } = string.Empty;

    /// <summary>Módulo que originó la subida. Ej: "RRHH", "Contabilidad", "Nomina"</summary>
    public string ModuloOrigen { get; private set; } = string.Empty;

    /// <summary>
    /// ID de la entidad del módulo consumidor a la que pertenece este archivo.
    /// Ej: CandidatoId, FacturaId, EmpleadoId. Permite listar todos los archivos de una entidad.
    /// </summary>
    public Guid? EntidadId { get; private set; }

    /// <summary>ID del usuario que subió el archivo.</summary>
    public Guid SubidoPor { get; private set; }

    /// <summary>Fecha y hora UTC en que se subió el archivo.</summary>
    public DateTime FechaSubida { get; private set; }

    /// <summary>Indica si el archivo está activo. Soft delete — los archivos eliminados quedan con EsActivo = false.</summary>
    public bool EsActivo { get; private set; }

    private ArchivoAdjunto() { }

    /// <summary>
    /// Crea un nuevo registro de archivo adjunto con sus metadatos.
    /// Llamar después de que <c>IStorageService.SubirAsync</c> retorne exitosamente.
    /// </summary>
    /// <param name="nombreOriginal">Nombre original del archivo.</param>
    /// <param name="nombreAlmacenado">Nombre generado (GUID + extensión) retornado por el storage.</param>
    /// <param name="rutaAlmacenada">Ruta interna retornada por el storage. Guardar en la entidad del módulo.</param>
    /// <param name="url">URL de acceso retornada por el storage.</param>
    /// <param name="contentType">Tipo MIME del archivo.</param>
    /// <param name="tamanioBytes">Tamaño en bytes retornado por el storage.</param>
    /// <param name="proveedor">Proveedor que almacenó el archivo.</param>
    /// <param name="contexto">Carpeta lógica usada al subir.</param>
    /// <param name="moduloOrigen">Módulo que originó la subida.</param>
    /// <param name="subidoPor">ID del usuario que subió el archivo.</param>
    /// <param name="entidadId">ID de la entidad relacionada en el módulo consumidor.</param>
    /// <param name="tenantId">ID del tenant. Null si single-tenant.</param>
    public static Result<ArchivoAdjunto> Crear(
        string nombreOriginal,
        string nombreAlmacenado,
        string rutaAlmacenada,
        string url,
        string contentType,
        long tamanioBytes,
        string proveedor,
        string contexto,
        string moduloOrigen,
        Guid subidoPor,
        Guid? entidadId = null,
        Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(nombreOriginal))
            return Result<ArchivoAdjunto>.Failure("El nombre original del archivo es requerido.");

        if (string.IsNullOrWhiteSpace(rutaAlmacenada))
            return Result<ArchivoAdjunto>.Failure("La ruta almacenada es requerida.");

        var archivo = new ArchivoAdjunto
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NombreOriginal = nombreOriginal.Trim(),
            NombreAlmacenado = nombreAlmacenado,
            RutaAlmacenada = rutaAlmacenada,
            Url = url,
            ContentType = contentType,
            TamanioBytes = tamanioBytes,
            Proveedor = proveedor,
            Contexto = contexto,
            ModuloOrigen = moduloOrigen,
            EntidadId = entidadId,
            SubidoPor = subidoPor,
            FechaSubida = DateTime.UtcNow,
            EsActivo = true
        };

        archivo.RaiseDomainEvent(new ArchivoSubido(
            archivo.Id, archivo.Url, archivo.RutaAlmacenada, archivo.Proveedor, archivo.TamanioBytes));

        return Result<ArchivoAdjunto>.Success(archivo);
    }

    /// <summary>
    /// Actualiza la URL del archivo. Útil cuando la URL firmada de S3 expira
    /// y se regenera una nueva mediante <c>GetArchivoUrlQuery</c>.
    /// </summary>
    public Result ActualizarUrl(string nuevaUrl)
    {
        if (string.IsNullOrWhiteSpace(nuevaUrl))
            return Result.Failure("La URL no puede estar vacía.");
        Url = nuevaUrl;
        return Result.Success();
    }

    /// <summary>
    /// Marca el archivo como eliminado (soft delete).
    /// El registro permanece en BD pero EsActivo = false.
    /// El archivo físico debe eliminarse del proveedor por separado via <c>IStorageService</c>.
    /// </summary>
    public Result Eliminar()
    {
        if (!EsActivo) return Result.Failure("El archivo ya está eliminado.");
        EsActivo = false;
        RaiseDomainEvent(new ArchivoEliminado(Id, RutaAlmacenada, Proveedor));
        return Result.Success();
    }
}
