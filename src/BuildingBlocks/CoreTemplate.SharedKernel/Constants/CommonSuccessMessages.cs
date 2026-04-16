namespace CoreTemplate.SharedKernel.Constants;

/// <summary>
/// Mensajes de éxito comunes reutilizables en todos los módulos del sistema.
/// Cada módulo puede definir sus propios mensajes adicionales en su capa de Application.
/// </summary>
public static class CommonSuccessMessages
{
    // ─── Operaciones CRUD ─────────────────────────────────────────────────────
    public const string CreadoExitosamente = "El recurso fue creado correctamente.";
    public const string ActualizadoExitosamente = "El recurso fue actualizado correctamente.";
    public const string EliminadoExitosamente = "El recurso fue eliminado correctamente.";
    public const string ConsultaExitosa = "Consulta realizada correctamente.";

    // ─── Cambios de estado ────────────────────────────────────────────────────
    public const string ActivadoExitosamente = "El recurso fue activado correctamente.";
    public const string DesactivadoExitosamente = "El recurso fue desactivado correctamente.";
}
