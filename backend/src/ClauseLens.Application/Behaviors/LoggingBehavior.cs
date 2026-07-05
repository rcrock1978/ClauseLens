using MediatR;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs the start/end of every command and query with
/// a correlation ID. Pairs with Serilog (Constitution Principle IV) and the
/// OpenTelemetry ActivitySource registered in Infrastructure/Telemetry.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Handling {Request}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await next();
            sw.Stop();
            _logger.LogInformation("Handled {Request} in {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Failed {Request} after {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
