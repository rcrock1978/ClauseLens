using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Application.Review.Commands;

public sealed record AssignReviewCommand(Guid ContractId, Guid PrimaryReviewerId, List<Guid> SecondaryReviewerIds)
    : ICommand<Result<AssignReviewResponse>>;

public sealed record AssignReviewResponse(Guid ReviewTaskId, DateTime SlaDeadline);

public sealed class AssignReviewHandler : IRequestHandler<AssignReviewCommand, Result<AssignReviewResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly IContractRepository _contracts;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;
    private readonly ILogger<AssignReviewHandler> _logger;

    public AssignReviewHandler(IUnitOfWork uow, IReviewTaskRepository tasks, IContractRepository contracts,
                                ICurrentUser current, IAuditEventDispatcher audit, ILogger<AssignReviewHandler> logger)
    { _uow = uow; _tasks = tasks; _contracts = contracts; _current = current; _audit = audit; _logger = logger; }

    public async Task<Result<AssignReviewResponse>> Handle(AssignReviewCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null || _current.TenantId is null)
            return Result<AssignReviewResponse>.Failure("Authentication required.", "UNAUTHENTICATED");
        if (!_current.IsInRole(nameof(UserRole.ContractOwner)))
            return Result<AssignReviewResponse>.Failure("Only Contract Owners can assign reviews.", "FORBIDDEN");

        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<AssignReviewResponse>.Failure("Contract not found.", "NOT_FOUND");
        if (contract.Status != Domain.Entities.ContractStatus.ReadyForReview && contract.Status != Domain.Entities.ContractStatus.RevisionsRequested)
            return Result<AssignReviewResponse>.Failure("Contract is not ready for review.", "INVALID_STATE");

        var existing = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (existing is not null && existing.Status != ReviewTaskStatus.Reassigned)
            return Result<AssignReviewResponse>.Failure("A review is already in progress for this contract.", "ALREADY_ASSIGNED");

        var now = DateTime.UtcNow;
        var task = ReviewTask.Create(cmd.ContractId, cmd.PrimaryReviewerId, cmd.SecondaryReviewerIds, _current.UserId.Value, now);
        await _tasks.AddAsync(task, ct);
        contract.StartReview();
        task.RaiseDomainEvent(new ReviewTaskAssignedDomainEvent(_current.TenantId.Value, contract.Id, task.Id, cmd.PrimaryReviewerId));

        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<AssignReviewResponse>.Success(new AssignReviewResponse(task.Id, task.SlaDeadline));
    }
}
