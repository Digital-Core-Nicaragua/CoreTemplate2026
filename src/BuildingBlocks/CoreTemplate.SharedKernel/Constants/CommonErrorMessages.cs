namespace CoreTemplate.SharedKernel.Constants;

/// <summary>
/// Mensajes de error comunes reutilizables en todos los módulos del sistema.
/// Cada módulo puede definir sus propios mensajes adicionales en su capa de Application.
/// </summary>
public static class CommonErrorMessages
{
    // ─── Genéricos ────────────────────────────────────────────────────────────
    public const string NoEncontrado = "El recurso solicitado no fue encontrado.";
    public const string ErrorInterno = "Ocurrió un error interno. Intente nuevamente.";
    public const string SolicitudInvalida = "La solicitud contiene datos inválidos.";
    public const string OperacionNoPermitida = "No tiene permisos para realizar esta operación.";
    public const string ConflictoEstado = "El recurso ya se encuentra en el estado solicitado.";

    // ─── Paginación ───────────────────────────────────────────────────────────
    public const string PaginaInvalida = "El número de página debe ser mayor a 0.";
    public const string TamanoPaginaInvalido = "El tamaño de página debe estar entre 1 y 100.";

    // ─── Identificadores ──────────────────────────────────────────────────────
    public const string IdRequerido = "El identificador es requerido.";
    public const string IdInvalido = "El identificador proporcionado no es válido.";
}
