using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;

namespace ClauseLens.Application.Review.Commands;

public sealed record AddSecondaryCommentCommand(Guid ContractId, Guid ClauseId, string Comment) : ICommand<Result<AddSecondaryCommentResponse>>;
public sealed record AddSecondaryCommentResponse(Guid CommentId);

/// <summary>
/// Per FR-007a: secondary reviewers (0-2 per contract) may post comments
/// only — never a decision. Their comment is recorded as an audit event
/// but does not change clause status.
/// </summary>
public sealed class AddSecondaryCommentHandler : IRequestHandler<AddSecondaryCommentCommand, Result<AddSecondaryCommentResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IReviewTaskRepository _tasks;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public AddSecondaryCommentHandler(IUnitOfWork uow, IReviewTaskRepository tasks, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tasks = tasks; _current = current; _audit = audit; }

    public async Task<Result<AddSecondaryCommentResponse>> Handle(AddSecondaryCommentCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null) return Result<AddSecondaryCommentResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        if (string.IsNullOrWhiteSpace(cmd.Comment)) return Result<AddSecondaryCommentResponse>.Failure("Comment required.", "EMPTY_COMMENT");

        var task = await _tasks.FindByContractAsync(cmd.ContractId, ct);
        if (task is null) return Result<AddSecondaryCommentResponse>.Failure("Review task not found.", "NOT_FOUND");
        if (!task.SecondaryReviewerIds.Contains(_current.UserId.Value))
            return Result<AddSecondaryCommentResponse>.Failure("Only secondary reviewers may post comments here.", "FORBIDDEN");

        // Comments are append-only audit entries. We synthesize a stable id.
        var commentId = Guid.NewGuid();
        task.RaiseDomainEvent(new SecondaryCommentAddedDomainEvent(
            _current.TenantId ?? Guid.Empty, task.Id, cmd.ClauseId, _current.UserId.Value, cmd.Comment));

        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<AddSecondaryCommentResponse>.Success(new AddSecondaryCommentResponse(commentId));
    }
}

public sealed record SecondaryCommentAddedDomainEvent(Guid TenantId, Guid ReviewTaskId, Guid ClauseId, Guid ReviewerId, string Comment) : Domain.Events.DomainEvent(TenantId);
