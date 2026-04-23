namespace CoreTemplate.Storage.Settings;

public sealed class StorageSettings
{
    public const string SectionName = "StorageSettings";
    public string Provider { get; set; } = "Local";
    public int MaxTamanioMB { get; set; } = 20;
    public List<string> TiposPermitidos { get; set; } =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];
}

public sealed class LocalStorageSettings
{
    public const string SectionName = "LocalStorageSettings";
    /// <summary>Ruta física en disco donde se guardan los archivos.</summary>
    public string BasePath { get; set; } = "archivos";
    /// <summary>Ruta HTTP bajo la cual se sirven los archivos. Ej: "/archivos"</summary>
    public string RequestPath { get; set; } = "/archivos";
    /// <summary>URL base fallback cuando no hay contexto HTTP (background jobs).</summary>
    public string BaseUrl { get; set; } = string.Empty;
}

public sealed class S3Settings
{
    public const string SectionName = "S3Settings";
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int UrlExpirationSeconds { get; set; } = 3600;
}

public sealed class FirebaseSettings
{
    public const string SectionName = "FirebaseSettings";
    public string ProjectId { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public string ServiceAccountKeyPath { get; set; } = string.Empty;
}
