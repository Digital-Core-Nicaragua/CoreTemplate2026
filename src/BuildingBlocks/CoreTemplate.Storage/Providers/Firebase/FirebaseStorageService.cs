using CoreTemplate.Storage.Abstractions;
using CoreTemplate.Storage.Settings;
using Microsoft.Extensions.Options;

namespace CoreTemplate.Storage.Providers.Firebase;

/// <summary>
/// Implementación de IStorageService para Firebase Storage.
/// Instalar paquete: FirebaseAdmin (Google.Apis.Auth)
/// </summary>
internal sealed class FirebaseStorageService(IOptions<FirebaseSettings> options) : IStorageService
{
    private readonly FirebaseSettings _settings = options.Value;

    public Task<StorageResult> SubirAsync(SubirArchivoRequest request, CancellationToken ct = default)
    {
        // TODO: Implementar con Firebase Admin SDK
        // FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(_settings.ServiceAccountKeyPath) });
        // var storage = FirebaseStorage.DefaultInstance;
        throw new NotImplementedException("Firebase Storage no está implementado aún. Cambia Provider a 'Local' o 'S3'.");
    }

    public Task<string?> ObtenerUrlAsync(string rutaAlmacenada, CancellationToken ct = default) =>
        throw new NotImplementedException("Firebase Storage no está implementado aún.");

    public Task EliminarAsync(string rutaAlmacenada, CancellationToken ct = default) =>
        throw new NotImplementedException("Firebase Storage no está implementado aún.");
}
