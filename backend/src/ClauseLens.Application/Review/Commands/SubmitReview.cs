using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;

namespace ClauseLens.Application.Review.Commands;

public sealed record SubmitReviewCommand(Guid ContractId) : ICommand<Result<SubmitReviewResponse>>;
public sealed record SubmitReviewResponse(Guid ReviewTaskId, string ContractStatus);

public sealed class SubmitReviewHandler : IRequestHandler<SubmitReviewCommand, Result<SubmitReviewResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly IContractRepository _contracts;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public SubmitReviewHandler(IUnitOfWork uow, IReviewTaskRepository tasks, IContractRepository contracts,
                                ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tasks = tasks; _contracts = contracts; _current = current; _audit = audit; }

    public async Task<Result<SubmitReviewResponse>> Handle(SubmitReviewCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null) return Result<SubmitReviewResponse>.Failure("Authentication required.", "UNAUTHENTICATED");
        var task = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (task is null) return Result<SubmitReviewResponse>.Failure("Review task not found.", "NOT_FOUND");
        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<SubmitReviewResponse>.Failure("Contract not found.", "NOT_FOUND");

        task.Submit(DateTime.UtcNow);
        contract.CompleteReview();
        task.RaiseDomainEvent(new ReviewTaskSubmittedDomainEvent(_current.TenantId ?? Guid.Empty, contract.Id, task.Id));
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<SubmitReviewResponse>.Success(new SubmitReviewResponse(task.Id, contract.Status.ToString()));
    }
}
