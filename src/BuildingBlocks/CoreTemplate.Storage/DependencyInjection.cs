using CoreTemplate.Storage.Abstractions;
using CoreTemplate.Storage.Providers.Firebase;
using CoreTemplate.Storage.Providers.Local;
using CoreTemplate.Storage.Providers.S3;
using CoreTemplate.Storage.Settings;
using CoreTemplate.Storage.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreTemplate.Storage;

public static class DependencyInjection
{
    public static IServiceCollection AddStorageService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));
        services.AddScoped<ArchivoValidator>();

        var provider = configuration[$"{StorageSettings.SectionName}:Provider"] ?? "Local";

        switch (provider.ToLowerInvariant())
        {
            case "local":
                services.Configure<LocalStorageSettings>(configuration.GetSection(LocalStorageSettings.SectionName));
                services.AddScoped<IStorageService, LocalStorageService>();
                break;

            case "s3":
                services.Configure<S3Settings>(configuration.GetSection(S3Settings.SectionName));
                services.AddScoped<IStorageService, S3StorageService>();
                break;

            case "firebase":
                services.Configure<FirebaseSettings>(configuration.GetSection(FirebaseSettings.SectionName));
                services.AddScoped<IStorageService, FirebaseStorageService>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Proveedor de storage '{provider}' no reconocido. Valores válidos: Local, S3, Firebase.");
        }

        return services;
    }
}
