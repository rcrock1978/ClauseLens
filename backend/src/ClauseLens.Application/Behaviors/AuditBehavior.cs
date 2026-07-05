using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Common;
using MediatR;

namespace ClauseLens.Application.Behaviors;

/// <summary>
/// Captures every domain event raised by an aggregate during a command and
/// forwards them to the audit writer + outbox. Honors the append-only contract
/// from FR-008 / SC-005.
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _dispatcher;

    public AuditBehavior(ICurrentUser current, IAuditEventDispatcher dispatcher)
    { _current = current; _dispatcher = dispatcher; }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var result = await next();

        // Handlers are expected to raise domain events on their aggregates. After
        // the handler runs, we collect any events that haven't yet been
        // dispatched and forward them to the audit/outbox pipeline.
        await _dispatcher.DispatchPendingAsync(_current, ct);
        return result;
    }
}

public interface IAuditEventDispatcher
{
    Task DispatchPendingAsync(ICurrentUser current, CancellationToken ct = default);
}
