using CoreTemplate.SharedKernel;
using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Aggregates;

/// <summary>
/// Aggregate Root que representa un permiso del sistema.
/// <para>
/// Los permisos son el catálogo global de acciones permitidas en el sistema.
/// Se asignan a roles y se verifican en los endpoints con
/// <c>[RequirePermission("Modulo.Recurso.Accion")]</c>.
/// </para>
/// <para>
/// Formato del código: <c>Modulo.Recurso.Accion</c>
/// Ejemplos: <c>Usuarios.Roles.Crear</c>, <c>Catalogos.Items.Gestionar</c>
/// </para>
/// </summary>
public sealed class Permiso : AggregateRoot<Guid>
{
    /// <summary>
    /// Código único del permiso en formato <c>Modulo.Recurso.Accion</c>.
    /// </summary>
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Nombre legible del permiso.</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Descripción detallada de lo que permite hacer.</summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>Módulo al que pertenece el permiso.</summary>
    public string Modulo { get; private set; } = string.Empty;

    /// <summary>Fecha de creación.</summary>
    public DateTime CreadoEn { get; private set; }

    private Permiso() { }

    /// <summary>
    /// Crea un nuevo permiso.
    /// </summary>
    /// <param name="codigo">Código en formato <c>Modulo.Recurso.Accion</c>.</param>
    /// <param name="nombre">Nombre legible.</param>
    /// <param name="descripcion">Descripción de lo que permite.</param>
    /// <param name="modulo">Módulo al que pertenece.</param>
    public static Result<Permiso> Crear(string codigo, string nombre, string descripcion, string modulo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Result<Permiso>.Failure("El código del permiso es requerido.");
        }

        if (!codigo.Contains('.'))
        {
            return Result<Permiso>.Failure("El código debe tener formato 'Modulo.Recurso.Accion'.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Result<Permiso>.Failure("El nombre del permiso es requerido.");
        }

        if (string.IsNullOrWhiteSpace(modulo))
        {
            return Result<Permiso>.Failure("El módulo del permiso es requerido.");
        }

        var permiso = new Permiso
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            Descripcion = descripcion?.Trim() ?? string.Empty,
            Modulo = modulo.Trim(),
            CreadoEn = DateTime.UtcNow
        };

        return Result<Permiso>.Success(permiso);
    }
}
