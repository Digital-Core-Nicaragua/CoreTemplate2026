namespace CoreTemplate.Infrastructure.Services;

/// <summary>
/// Contrato para obtener información del usuario autenticado en la solicitud actual.
/// <para>
/// Se resuelve desde los claims del JWT en cada request HTTP.
/// Disponible en cualquier capa que lo inyecte — Application, Infrastructure, Api.
/// </para>
/// <para>
/// Uso típico en un handler:
/// <code>
/// internal sealed class GetMiPerfilQueryHandler(
///     IUsuarioRepository _repo,
///     ICurrentUser _currentUser) : IRequestHandler&lt;GetMiPerfilQuery, Result&lt;UsuarioDto&gt;&gt;
/// {
///     public async Task&lt;Result&lt;UsuarioDto&gt;&gt; Handle(GetMiPerfilQuery query, CancellationToken ct)
///     {
///         var usuario = await _repo.GetByIdAsync(_currentUser.Id, ct);
///         // ...
///     }
/// }
/// </code>
/// </para>
/// </summary>
public interface ICurrentUser
{
    /// <summary>ID del usuario autenticado. Null si la solicitud es anónima.</summary>
    Guid? Id { get; }

    /// <summary>Email del usuario autenticado. Null si la solicitud es anónima.</summary>
    string? Email { get; }

    /// <summary>Nombre del usuario autenticado. Null si la solicitud es anónima.</summary>
    string? Nombre { get; }

    /// <summary>Roles del usuario autenticado.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Indica si el usuario está autenticado.</summary>
    bool EstaAutenticado { get; }

    /// <summary>Indica si el usuario tiene el rol indicado.</summary>
    bool TieneRol(string rol);

    /// <summary>Indica si el usuario tiene el permiso indicado.</summary>
    bool TienePermiso(string permiso);
}
