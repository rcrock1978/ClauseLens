namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Result envelope for command/query handlers. Use static factories to construct.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? value, string? error, string? code)
    { IsSuccess = isSuccess; Value = value; Error = error; ErrorCode = code; }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string error, string code = "FAILURE") => new(false, default, error, code);
}

public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }
    private Result(bool ok, string? err, string? code) { IsSuccess = ok; Error = err; ErrorCode = code; }
    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string code = "FAILURE") => new(false, error, code);
}
