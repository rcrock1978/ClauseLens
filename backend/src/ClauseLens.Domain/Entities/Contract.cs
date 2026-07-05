using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A contract uploaded by a Contract Owner. State machine:
///   Uploaded -> Analyzing -> ReadyForReview -> InReview -> Reviewed
///                         ^                     |
///                         └── RevisionsRequested ┘ (per FR, triggered by reviewer rejection)
/// </summary>
public class Contract : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string FileFormat { get; private set; } = string.Empty;
    public string BlobUri { get; private set; } = string.Empty;
    public ContractStatus Status { get; private set; } = ContractStatus.Uploaded;
    public DateTime? AnalyzedAt { get; private set; }

    private readonly List<Clause> _clauses = new();
    public IReadOnlyCollection<Clause> Clauses => _clauses.AsReadOnly();

    private Contract() { }

    public static Contract Create(Guid tenantId, Guid ownerId, string fileName, long fileSize, string fileFormat, string blobUri)
    {
        if (tenantId == Guid.Empty || ownerId == Guid.Empty)
            throw new DomainException("TenantId and OwnerId are required.");
        if (fileSize <= 0 || fileSize > 25 * 1024 * 1024)
            throw new DomainException("File size must be > 0 and <= 25 MB.");
        var fmt = fileFormat?.ToLowerInvariant();
        if (fmt != "pdf" && fmt != "docx")
            throw new DomainException("File format must be pdf or docx.");

        return new Contract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerId = ownerId,
            FileName = fileName,
            FileSize = fileSize,
            FileFormat = fmt!,
            BlobUri = blobUri,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void ReplaceClauses(IEnumerable<Clause> clauses)
    {
        _clauses.Clear();
        _clauses.AddRange(clauses);
        Status = ContractStatus.Analyzing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkReadyForReview(DateTime now)
    {
        if (Status != ContractStatus.Analyzing)
            throw new DomainException($"Cannot transition to ReadyForReview from {Status}.");
        Status = ContractStatus.ReadyForReview;
        AnalyzedAt = now;
        UpdatedAt = now;
    }

    public void StartReview() => TransitionTo(ContractStatus.InReview);
    public void RequestRevisions() => TransitionTo(ContractStatus.RevisionsRequested);
    public void CompleteReview() => TransitionTo(ContractStatus.Reviewed);
    public void Resubmit() => TransitionTo(ContractStatus.ReadyForReview);

    private void TransitionTo(ContractStatus target)
    {
        if (!IsValidTransition(Status, target))
            throw new DomainException($"Invalid Contract transition: {Status} -> {target}");
        Status = target;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidTransition(ContractStatus from, ContractStatus to) => (from, to) switch
    {
        (ContractStatus.Analyzing, ContractStatus.ReadyForReview) => true,
        (ContractStatus.ReadyForReview, ContractStatus.InReview) => true,
        (ContractStatus.InReview, ContractStatus.RevisionsRequested) => true,
        (ContractStatus.InReview, ContractStatus.Reviewed) => true,
        (ContractStatus.RevisionsRequested, ContractStatus.ReadyForReview) => true,
        _ => false,
    };
}

public enum ContractStatus
{
    Uploaded = 0,
    Analyzing = 1,
    ReadyForReview = 2,
    InReview = 3,
    RevisionsRequested = 4,
    Reviewed = 5,
}
