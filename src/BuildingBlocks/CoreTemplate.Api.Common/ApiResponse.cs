using CoreTemplate.SharedKernel;

namespace CoreTemplate.Api.Common;

/// <summary>
/// Formato estándar de respuesta JSON para todos los endpoints de la API.
/// <para>
/// Garantiza que todas las respuestas — exitosas o fallidas — tengan
/// la misma estructura, facilitando el consumo desde cualquier cliente.
/// </para>
/// <para>
/// Estructura de respuesta exitosa:
/// <code>
/// {
///   "success": true,
///   "message": "Usuario creado correctamente.",
///   "data": { "id": "..." },
///   "errors": []
/// }
/// </code>
/// </para>
/// <para>
/// Estructura de respuesta fallida:
/// <code>
/// {
///   "success": false,
///   "message": "El email ya está registrado.",
///   "data": null,
///   "errors": ["El email ya está registrado."]
/// }
/// </code>
/// </para>
/// </summary>
/// <typeparam name="T">Tipo del valor retornado en caso de éxito.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>Indica si la operación fue exitosa.</summary>
    public bool Success { get; init; }

    /// <summary>Mensaje descriptivo de la operación.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Datos retornados. Null cuando la operación falla.</summary>
    public T? Data { get; init; }

    /// <summary>Lista de errores. Vacía cuando la operación es exitosa.</summary>
    public string[] Errors { get; init; } = [];

    /// <summary>
    /// Crea un <see cref="ApiResponse{T}"/> a partir de un <see cref="Result{T}"/>.
    /// Mapea automáticamente éxito/fallo, mensaje y errores.
    /// </summary>
    /// <param name="result">Resultado de la operación de negocio.</param>
    public static ApiResponse<T> FromResult(Result<T> result) => new()
    {
        Success = result.IsSuccess,
        Message = result.Message,
        Data = result.IsSuccess ? result.Value : default,
        Errors = result.IsSuccess ? [] : result.Errors
    };
}
