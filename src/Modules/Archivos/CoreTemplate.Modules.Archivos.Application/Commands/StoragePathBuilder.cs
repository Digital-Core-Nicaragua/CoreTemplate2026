namespace CoreTemplate.Modules.Archivos.Application.Commands;

internal static class StoragePathBuilder
{
    public static string Construir(string moduloOrigen, string contentType, DateTime fecha)
    {
        var modulo = Sanitizar(moduloOrigen);
        var mes = fecha.ToString("yyyy-MM");
        var tipo = ResolverTipo(contentType);
        return $"{modulo}/{mes}/{tipo}";
    }

    private static string ResolverTipo(string contentType) => contentType.ToLowerInvariant() switch
    {
        var ct when ct == "application/pdf" => "PDF",
        var ct when ct.StartsWith("image/") => "Imagenes",
        var ct when ct is "application/msword"
            or "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "Word",
        var ct when ct is "application/vnd.ms-excel"
            or "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "Excel",
        var ct when ct.StartsWith("video/") => "Videos",
        var ct when ct.StartsWith("text/") => "Texto",
        _ => "Otros"
    };

    private static string Sanitizar(string valor) =>
        string.IsNullOrWhiteSpace(valor) ? "General" : valor.Trim().Replace(" ", "_");
}
