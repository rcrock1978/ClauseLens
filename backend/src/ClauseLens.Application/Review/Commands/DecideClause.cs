using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;

namespace ClauseLens.Application.Review.Commands;

public sealed record DecideClauseCommand(Guid ContractId, Guid ClauseId, ClauseDecisionType Decision, string? Comment)
    : ICommand<Result<DecideClauseResponse>>;
public sealed record DecideClauseResponse(Guid DecisionId);

public sealed class DecideClauseHandler : IRequestHandler<DecideClauseCommand, Result<DecideClauseResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public DecideClauseHandler(IUnitOfWork uow, IReviewTaskRepository tasks, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tasks = tasks; _current = current; _audit = audit; }

    public async Task<Result<DecideClauseResponse>> Handle(DecideClauseCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null) return Result<DecideClauseResponse>.Failure("Authentication required.", "UNAUTHENTICATED");
        var task = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (task is null) return Result<DecideClauseResponse>.Failure("Review task not found.", "NOT_FOUND");
        var decision = ClauseDecision.Create(task.Id, cmd.ClauseId, _current.UserId.Value, cmd.Decision, cmd.Comment);
        task.RecordDecision(decision);
        task.RaiseDomainEvent(new ClauseDecidedDomainEvent(_current.TenantId ?? Guid.Empty, cmd.ClauseId, _current.UserId.Value));
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<DecideClauseResponse>.Success(new DecideClauseResponse(decision.Id));
    }
}
