using CoreTemplate.Storage.Abstractions;
using CoreTemplate.Storage.Settings;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Storage.Validation;

internal sealed class ArchivoValidator(IOptions<StorageSettings> options)
{
    private readonly StorageSettings _settings = options.Value;

    public StorageResult? Validar(SubirArchivoRequest request)
    {
        if (!_settings.TiposPermitidos.Contains(request.ContentType.ToLowerInvariant()))
            return StorageResult.Fallo($"Tipo de archivo '{request.ContentType}' no permitido.");

        var extension = Path.GetExtension(request.NombreOriginal).ToLowerInvariant();
        if (!EsExtensionConsistente(request.ContentType, extension))
            return StorageResult.Fallo("La extensión del archivo no coincide con el tipo de contenido.");

        if (request.Contenido.CanSeek)
        {
            var tamanioBytes = request.Contenido.Length;
            var maxBytes = (long)_settings.MaxTamanioMB * 1024 * 1024;
            if (tamanioBytes > maxBytes)
                return StorageResult.Fallo($"El archivo supera el tamaño máximo de {_settings.MaxTamanioMB} MB.");
        }

        return null;
    }

    private static bool EsExtensionConsistente(string contentType, string extension) =>
        contentType switch
        {
            "application/pdf" => extension == ".pdf",
            "image/jpeg" => extension is ".jpg" or ".jpeg",
            "image/png" => extension == ".png",
            "image/webp" => extension == ".webp",
            "application/msword" => extension == ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => extension == ".docx",
            _ => true
        };
}
