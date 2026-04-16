namespace CoreTemplate.SharedKernel;

/// <summary>
/// Representa el resultado de una operación sin valor de retorno.
/// <para>
/// Permite modelar explícitamente si una operación fue exitosa o fallida,
/// evitando el uso de excepciones para el control de flujo en validaciones
/// o reglas de negocio.
/// </para>
/// <para>
/// Uso típico en un método de dominio:
/// <code>
/// public Result Activar()
/// {
///     if (Estado == EstadoUsuario.Activo)
///         return Result.Failure("El usuario ya está activo.");
///
///     Estado = EstadoUsuario.Activo;
///     return Result.Success();
/// }
/// </code>
/// </para>
/// </summary>
public class Result
{
    /// <summary>Indica si la operación fue exitosa.</summary>
    public bool IsSuccess { get; }

    /// <summary>Mensaje de error. Null cuando la operación es exitosa.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Crea un resultado exitoso.</summary>
    public static Result Success() => new(true, null);

    /// <summary>Crea un resultado fallido con el mensaje de error indicado.</summary>
    /// <param name="error">Mensaje que explica por qué falló la operación.</param>
    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// Representa el resultado de una operación que retorna un valor de tipo <typeparamref name="T"/>.
/// <para>
/// Permite modelar explícitamente si una operación fue exitosa o fallida,
/// evitando el uso de excepciones para el control de flujo en validaciones
/// o reglas de negocio.
/// </para>
/// <para>
/// Uso típico en un Handler de MediatR:
/// <code>
/// public async Task&lt;Result&lt;UsuarioDto&gt;&gt; Handle(GetUsuarioByIdQuery query, CancellationToken ct)
/// {
///     var usuario = await _repo.GetByIdAsync(query.Id, ct);
///     if (usuario is null)
///         return Result&lt;UsuarioDto&gt;.Failure("El usuario no fue encontrado.");
///
///     return Result&lt;UsuarioDto&gt;.Success(usuario.ToDto(), "Usuario obtenido correctamente.");
/// }
/// </code>
/// </para>
/// </summary>
/// <typeparam name="T">Tipo del valor retornado cuando la operación es exitosa.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Valor retornado cuando la operación fue exitosa.
    /// Será <c>null</c> cuando la operación falle.
    /// </summary>
    public T? Value { get; }

    /// <summary>Indica si la operación fue exitosa.</summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Mensaje descriptivo de la operación.
    /// Informativo en caso de éxito, explicativo en caso de error.
    /// </summary>
    public string Message { get; } = string.Empty;

    /// <summary>
    /// Lista de errores asociados a la operación.
    /// Solo contiene valores cuando <see cref="IsSuccess"/> es <c>false</c>.
    /// </summary>
    public string[] Errors { get; } = [];

    /// <summary>Primer error de la lista. Null cuando la operación es exitosa.</summary>
    public string? Error => Errors.FirstOrDefault();

    private Result(T? value, bool isSuccess, string message, string[] errors)
    {
        Value = value;
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    /// <summary>
    /// Crea un resultado exitoso con el valor y mensaje indicados.
    /// </summary>
    /// <param name="value">Valor retornado por la operación.</param>
    /// <param name="message">Mensaje descriptivo. Ejemplo: "Usuario creado correctamente."</param>
    public static Result<T> Success(T value, string message = "") =>
        new(value, true, message, []);

    /// <summary>
    /// Crea un resultado fallido con uno o más mensajes de error.
    /// </summary>
    /// <param name="errors">
    /// Mensajes que explican por qué falló la operación.
    /// Ejemplo: "El email ya está registrado.", "La contraseña es muy corta."
    /// </param>
    public static Result<T> Failure(params string[] errors) =>
        new(default, false, errors.FirstOrDefault() ?? "Ocurrió un error.", errors);
}
