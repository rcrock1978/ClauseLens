using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;

namespace ClauseLens.Application.Review.Commands;

public sealed record RequestRevisionsCommand(Guid ContractId, Guid ClauseId, string Comment) : ICommand<Result<RequestRevisionsResponse>>;
public sealed record RequestRevisionsResponse(Guid ContractId, string ContractStatus);

/// <summary>
/// Per US6/AC3: when a reviewer rejects a clause with revisions requested, the
/// Contract transitions to RevisionsRequested and returns to ReadyForReview
/// once the Owner supplies an updated redline (see ResubmitRevisedContractCommand).
/// </summary>
public sealed class RequestRevisionsHandler : IRequestHandler<RequestRevisionsCommand, Result<RequestRevisionsResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly IContractRepository _contracts;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public RequestRevisionsHandler(IUnitOfWork uow, IReviewTaskRepository tasks, IContractRepository contracts,
                                   ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tasks = tasks; _contracts = contracts; _current = current; _audit = audit; }

    public async Task<Result<RequestRevisionsResponse>> Handle(RequestRevisionsCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null) return Result<RequestRevisionsResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        if (string.IsNullOrWhiteSpace(cmd.Comment)) return Result<RequestRevisionsResponse>.Failure("Comment required.", "EMPTY_COMMENT");

        var task = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (task is null) return Result<RequestRevisionsResponse>.Failure("Review task not found.", "NOT_FOUND");
        if (task.PrimaryReviewerId != _current.UserId.Value)
            return Result<RequestRevisionsResponse>.Failure("Only the primary reviewer can request revisions.", "FORBIDDEN");

        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<RequestRevisionsResponse>.Failure("Contract not found.", "NOT_FOUND");
        if (contract.Status != ContractStatus.InReview)
            return Result<RequestRevisionsResponse>.Failure("Contract is not InReview.", "INVALID_STATE");

        var clause = contract.Clauses.FirstOrDefault(c => c.Id == cmd.ClauseId);
        if (clause is null) return Result<RequestRevisionsResponse>.Failure("Clause not found.", "NOT_FOUND");

        clause.RejectWithComment(cmd.Comment);
        contract.RequestRevisions();
        task.RaiseDomainEvent(new ReviewerRevisionsRequestedDomainEvent(_current.TenantId ?? Guid.Empty, task.Id, cmd.ClauseId, cmd.Comment));

        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<RequestRevisionsResponse>.Success(new RequestRevisionsResponse(contract.Id, contract.Status.ToString()));
    }
}

public sealed record ResubmitRevisedContractCommand(Guid ContractId, IReadOnlyList<Guid> RevisedClauseIds) : ICommand<Result<ResubmitRevisedContractResponse>>;
public sealed record ResubmitRevisedContractResponse(string ContractStatus);

/// <summary>
/// Per US6/AC3: Owner supplies updated redlines for the rejected clauses and
/// returns the contract to ReadyForReview. Triggers a new SLA window for the
/// existing review task (or creates a new task if the previous one was reassigned).
/// </summary>
public sealed class ResubmitRevisedContractHandler : IRequestHandler<ResubmitRevisedContractCommand, Result<ResubmitRevisedContractResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly IContractRepository _contracts;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public ResubmitRevisedContractHandler(IUnitOfWork uow, IReviewTaskRepository tasks, IContractRepository contracts,
                                          ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tasks = tasks; _contracts = contracts; _current = current; _audit = audit; }

    public async Task<Result<ResubmitRevisedContractResponse>> Handle(ResubmitRevisedContractCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null || _current.TenantId is null)
            return Result<ResubmitRevisedContractResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        if (!_current.IsInRole(nameof(UserRole.ContractOwner)))
            return Result<ResubmitRevisedContractResponse>.Failure("Only Contract Owners can resubmit.", "FORBIDDEN");

        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<ResubmitRevisedContractResponse>.Failure("Contract not found.", "NOT_FOUND");
        if (contract.Status != ContractStatus.RevisionsRequested)
            return Result<ResubmitRevisedContractResponse>.Failure("Contract is not in RevisionsRequested.", "INVALID_STATE");

        // Reset the rejected clauses' statuses so the reviewer sees them again.
        foreach (var cid in cmd.RevisedClauseIds)
        {
            var c = contract.Clauses.FirstOrDefault(x => x.Id == cid);
            if (c is null) continue;
            c.NeedsReviewerDiscussion();
        }
        contract.Resubmit();

        var task = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (task is not null)
            task.RaiseDomainEvent(new ContractResubmittedDomainEvent(_current.TenantId.Value, contract.Id, task.Id));

        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<ResubmitRevisedContractResponse>.Success(new ResubmitRevisedContractResponse(contract.Status.ToString()));
    }
}

public sealed record ReviewerRevisionsRequestedDomainEvent(Guid TenantId, Guid ReviewTaskId, Guid ClauseId, string Comment) : Domain.Events.DomainEvent(TenantId);
public sealed record ContractResubmittedDomainEvent(Guid TenantId, Guid ContractId, Guid ReviewTaskId) : Domain.Events.DomainEvent(TenantId);
