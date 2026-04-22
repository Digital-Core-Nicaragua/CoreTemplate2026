using CoreTemplate.Modules.Auth.Domain.Entities;
using CoreTemplate.Modules.Auth.Domain.Events;
using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa un rol del sistema.
/// <para>
/// Un rol agrupa permisos y se asigna a usuarios.
/// Los roles de sistema (<see cref="EsSistema"/> = true) no pueden eliminarse.
/// </para>
/// </summary>
public sealed class Rol : AggregateRoot<Guid>, IAuditable
{
    /// <summary>ID del tenant al que pertenece. Null si es un rol global del sistema.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Nombre único del rol (por tenant).</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Descripción del rol.</summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si es un rol del sistema (SuperAdmin, Admin, User).
    /// Los roles de sistema no pueden eliminarse.
    /// </summary>
    public bool EsSistema { get; private set; }

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    private readonly List<RolPermiso> _permisos = [];

    /// <summary>Permisos asignados al rol.</summary>
    public IReadOnlyList<RolPermiso> Permisos => _permisos;

    private Rol() { }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    /// <param name="nombre">Nombre único del rol.</param>
    /// <param name="descripcion">Descripción del rol.</param>
    /// <param name="esSistema">True si es un rol del sistema (no eliminable).</param>
    /// <param name="tenantId">ID del tenant. Null si es rol global.</param>
    public static Result<Rol> Crear(string nombre, string descripcion, bool esSistema, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result<Rol>.Failure("El nombre del rol es requerido.");
        }

        if (nombre.Trim().Length > 100)
        {
            return Result<Rol>.Failure("El nombre del rol no puede superar los 100 caracteres.");
        }

        var rol = new Rol
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Nombre = nombre.Trim(),
            Descripcion = descripcion?.Trim() ?? string.Empty,
            EsSistema = esSistema,
            CreadoEn = DateTime.UtcNow
        };

        rol.RaiseDomainEvent(new RolCreadoEvent(rol.Id, rol.Nombre, tenantId));
        return Result<Rol>.Success(rol);
    }

    /// <summary>Actualiza el nombre y descripción del rol.</summary>
    public Result Actualizar(string nombre, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result.Failure("El nombre del rol es requerido.");
        }

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        RaiseDomainEvent(new RolActualizadoEvent(Id, Nombre));
        return Result.Success();
    }

    /// <summary>Agrega un permiso al rol si no lo tiene ya.</summary>
    public Result AgregarPermiso(Guid permisoId)
    {
        if (_permisos.Any(p => p.PermisoId == permisoId))
        {
            return Result.Failure("El rol ya tiene asignado este permiso.");
        }

        _permisos.Add(RolPermiso.Crear(Id, permisoId));
        RaiseDomainEvent(new PermisoAgregadoARolEvent(Id, permisoId));
        return Result.Success();
    }

    /// <summary>Quita un permiso del rol.</summary>
    public Result QuitarPermiso(Guid permisoId)
    {
        var permiso = _permisos.FirstOrDefault(p => p.PermisoId == permisoId);
        if (permiso is null)
        {
            return Result.Failure("El rol no tiene asignado este permiso.");
        }

        _permisos.Remove(permiso);
        RaiseDomainEvent(new PermisoQuitadoDeRolEvent(Id, permisoId));
        return Result.Success();
    }

    /// <summary>
    /// Indica si el rol puede ser eliminado.
    /// Los roles de sistema nunca pueden eliminarse.
    /// </summary>
    public bool PuedeEliminarse() => !EsSistema;
}
