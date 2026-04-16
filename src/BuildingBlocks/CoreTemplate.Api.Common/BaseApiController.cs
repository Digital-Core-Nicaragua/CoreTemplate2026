using CoreTemplate.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace CoreTemplate.Api.Common;

/// <summary>
/// Controller base para todos los controllers de la API.
/// <para>
/// Provee métodos helper que encapsulan la creación de respuestas HTTP
/// con el formato estándar <see cref="ApiResponse{T}"/>, evitando
/// código repetitivo en cada controller.
/// </para>
/// <para>
/// Todos los controllers del sistema deben heredar de esta clase:
/// <code>
/// public sealed class AuthController(IMediator _mediator) : BaseApiController
/// {
///     [HttpPost("login")]
///     public async Task&lt;IActionResult&gt; Login([FromBody] LoginRequest request, CancellationToken ct)
///     {
///         var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), ct);
///         if (!result.IsSuccess) return UnauthorizedResponse&lt;LoginResponseDto&gt;(result.Errors);
///         return SuccessResponse(result.Value!, result.Message);
///     }
/// }
/// </code>
/// </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Retorna 200 OK con los datos y mensaje indicados.
    /// </summary>
    protected OkObjectResult SuccessResponse<T>(T data, string message) =>
        Ok(ApiResponse<T>.FromResult(Result<T>.Success(data, message)));

    /// <summary>
    /// Retorna 200 OK con un resultado paginado.
    /// </summary>
    protected OkObjectResult SuccessPagedResponse<T>(PagedResult<T> data, string message) =>
        Ok(ApiResponse<PagedResult<T>>.FromResult(Result<PagedResult<T>>.Success(data, message)));

    /// <summary>
    /// Retorna 201 Created con la URL del recurso creado, usando nombre de ruta.
    /// </summary>
    protected CreatedAtRouteResult CreatedResponse<T>(string routeName, object routeValues, T data, string message) =>
        CreatedAtRoute(routeName, routeValues, ApiResponse<T>.FromResult(Result<T>.Success(data, message)));

    /// <summary>
    /// Retorna 400 Bad Request con los errores de validación del ModelState.
    /// </summary>
    protected BadRequestObjectResult ValidationProblem<T>()
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();

        return BadRequest(ApiResponse<T>.FromResult(Result<T>.Failure(errors)));
    }

    /// <summary>
    /// Retorna 400 Bad Request con los errores indicados.
    /// </summary>
    protected BadRequestObjectResult BadRequestResponse<T>(params string[] errors) =>
        BadRequest(ApiResponse<T>.FromResult(Result<T>.Failure(errors)));

    /// <summary>
    /// Retorna 401 Unauthorized con los errores indicados.
    /// </summary>
    protected UnauthorizedObjectResult UnauthorizedResponse<T>(params string[] errors) =>
        Unauthorized(ApiResponse<T>.FromResult(Result<T>.Failure(errors)));

    /// <summary>
    /// Retorna 403 Forbidden con los errores indicados.
    /// </summary>
    protected ObjectResult ForbiddenResponse<T>(params string[] errors) =>
        StatusCode(403, ApiResponse<T>.FromResult(Result<T>.Failure(errors)));

    /// <summary>
    /// Retorna 404 Not Found con los errores indicados.
    /// </summary>
    protected NotFoundObjectResult NotFoundResponse<T>(params string[] errors) =>
        NotFound(ApiResponse<T>.FromResult(Result<T>.Failure(errors)));

    /// <summary>
    /// Retorna 409 Conflict con los errores indicados.
    /// </summary>
    protected ConflictObjectResult ConflictResponse<T>(params string[] errors) =>
        Conflict(ApiResponse<T>.FromResult(
            Result<T>.Failure(errors.Length > 0 ? errors : ["Conflicto en la operación solicitada."])));

    /// <summary>
    /// Retorna 422 Unprocessable Entity con los errores indicados.
    /// Útil para errores de reglas de negocio que no son conflictos ni validaciones.
    /// </summary>
    protected ObjectResult UnprocessableResponse<T>(params string[] errors) =>
        UnprocessableEntity(ApiResponse<T>.FromResult(Result<T>.Failure(errors)));
}
