using CoreTemplate.Modules.Catalogos.Domain.Events;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Catalogos.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa un ítem de catálogo genérico.
/// <para>
/// Este aggregate sirve como patrón de referencia para crear nuevos catálogos
/// en sistemas que usen CoreTemplate. Para agregar un nuevo catálogo, copiar
/// este aggregate y renombrar según el dominio específico.
/// </para>
/// </summary>
public sealed class CatalogoItem : AggregateRoot<Guid>
{
    /// <summary>ID del tenant al que pertenece. Null si single-tenant.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Código único del ítem (por tenant).</summary>
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Nombre descriptivo del ítem.</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Descripción opcional del ítem.</summary>
    public string? Descripcion { get; private set; }

    /// <summary>Indica si el ítem está activo y disponible para uso.</summary>
    public bool EsActivo { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    /// <summary>Fecha de última modificación.</summary>
    public DateTime? ModificadoEn { get; private set; }

    private CatalogoItem() { }

    /// <summary>
    /// Crea un nuevo ítem de catálogo activo.
    /// </summary>
    /// <param name="codigo">Código único del ítem.</param>
    /// <param name="nombre">Nombre descriptivo.</param>
    /// <param name="descripcion">Descripción opcional.</param>
    /// <param name="tenantId">ID del tenant. Null si single-tenant.</param>
    public static Result<CatalogoItem> Crear(string codigo, string nombre, string? descripcion = null, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Result<CatalogoItem>.Failure("El código del ítem es requerido.");
        }

        if (codigo.Trim().Length > 50)
        {
            return Result<CatalogoItem>.Failure("El código no puede superar los 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result<CatalogoItem>.Failure("El nombre del ítem es requerido.");
        }

        if (nombre.Trim().Length > 200)
        {
            return Result<CatalogoItem>.Failure("El nombre no puede superar los 200 caracteres.");
        }

        var item = new CatalogoItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo.Trim().ToUpperInvariant(),
            Nombre = nombre.Trim(),
            Descripcion = descripcion?.Trim(),
            EsActivo = true,
            CreadoEn = DateTime.UtcNow
        };

        item.RaiseDomainEvent(new CatalogoItemCreadoEvent(item.Id, item.Codigo, item.Nombre));
        return Result<CatalogoItem>.Success(item);
    }

    /// <summary>Activa el ítem para que esté disponible.</summary>
    public Result Activar()
    {
        if (EsActivo)
        {
            return Result.Failure("El ítem ya está activo.");
        }

        EsActivo = true;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new CatalogoItemActivadoEvent(Id, Codigo));
        return Result.Success();
    }

    /// <summary>Desactiva el ítem para que no esté disponible.</summary>
    public Result Desactivar()
    {
        if (!EsActivo)
        {
            return Result.Failure("El ítem ya está inactivo.");
        }

        EsActivo = false;
        ModificadoEn = DateTime.UtcNow;
        RaiseDomainEvent(new CatalogoItemDesactivadoEvent(Id, Codigo));
        return Result.Success();
    }
}
