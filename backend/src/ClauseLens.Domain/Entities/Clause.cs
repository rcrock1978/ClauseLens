using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A segmented portion of a <see cref="Contract"/>. Status is computed:
///   - Unreviewed        no rules matched (US2 acceptance 3)
///   - Compliant         a CompliantAnalysis result was recorded
///   - Flagged           >=1 High/Medium confidence RiskFlag exists
///   - NeedsDiscussion   reviewer chose it OR all flags have Low confidence (FR-012)
///   - Approved / Rejected   set by the primary Reviewer (US6)
/// </summary>
public class Clause : Entity
{
    public Guid ContractId { get; private set; }
    public int Index { get; private set; }
    public string Heading { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public ClauseStatus Status { get; private set; } = ClauseStatus.Unreviewed;
    public bool ContainsNonTextualContent { get; private set; }
    public string? SystemNote { get; private set; }

    private Clause() { }

    public static Clause Create(Guid contractId, int index, string heading, string text,
                                bool containsNonTextualContent = false, string? systemNote = null)
    {
        if (contractId == Guid.Empty) throw new DomainException("ContractId is required.");
        if (index < 0) throw new DomainException("Index must be >= 0.");
        if (string.IsNullOrWhiteSpace(text)) throw new DomainException("Clause text is required.");
        return new Clause
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            Index = index,
            Heading = heading?.Trim() ?? string.Empty,
            Text = text,
            ContainsNonTextualContent = containsNonTextualContent,
            SystemNote = systemNote,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Apply Clause status rules after RiskFlag and Confidence aggregation.
    /// Per FR-012: if any flag with Low confidence exists and no higher-confidence
    /// flag exists, route to NeedsDiscussion.
    /// </summary>
    public void ApplyAutoRouting(IReadOnlyCollection<RiskFlag> flags)
    {
        if (ContainsNonTextualContent)
        {
            Status = ClauseStatus.NeedsDiscussion;
            SystemNote = "Contains non-textual content — manual review required";
            return;
        }

        if (flags.Count == 0)
        {
            Status = ClauseStatus.Unreviewed;
            return;
        }

        var hasAnyHighOrMedium = flags.Any(f => f.Confidence != ConfidenceLevel.Low);
        if (hasAnyHighOrMedium)
        {
            Status = ClauseStatus.Flagged;
        }
        else
        {
            Status = ClauseStatus.NeedsDiscussion;
            SystemNote = "Low confidence — manual review recommended";
        }
    }

    public void MarkCompliant() { Status = ClauseStatus.Compliant; UpdatedAt = DateTime.UtcNow; }
    public void Approve() { Status = ClauseStatus.Approved; UpdatedAt = DateTime.UtcNow; }
    public void RejectWithComment(string comment)
    {
        Status = ClauseStatus.Rejected;
        SystemNote = comment;
        UpdatedAt = DateTime.UtcNow;
    }
    public void NeedsReviewerDiscussion() { Status = ClauseStatus.NeedsDiscussion; UpdatedAt = DateTime.UtcNow; }
}

public enum ClauseStatus
{
    Unreviewed = 0,
    Compliant = 1,
    Flagged = 2,
    NeedsDiscussion = 3,
    Approved = 4,
    Rejected = 5,
}
