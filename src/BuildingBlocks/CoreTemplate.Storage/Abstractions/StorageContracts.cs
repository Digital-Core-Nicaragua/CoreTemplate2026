namespace CoreTemplate.Storage.Abstractions;

/// <summary>
/// Datos necesarios para subir un archivo al proveedor de almacenamiento.
/// </summary>
/// <param name="Contenido">Stream del archivo. Se usa streaming para soportar archivos grandes sin cargarlos en memoria.</param>
/// <param name="NombreOriginal">Nombre original del archivo con extensión. Ej: "cv-juan-perez.pdf"</param>
/// <param name="Contexto">Carpeta lógica de destino. Ej: "rrhh/candidatos/cv", "contabilidad/facturas/2025"</param>
/// <param name="ContentType">Tipo MIME del archivo. Ej: "application/pdf", "image/jpeg"</param>
public record SubirArchivoRequest(
    Stream Contenido,
    string NombreOriginal,
    string Contexto,
    string ContentType);

/// <summary>
/// Resultado de una operación de almacenamiento.
/// <para>
/// <c>IStorageService</c> nunca lanza excepciones al consumidor — los errores
/// se encapsulan en este resultado. El consumidor decide si el fallo es crítico.
/// </para>
/// </summary>
/// <param name="Exitoso">Indica si la operación fue exitosa.</param>
/// <param name="Url">URL para visualizar o descargar el archivo. Firmada con expiración si el proveedor es S3.</param>
/// <param name="RutaAlmacenada">Ruta interna del archivo. Guardar en BD del módulo para futuras consultas. Ej: "rrhh/candidatos/cv/{guid}.pdf"</param>
/// <param name="Proveedor">Proveedor que almacenó el archivo: "Local", "S3" o "Firebase".</param>
/// <param name="TamanioBytes">Tamaño del archivo en bytes.</param>
/// <param name="Error">Descripción del error si <c>Exitoso = false</c>.</param>
public record StorageResult(
    bool Exitoso,
    string? Url = null,
    string? RutaAlmacenada = null,
    string? Proveedor = null,
    long TamanioBytes = 0,
    string? Error = null)
{
    /// <summary>Crea un resultado exitoso con todos los datos del archivo almacenado.</summary>
    public static StorageResult Ok(string url, string ruta, string proveedor, long tamanio) =>
        new(true, url, ruta, proveedor, tamanio);

    /// <summary>Crea un resultado fallido con la descripción del error.</summary>
    public static StorageResult Fallo(string error) => new(false, Error: error);
}

/// <summary>
/// Contrato para el almacenamiento de archivos.
/// <para>
/// El proveedor activo (Local, S3, Firebase) se configura en
/// <c>appsettings.json → StorageSettings:Provider</c> y se resuelve
/// automáticamente en el DI. Cambiar de proveedor no requiere modificar
/// ningún módulo consumidor.
/// </para>
/// <para>
/// Los módulos de negocio NO deben inyectar esta interfaz directamente.
/// Deben usar los commands del módulo <c>Archivos</c> (SubirArchivoCommand)
/// para que los metadatos queden registrados en base de datos.
/// </para>
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Sube un archivo al proveedor configurado.
    /// Valida tipo MIME y tamaño antes de subir.
    /// Genera nombre único (GUID + extensión) para evitar colisiones.
    /// </summary>
    Task<StorageResult> SubirAsync(SubirArchivoRequest request, CancellationToken ct = default);

    /// <summary>
    /// Obtiene la URL de acceso para un archivo dado su ruta interna.
    /// Para S3 genera una URL firmada con expiración configurable.
    /// Para Local retorna la URL estática del servidor.
    /// </summary>
    Task<string?> ObtenerUrlAsync(string rutaAlmacenada, CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo del proveedor. Operación idempotente —
    /// no lanza error si el archivo no existe.
    /// </summary>
    Task EliminarAsync(string rutaAlmacenada, CancellationToken ct = default);
}
