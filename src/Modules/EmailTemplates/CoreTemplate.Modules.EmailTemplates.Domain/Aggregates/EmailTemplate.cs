using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Modules.EmailTemplates.Domain.Events;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;
using System.Text.Json;

namespace CoreTemplate.Modules.EmailTemplates.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa una plantilla de correo electrónico editable.
/// <para>
/// Las plantillas permiten personalizar el contenido y diseño de los correos
/// del sistema sin necesidad de redeployar la aplicación. El administrador
/// puede editarlas desde la UI en <c>/api/email-templates</c>.
/// </para>
/// <para>
/// Implementa <see cref="IHasTenant"/> para soporte multi-tenant:
/// cada empresa puede tener sus propias plantillas con su logo, colores
/// y contenido personalizado. La jerarquía de resolución es:
/// <list type="number">
/// <item>Plantilla del tenant actual (personalizada por empresa)</item>
/// <item>Plantilla global del sistema (TenantId = null)</item>
/// <item>Archivo .html del proyecto (fallback final)</item>
/// </list>
/// </para>
/// <para>
/// Las plantillas del sistema (<c>EsDeSistema = true</c>) no pueden eliminarse
/// pero sí editarse. Se crean automáticamente al arrancar via <c>EmailTemplatesDataSeeder</c>.
/// </para>
/// </summary>
public sealed class EmailTemplate : AggregateRoot<Guid>, IHasTenant
{
    /// <summary>
    /// ID del tenant propietario. Null = plantilla global del sistema visible para todos los tenants.
    /// Cuando un tenant tiene su propia versión, esta tiene prioridad sobre la global.
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Código único e inmutable de la plantilla. Formato: "modulo.nombre-accion".
    /// Ej: "auth.reset-password", "nomina.comprobante-pago"
    /// Los módulos consumidores referencian la plantilla por este código.
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Nombre descriptivo para mostrar en la UI. Ej: "Restablecimiento de contraseña"</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Módulo al que pertenece la plantilla. Ej: "Auth", "RRHH", "Sistema"</summary>
    public string Modulo { get; private set; } = string.Empty;

    /// <summary>Asunto del correo. Puede contener variables. Ej: "Restablece tu contraseña en {{SistemaNombre}}"</summary>
    public string Asunto { get; private set; } = string.Empty;

    /// <summary>
    /// Cuerpo HTML del correo. Puede contener variables con sintaxis {{NombreVariable}}.
    /// Si <c>UsarLayout = true</c>, este contenido se inserta en {{Contenido}} del layout.
    /// </summary>
    public string CuerpoHtml { get; private set; } = string.Empty;

    /// <summary>Lista de variables disponibles serializada como JSON. Usar <see cref="VariablesDisponibles"/> para acceder.</summary>
    public string VariablesDisponiblesJson { get; private set; } = "[]";

    /// <summary>
    /// Si true, el cuerpo se envuelve en la plantilla <c>sistema.layout</c> antes de enviar.
    /// El layout aporta header, footer y estilos corporativos.
    /// </summary>
    public bool UsarLayout { get; private set; } = true;

    /// <summary>
    /// Indica que es una plantilla del sistema. No puede eliminarse pero sí editarse.
    /// Se crea automáticamente por el seeder al arrancar la aplicación.
    /// </summary>
    public bool EsDeSistema { get; private set; }

    /// <summary>Indica si la plantilla está activa. Las plantillas inactivas no se usan en envíos.</summary>
    public bool EsActivo { get; private set; }

    /// <summary>Fecha y hora UTC de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    /// <summary>Fecha y hora UTC de la última modificación.</summary>
    public DateTime? ModificadoEn { get; private set; }

    /// <summary>ID del usuario que realizó la última modificación.</summary>
    public Guid? ModificadoPor { get; private set; }

    /// <summary>Lista de variables disponibles para esta plantilla. Ej: ["NombreUsuario", "LinkReset", "ExpiraEn"]</summary>
    public IReadOnlyList<string> VariablesDisponibles =>
        JsonSerializer.Deserialize<List<string>>(VariablesDisponiblesJson) ?? [];

    private EmailTemplate() { }

    /// <summary>
    /// Crea una nueva plantilla de correo electrónico.
    /// </summary>
    /// <param name="codigo">Código único e inmutable. Formato: "modulo.nombre". Ej: "auth.reset-password"</param>
    /// <param name="nombre">Nombre descriptivo para la UI.</param>
    /// <param name="modulo">Módulo propietario. Ej: "Auth", "RRHH"</param>
    /// <param name="asunto">Asunto del correo. Puede contener variables {{Variable}}.</param>
    /// <param name="cuerpoHtml">Cuerpo HTML. Puede contener variables {{Variable}}.</param>
    /// <param name="variables">Lista de variables disponibles para documentar al editor.</param>
    /// <param name="usarLayout">Si true, envuelve el cuerpo en sistema.layout.</param>
    /// <param name="esDeSistema">Si true, no puede eliminarse.</param>
    /// <param name="tenantId">Null = plantilla global. Con valor = plantilla exclusiva del tenant.</param>
    public static Result<EmailTemplate> Crear(
        string codigo,
        string nombre,
        string modulo,
        string asunto,
        string cuerpoHtml,
        IEnumerable<string>? variables = null,
        bool usarLayout = true,
        bool esDeSistema = false,
        Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return Result<EmailTemplate>.Failure("El código de la plantilla es requerido.");

        if (string.IsNullOrWhiteSpace(nombre))
            return Result<EmailTemplate>.Failure("El nombre de la plantilla es requerido.");

        if (string.IsNullOrWhiteSpace(asunto))
            return Result<EmailTemplate>.Failure("El asunto de la plantilla es requerido.");

        if (string.IsNullOrWhiteSpace(cuerpoHtml))
            return Result<EmailTemplate>.Failure("El cuerpo HTML de la plantilla es requerido.");

        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo.Trim().ToLowerInvariant(),
            Nombre = nombre.Trim(),
            Modulo = modulo.Trim(),
            Asunto = asunto.Trim(),
            CuerpoHtml = cuerpoHtml,
            VariablesDisponiblesJson = JsonSerializer.Serialize(variables?.ToList() ?? []),
            UsarLayout = usarLayout,
            EsDeSistema = esDeSistema,
            EsActivo = true,
            CreadoEn = DateTime.UtcNow
        };

        template.RaiseDomainEvent(new PlantillaCreada(template.Id, template.Codigo, template.Modulo));
        return Result<EmailTemplate>.Success(template);
    }

    /// <summary>
    /// Actualiza el asunto, cuerpo HTML y variables de la plantilla.
    /// El código es inmutable y no puede modificarse.
    /// </summary>
    public Result Actualizar(string asunto, string cuerpoHtml, IEnumerable<string>? variables, Guid modificadoPor)
    {
        if (string.IsNullOrWhiteSpace(asunto))
            return Result.Failure("El asunto es requerido.");

        if (string.IsNullOrWhiteSpace(cuerpoHtml))
            return Result.Failure("El cuerpo HTML es requerido.");

        Asunto = asunto.Trim();
        CuerpoHtml = cuerpoHtml;
        VariablesDisponiblesJson = JsonSerializer.Serialize(variables?.ToList() ?? []);
        ModificadoEn = DateTime.UtcNow;
        ModificadoPor = modificadoPor;

        RaiseDomainEvent(new PlantillaActualizada(Id, Codigo, modificadoPor));
        return Result.Success();
    }

    /// <summary>Activa la plantilla para que sea usada en envíos.</summary>
    public Result Activar()
    {
        if (EsActivo) return Result.Failure("La plantilla ya está activa.");
        EsActivo = true;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new PlantillaActivada(Id, Codigo));
        return Result.Success();
    }

    /// <summary>Desactiva la plantilla. Los envíos que la referencien usarán el fallback de archivo.</summary>
    public Result Desactivar()
    {
        if (!EsActivo) return Result.Failure("La plantilla ya está inactiva.");
        EsActivo = false;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new PlantillaDesactivada(Id, Codigo));
        return Result.Success();
    }
}
