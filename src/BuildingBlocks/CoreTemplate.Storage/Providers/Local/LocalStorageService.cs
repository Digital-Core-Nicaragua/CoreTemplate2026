using CoreTemplate.Logging.Abstractions;
using CoreTemplate.Storage.Abstractions;
using CoreTemplate.Storage.Settings;
using CoreTemplate.Storage.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Storage.Providers.Local;

/// <summary>
/// Almacena archivos en el sistema de archivos del servidor.
/// La URL de acceso se construye dinámicamente usando el host real del request
/// (igual que Request.Scheme + Request.Host en un controller), por lo que
/// funciona en cualquier puerto sin configuración adicional.
/// Requiere que Program.cs registre UseStaticFiles apuntando a la carpeta base.
/// </summary>
internal sealed class LocalStorageService(
    IOptions<LocalStorageSettings> options,
    ArchivoValidator validator,
    IHttpContextAccessor httpContextAccessor,
    IAppLogger logger) : IStorageService
{
    private readonly LocalStorageSettings _settings = options.Value;
    private readonly IAppLogger _logger = logger.ForContext<LocalStorageService>();

    public async Task<StorageResult> SubirAsync(SubirArchivoRequest request, CancellationToken ct = default)
    {
        var error = validator.Validar(request);
        if (error is not null) return error;

        try
        {
            var extension = Path.GetExtension(request.NombreOriginal);
            var nombreAlmacenado = $"{Guid.NewGuid()}{extension}";
            var rutaRelativa = Path.Combine(
                request.Contexto.Replace('/', Path.DirectorySeparatorChar),
                nombreAlmacenado);
            var rutaCompleta = Path.Combine(_settings.BasePath, rutaRelativa);

            Directory.CreateDirectory(Path.GetDirectoryName(rutaCompleta)!);

            long tamanio;
            await using (var fs = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write))
            {
                await request.Contenido.CopyToAsync(fs, ct);
                tamanio = fs.Length;
            }

            var rutaNormalizada = rutaRelativa.Replace(Path.DirectorySeparatorChar, '/');
            var url = ConstruirUrl(rutaNormalizada);

            _logger.Info("Archivo subido localmente: {Ruta}, {Tamanio} bytes", rutaNormalizada, tamanio);
            return StorageResult.Ok(url, rutaNormalizada, "Local", tamanio);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al subir archivo localmente: {Nombre}", request.NombreOriginal);
            return StorageResult.Fallo(ex.Message);
        }
    }

    public Task<string?> ObtenerUrlAsync(string rutaAlmacenada, CancellationToken ct = default)
    {
        var url = ConstruirUrl(rutaAlmacenada);
        return Task.FromResult<string?>(url);
    }

    public Task EliminarAsync(string rutaAlmacenada, CancellationToken ct = default)
    {
        var rutaCompleta = Path.Combine(
            _settings.BasePath,
            rutaAlmacenada.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(rutaCompleta))
        {
            File.Delete(rutaCompleta);
            _logger.Info("Archivo eliminado localmente: {Ruta}", rutaAlmacenada);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Construye la URL usando el host real del request actual (scheme + host + port).
    /// Si no hay contexto HTTP (ej: background job), usa BaseUrl de configuración como fallback.
    /// </summary>
    private string ConstruirUrl(string rutaNormalizada)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is not null)
        {
            // Igual que: $"{Request.Scheme}://{Request.Host}/archivos/{ruta}"
            return $"{ctx.Request.Scheme}://{ctx.Request.Host}/{_settings.RequestPath.Trim('/')}/{rutaNormalizada}";
        }

        // Fallback para cuando no hay contexto HTTP
        return $"{_settings.BaseUrl.TrimEnd('/')}/{rutaNormalizada}";
    }
}
