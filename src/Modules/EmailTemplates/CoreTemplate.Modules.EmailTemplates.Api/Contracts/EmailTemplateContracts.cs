namespace CoreTemplate.Modules.EmailTemplates.Api.Contracts;

public record CrearPlantillaRequest(
    string Codigo,
    string Nombre,
    string Modulo,
    string Asunto,
    string CuerpoHtml,
    List<string>? Variables,
    bool UsarLayout = true);

public record ActualizarPlantillaRequest(
    string Asunto,
    string CuerpoHtml,
    List<string>? Variables);

public record PreviewPlantillaRequest(Dictionary<string, string> Variables);

public record EnviarPruebaRequest(
    string Destinatario,
    Dictionary<string, string> Variables);
