using FluentValidation;
using MediatR;

namespace ClauseLens.Application.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation validators before every command.
/// Validation failures short-circuit the pipeline and return a Result.Failure.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count == 0) return await next();

        var error = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        return (TResponse)FailureResultFactory.Create(typeof(TResponse), error, "VALIDATION_FAILED");
    }
}

internal static class FailureResultFactory
{
    public static object Create(Type responseType, string error, string code)
    {
        var resultType = responseType.IsGenericType
            ? responseType.GetGenericTypeDefinition()
            : responseType;
        var genericArg = responseType.IsGenericType ? responseType.GetGenericArguments()[0] : typeof(object);
        var closed = resultType.MakeGenericType(genericArg);
        var failure = closed.GetMethod("Failure", new[] { typeof(string), typeof(string) });
        return failure!.Invoke(null, new object[] { error, code })!;
    }
}
