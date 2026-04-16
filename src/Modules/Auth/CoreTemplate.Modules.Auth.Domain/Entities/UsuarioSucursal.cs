using CoreTemplate.SharedKernel.Domain;

namespace CoreTemplate.Modules.Auth.Domain.Entities;

/// <summary>
/// Relación entre un usuario y una sucursal asignada.
/// Un usuario puede pertenecer a múltiples sucursales con una principal.
/// </summary>
public sealed class UsuarioSucursal : Entity<Guid>
{
    /// <summary>ID del usuario.</summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>ID de la sucursal.</summary>
    public Guid SucursalId { get; private set; }

    /// <summary>Indica si esta es la sucursal principal del usuario.</summary>
    public bool EsPrincipal { get; private set; }

    /// <summary>Fecha de asignación.</summary>
    public DateTime AsignadoEn { get; private set; }

    private UsuarioSucursal() { }

    internal static UsuarioSucursal Crear(Guid usuarioId, Guid sucursalId, bool esPrincipal = false) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        SucursalId = sucursalId,
        EsPrincipal = esPrincipal,
        AsignadoEn = DateTime.UtcNow
    };

    internal void MarcarComoPrincipal() => EsPrincipal = true;
    internal void QuitarPrincipal() => EsPrincipal = false;
}
