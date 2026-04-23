using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CoreTemplate.Logging.Abstractions;
using CoreTemplate.Storage.Abstractions;
using CoreTemplate.Storage.Settings;
using CoreTemplate.Storage.Validation;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Storage.Providers.S3;

internal sealed class S3StorageService(
    IOptions<S3Settings> options,
    ArchivoValidator validator,
    IAppLogger logger) : IStorageService
{
    private readonly S3Settings _settings = options.Value;
    private readonly IAppLogger _logger = logger.ForContext<S3StorageService>();

    public async Task<StorageResult> SubirAsync(SubirArchivoRequest request, CancellationToken ct = default)
    {
        var error = validator.Validar(request);
        if (error is not null) return error;

        try
        {
            var extension = Path.GetExtension(request.NombreOriginal);
            var nombreAlmacenado = $"{Guid.NewGuid()}{extension}";
            var key = $"{request.Contexto.Trim('/')}/{nombreAlmacenado}";

            using var client = CrearCliente();

            var putRequest = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = request.Contenido,
                ContentType = request.ContentType,
                AutoCloseStream = false
            };

            var response = await client.PutObjectAsync(putRequest, ct);
            var tamanio = request.Contenido.CanSeek ? request.Contenido.Length : 0;
            var url = await ObtenerUrlAsync(key, ct);

            _logger.Info("Archivo subido a S3: {Key}, {Tamanio} bytes", key, tamanio);
            return StorageResult.Ok(url!, key, "S3", tamanio);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al subir archivo a S3: {Nombre}", request.NombreOriginal);
            return StorageResult.Fallo(ex.Message);
        }
    }

    public Task<string?> ObtenerUrlAsync(string rutaAlmacenada, CancellationToken ct = default)
    {
        using var client = CrearCliente();
        var urlRequest = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = rutaAlmacenada,
            Expires = DateTime.UtcNow.AddSeconds(_settings.UrlExpirationSeconds)
        };
        var url = client.GetPreSignedURL(urlRequest);
        return Task.FromResult<string?>(url);
    }

    public async Task EliminarAsync(string rutaAlmacenada, CancellationToken ct = default)
    {
        try
        {
            using var client = CrearCliente();
            await client.DeleteObjectAsync(_settings.BucketName, rutaAlmacenada, ct);
            _logger.Info("Archivo eliminado de S3: {Key}", rutaAlmacenada);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al eliminar archivo de S3: {Key}", rutaAlmacenada);
        }
    }

    private AmazonS3Client CrearCliente()
    {
        var region = RegionEndpoint.GetBySystemName(_settings.Region);

        if (!string.IsNullOrWhiteSpace(_settings.AccessKey))
            return new AmazonS3Client(
                new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey), region);

        // Sin credenciales explícitas → usa IAM Role / credenciales del entorno
        return new AmazonS3Client(region);
    }
}
