using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A record linking a <see cref="Clause"/> to one or more <see cref="PlaybookRule"/>s
/// it violates. Per FR-009a, multiple matching rules produce a single flag with
/// the highest severity and all rule IDs aggregated in <see cref="MatchedRuleIds"/>.
/// </summary>
public class RiskFlag : Entity
{
    public Guid ClauseId { get; private set; }
    public Guid RuleId { get; private set; }
    public List<Guid> MatchedRuleIds { get; private set; } = new();
    public RiskSeverity Severity { get; private set; }
    public ConfidenceLevel Confidence { get; private set; }
    public string Rationale { get; private set; } = string.Empty;
    public DateTime? ResolvedAt { get; private set; }

    private RiskFlag() { }

    public static RiskFlag Create(Guid clauseId, Guid primaryRuleId, IEnumerable<Guid> matchedRuleIds,
                                   RiskSeverity highestSeverity, ConfidenceLevel confidence, string rationale)
    {
        if (clauseId == Guid.Empty || primaryRuleId == Guid.Empty)
            throw new DomainException("ClauseId and primary RuleId are required.");
        if (string.IsNullOrWhiteSpace(rationale))
            throw new DomainException("Rationale is required.");

        return new RiskFlag
        {
            Id = Guid.NewGuid(),
            ClauseId = clauseId,
            RuleId = primaryRuleId,
            MatchedRuleIds = matchedRuleIds.Distinct().ToList(),
            Severity = highestSeverity,
            Confidence = confidence,
            Rationale = rationale.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Resolve() { ResolvedAt = DateTime.UtcNow; UpdatedAt = DateTime.UtcNow; }
}

public enum ConfidenceLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
}

/// <summary>
/// A suggested revision for a <see cref="RiskFlag"/>. Confidence propagates from
/// the originating flag (per FR-012).
/// </summary>
public class Redline : Entity
{
    public Guid RiskFlagId { get; private set; }
    public string SuggestedText { get; private set; } = string.Empty;
    public string Rationale { get; private set; } = string.Empty;
    public string Citations { get; private set; } = string.Empty;
    public ConfidenceLevel Confidence { get; private set; }
    public RedlineStatus Status { get; private set; } = RedlineStatus.Pending;

    private Redline() { }

    public static Redline Create(Guid riskFlagId, string suggestedText, string rationale,
                                  string citations, ConfidenceLevel confidence)
    {
        if (riskFlagId == Guid.Empty) throw new DomainException("RiskFlagId is required.");
        if (string.IsNullOrWhiteSpace(suggestedText)) throw new DomainException("SuggestedText is required.");
        if (string.IsNullOrWhiteSpace(citations)) throw new DomainException("Citations are required (per FR-004 / Principle V).");

        return new Redline
        {
            Id = Guid.NewGuid(),
            RiskFlagId = riskFlagId,
            SuggestedText = suggestedText.Trim(),
            Rationale = rationale?.Trim() ?? string.Empty,
            Citations = citations.Trim(),
            Confidence = confidence,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Accept() { Status = RedlineStatus.Accepted; UpdatedAt = DateTime.UtcNow; }
    public void Reject() { Status = RedlineStatus.Rejected; UpdatedAt = DateTime.UtcNow; }
}

public enum RedlineStatus { Pending = 0, Accepted = 1, Rejected = 2 }

/// <summary>
/// An extracted duty from the contract (per US5). Party is normalized to title-case.
/// </summary>
public class Obligation : Entity
{
    public Guid ClauseId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string ResponsibleParty { get; private set; } = string.Empty;
    public DateTime? DueDate { get; private set; }
    public string? TriggerCondition { get; private set; }

    private Obligation() { }

    public static Obligation Create(Guid clauseId, string description, string responsibleParty,
                                     DateTime? dueDate = null, string? triggerCondition = null)
    {
        if (clauseId == Guid.Empty) throw new DomainException("ClauseId is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        if (string.IsNullOrWhiteSpace(responsibleParty)) throw new DomainException("ResponsibleParty is required.");
        return new Obligation
        {
            Id = Guid.NewGuid(),
            ClauseId = clauseId,
            Description = description.Trim(),
            ResponsibleParty = responsibleParty.Trim(),
            DueDate = dueDate,
            TriggerCondition = triggerCondition?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }
}
