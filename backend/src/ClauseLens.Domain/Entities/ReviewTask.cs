using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A review assignment for a <see cref="Contract"/>. Per FR-007a:
///   - One primary Reviewer (the decider)
///   - 0..2 secondary Reviewers (comments only, non-blocking)
/// Per FR-007b: 7 business-day SLA on the primary reviewer.
/// </summary>
public class ReviewTask : AggregateRoot
{
    public Guid ContractId { get; private set; }
    public Guid PrimaryReviewerId { get; private set; }
    public List<Guid> SecondaryReviewerIds { get; private set; } = new();
    public Guid AssignedById { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime SlaDeadline { get; private set; }
    public DateTime? SlaNudgedAt { get; private set; }
    public Guid? ReassignedFromId { get; private set; }
    public string? ReassignmentReason { get; private set; }
    public ReviewTaskStatus Status { get; private set; } = ReviewTaskStatus.InProgress;
    public DateTime? SubmittedAt { get; private set; }

    private readonly List<ClauseDecision> _decisions = new();
    public IReadOnlyCollection<ClauseDecision> Decisions => _decisions.AsReadOnly();

    private ReviewTask() { }

    public static ReviewTask Create(Guid contractId, Guid primaryReviewerId, IEnumerable<Guid>? secondaryReviewerIds,
                                     Guid assignedById, DateTime now, int businessDaySla = 7)
    {
        if (contractId == Guid.Empty || primaryReviewerId == Guid.Empty || assignedById == Guid.Empty)
            throw new DomainException("ContractId, PrimaryReviewerId, and AssignedById are required.");

        var secondaries = (secondaryReviewerIds ?? Enumerable.Empty<Guid>()).Distinct().ToList();
        if (secondaries.Count > 2)
            throw new DomainException("A review task can have at most 2 secondary reviewers (per FR-007a).");
        if (secondaries.Contains(primaryReviewerId))
            throw new DomainException("Primary reviewer cannot also be a secondary reviewer.");

        return new ReviewTask
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            PrimaryReviewerId = primaryReviewerId,
            SecondaryReviewerIds = secondaries,
            AssignedById = assignedById,
            AssignedAt = now,
            SlaDeadline = AddBusinessDays(now, businessDaySla),
            CreatedAt = now,
        };
    }

    public void RecordSlaNudge(DateTime now)
    {
        if (SlaNudgedAt.HasValue) return;
        SlaNudgedAt = now;
        UpdatedAt = now;
        RaiseDomainEvent(new ReviewerSlaNudgedDomainEvent(
            TenantId, Id, PrimaryReviewerId, now));
    }

    public void Reassign(Guid newPrimaryReviewerId, string reason, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reassignment reason is required (per FR-007b).");
        ReassignedFromId = PrimaryReviewerId;
        PrimaryReviewerId = newPrimaryReviewerId;
        ReassignmentReason = reason;
        AssignedAt = now;
        SlaDeadline = AddBusinessDays(now, 7);
        SlaNudgedAt = null;
        Status = ReviewTaskStatus.Reassigned;
        UpdatedAt = now;
    }

    public void RecordDecision(ClauseDecision decision)
    {
        if (decision.ReviewerId != PrimaryReviewerId)
            throw new DomainException("Only the primary reviewer can submit clause decisions (per FR-007a).");
        _decisions.Add(decision);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit(DateTime now)
    {
        Status = ReviewTaskStatus.Submitted;
        SubmittedAt = now;
        UpdatedAt = now;
    }

    public static DateTime AddBusinessDays(DateTime start, int businessDays)
    {
        var d = start;
        var added = 0;
        while (added < businessDays)
        {
            d = d.AddDays(1);
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) added++;
        }
        return d;
    }
}

public enum ReviewTaskStatus
{
    InProgress = 0,
    Reassigned = 1,
    Submitted = 2,
}

/// <summary>
/// A reviewer's per-clause verdict. Only the primary reviewer can submit these.
/// </summary>
public class ClauseDecision : Entity
{
    public Guid ReviewTaskId { get; private set; }
    public Guid ClauseId { get; private set; }
    public Guid ReviewerId { get; private set; }
    public ClauseDecisionType Decision { get; private set; }
    public string? Comment { get; private set; }

    private ClauseDecision() { }

    public static ClauseDecision Create(Guid reviewTaskId, Guid clauseId, Guid reviewerId,
                                          ClauseDecisionType decision, string? comment = null)
    {
        if (reviewTaskId == Guid.Empty || clauseId == Guid.Empty || reviewerId == Guid.Empty)
            throw new DomainException("ReviewTaskId, ClauseId, and ReviewerId are required.");
        return new ClauseDecision
        {
            Id = Guid.NewGuid(),
            ReviewTaskId = reviewTaskId,
            ClauseId = clauseId,
            ReviewerId = reviewerId,
            Decision = decision,
            Comment = comment?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }
}

public enum ClauseDecisionType
{
    Approved = 0,
    RejectedWithComment = 1,
    NeedsDiscussion = 2,
}
