namespace ClauseLens.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
    Guid TenantId { get; }
}

public abstract record DomainEvent(Guid TenantId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public sealed record TenantSignedUpDomainEvent(Guid TenantId, Guid FirstAdminId) : DomainEvent(TenantId);
public sealed record ContractUploadedDomainEvent(Guid TenantId, Guid ContractId, Guid OwnerId) : DomainEvent(TenantId);
public sealed record ContractAnalyzedDomainEvent(Guid TenantId, Guid ContractId) : DomainEvent(TenantId);
public sealed record ReviewTaskAssignedDomainEvent(Guid TenantId, Guid ContractId, Guid ReviewTaskId, Guid PrimaryReviewerId) : DomainEvent(TenantId);
public sealed record ReviewTaskReassignedDomainEvent(Guid TenantId, Guid ReviewTaskId, Guid OldReviewerId, Guid NewReviewerId, string Reason) : DomainEvent(TenantId);
public sealed record ClauseDecidedDomainEvent(Guid TenantId, Guid ClauseId, Guid ReviewerId) : DomainEvent(TenantId);
public sealed record ReviewTaskSubmittedDomainEvent(Guid TenantId, Guid ContractId, Guid ReviewTaskId) : DomainEvent(TenantId);
public sealed record ErasureRequestedDomainEvent(Guid TenantId, string SubjectEmail) : DomainEvent(TenantId);
public sealed record TenantOffboardedDomainEvent(Guid TenantId) : DomainEvent(TenantId);
public sealed record ReviewerSlaNudgedDomainEvent(Guid TenantId, Guid ReviewTaskId, Guid PrimaryReviewerId, DateTime NudgedAt) : DomainEvent(TenantId);
