using CoreTemplate.SharedKernel;
using FluentValidation;
using MediatR;

namespace CoreTemplate.Api.Common.Behaviors;

/// <summary>
/// Pipeline behavior de MediatR que ejecuta automáticamente las validaciones
/// de FluentValidation antes de que el request llegue al handler.
/// <para>
/// Si existen validadores registrados para el tipo de request y alguna
/// validación falla, el handler nunca se ejecuta — se retorna directamente
/// un <see cref="Result{T}"/> con los errores de validación.
/// </para>
/// <para>
/// Se registra una sola vez en el DI del módulo y aplica automáticamente
/// a todos los commands y queries que tengan un validator asociado:
/// <code>
/// services.AddMediatR(cfg =>
/// {
///     cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
///     cfg.AddBehavior(typeof(IPipelineBehavior&lt;,&gt;), typeof(ValidationBehavior&lt;,&gt;));
/// });
/// </code>
/// </para>
/// </summary>
/// <typeparam name="TRequest">Tipo del request (Command o Query).</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta, debe ser <see cref="Result{T}"/>.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> _validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si no hay validators registrados para este request, continuar al handler
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        // Ejecutar todos los validators en paralelo
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Recopilar todos los errores de todas las validaciones
        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            return await next(cancellationToken);
        }

        // Construir el Result<T> de fallo con los errores de validación
        // usando reflexión para crear la instancia del tipo de respuesta correcto
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(Result<object>.Failure), [typeof(string[])]);

            var result = failureMethod!.Invoke(null, [errors]);
            return (TResponse)result!;
        }

        // Si el tipo de respuesta no es Result<T>, lanzar excepción de validación
        throw new ValidationException(
            string.Join(" | ", errors));
    }
}
