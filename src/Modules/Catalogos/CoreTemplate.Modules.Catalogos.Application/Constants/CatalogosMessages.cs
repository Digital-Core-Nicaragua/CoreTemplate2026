namespace CoreTemplate.Modules.Catalogos.Application.Constants;

internal static class CatalogosErrorMessages
{
    public const string ItemNoEncontrado = "El ítem de catálogo no fue encontrado.";
    public const string CodigoYaExiste = "Ya existe un ítem con ese código.";
    public const string ItemYaActivo = "El ítem ya está activo.";
    public const string ItemYaInactivo = "El ítem ya está inactivo.";
}

internal static class CatalogosSuccessMessages
{
    public const string ItemCreado = "Ítem de catálogo creado correctamente.";
    public const string ItemActivado = "Ítem activado correctamente.";
    public const string ItemDesactivado = "Ítem desactivado correctamente.";
}
