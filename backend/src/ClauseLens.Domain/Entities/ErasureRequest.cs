using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A GDPR right-to-erasure request. Per FR-013, honored within 30 days.
/// Tracks subject (email), scope (user/contract/audit_entry), and the SLA
/// deadline so the <c>ErasureProcessor</c> background service can find and
/// process requests whose SLA has elapsed.
/// </summary>
public class ErasureRequest : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string SubjectEmail { get; private set; } = string.Empty;
    public ErasureScope Scope { get; private set; }
    public ErasureStatus Status { get; private set; } = ErasureStatus.Pending;
    public DateTime SlaDeadline { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid RequestedById { get; private set; }

    private ErasureRequest() { }

    public static ErasureRequest Create(Guid tenantId, string subjectEmail, ErasureScope scope,
                                          Guid requestedById, DateTime now, int slaDays = 30)
    {
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (string.IsNullOrWhiteSpace(subjectEmail)) throw new DomainException("SubjectEmail is required.");
        if (requestedById == Guid.Empty) throw new DomainException("RequestedById is required.");
        return new ErasureRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SubjectEmail = subjectEmail.Trim().ToLowerInvariant(),
            Scope = scope,
            RequestedById = requestedById,
            SlaDeadline = now.AddDays(slaDays),
            CreatedAt = now,
        };
    }

    public void MarkCompleted(DateTime now)
    {
        if (Status == ErasureStatus.Completed) return;
        Status = ErasureStatus.Completed;
        CompletedAt = now;
        UpdatedAt = now;
    }
}

public enum ErasureScope { User = 0, Contract = 1, AuditEntry = 2 }
public enum ErasureStatus { Pending = 0, Completed = 1 }
