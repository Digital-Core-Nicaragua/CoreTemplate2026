using CoreTemplate.Infrastructure.Persistence;
using CoreTemplate.Modules.Configuracion.Domain.Enums;
using CoreTemplate.Modules.Configuracion.Domain.Events;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Configuracion.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa un parámetro de configuración del sistema.
/// Permite al administrador cambiar valores de negocio desde la UI sin redeployar.
/// Implementa IHasTenant para soporte multi-tenant: cada empresa puede tener
/// sus propios valores. Si no existe para el tenant → usa el valor global (TenantId = null).
/// </summary>
public sealed class ConfiguracionItem : AggregateRoot<Guid>, IHasTenant
{
    /// <summary>ID del tenant. null = parámetro global visible para todos los tenants.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Clave única e inmutable. Formato: "grupo.nombre". Ej: "sistema.nombre"</summary>
    public string Clave { get; private set; } = string.Empty;

    /// <summary>Valor actual como string. Se convierte según Tipo al leer.</summary>
    public string Valor { get; private set; } = string.Empty;

    /// <summary>Tipo del valor: String, Number, Boolean, Json.</summary>
    public TipoValor Tipo { get; private set; }

    /// <summary>Descripción para mostrar en la UI al administrador.</summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>Grupo de agrupación visual. Ej: "Sistema", "Facturacion", "Nomina"</summary>
    public string Grupo { get; private set; } = string.Empty;

    /// <summary>Si false, el administrador no puede modificarlo desde la UI.</summary>
    public bool EsEditable { get; private set; } = true;

    public DateTime CreadoEn { get; private set; }
    public DateTime? ModificadoEn { get; private set; }
    public Guid? ModificadoPor { get; private set; }

    private ConfiguracionItem() { }

    public static Result<ConfiguracionItem> Crear(
        string clave, string valor, TipoValor tipo,
        string descripcion, string grupo,
        bool esEditable = true, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return Result<ConfiguracionItem>.Failure("La clave es requerida.");

        if (!clave.Contains('.'))
            return Result<ConfiguracionItem>.Failure("La clave debe tener formato 'grupo.nombre'. Ej: 'sistema.nombre'.");

        var item = new ConfiguracionItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Clave = clave.Trim().ToLowerInvariant(),
            Valor = valor ?? string.Empty,
            Tipo = tipo,
            Descripcion = descripcion.Trim(),
            Grupo = grupo.Trim(),
            EsEditable = esEditable,
            CreadoEn = DateTime.UtcNow
        };

        item.RaiseDomainEvent(new ConfiguracionCreada(item.Id, item.Clave, item.Grupo));
        return Result<ConfiguracionItem>.Success(item);
    }

    public Result Actualizar(string valor, Guid modificadoPor)
    {
        if (!EsEditable)
            return Result.Failure("Este parámetro no es editable.");

        var valorAnterior = Valor;
        Valor = valor ?? string.Empty;
        ModificadoEn = DateTime.UtcNow;
        ModificadoPor = modificadoPor;

        RaiseDomainEvent(new ConfiguracionActualizada(Id, Clave, valorAnterior, Valor, modificadoPor));
        return Result.Success();
    }
}
