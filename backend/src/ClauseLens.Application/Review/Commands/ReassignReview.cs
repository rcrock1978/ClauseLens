using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;

namespace ClauseLens.Application.Review.Commands;

public sealed record ReassignReviewCommand(Guid ContractId, Guid NewPrimaryReviewerId, string Reason)
    : ICommand<Result<ReassignReviewResponse>>;
public sealed record ReassignReviewResponse(Guid ReviewTaskId, DateTime NewSlaDeadline);

public sealed class ReassignReviewHandler : IRequestHandler<ReassignReviewCommand, Result<ReassignReviewResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly IContractRepository _contracts;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public ReassignReviewHandler(IUnitOfWork uow, IReviewTaskRepository tasks, IContractRepository contracts,
                                  ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tasks = tasks; _contracts = contracts; _current = current; _audit = audit; }

    public async Task<Result<ReassignReviewResponse>> Handle(ReassignReviewCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null || _current.TenantId is null)
            return Result<ReassignReviewResponse>.Failure("Authentication required.", "UNAUTHENTICATED");
        var task = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (task is null) return Result<ReassignReviewResponse>.Failure("Review task not found.", "NOT_FOUND");
        if (task.SlaDeadline > DateTime.UtcNow)
            return Result<ReassignReviewResponse>.Failure("Reassignment only allowed after SLA expiry (day 7).", "SLA_NOT_REACHED");

        var old = task.PrimaryReviewerId;
        task.Reassign(cmd.NewPrimaryReviewerId, cmd.Reason, DateTime.UtcNow);
        task.RaiseDomainEvent(new ReviewTaskReassignedDomainEvent(_current.TenantId.Value, task.Id, old, cmd.NewPrimaryReviewerId, cmd.Reason));
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<ReassignReviewResponse>.Success(new ReassignReviewResponse(task.Id, task.SlaDeadline));
    }
}
