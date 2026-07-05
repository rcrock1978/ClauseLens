using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// An append-only record of a state-changing action. Hash-chained by the
/// TamperEvidentAuditWriter (FR-008, SC-005). Cannot be updated or deleted.
/// </summary>
public class AuditEntry : Entity
{
    public Guid TenantId { get; private set; }
    public Guid? ContractId { get; private set; }
    public Guid ActorId { get; private set; }
    public string ActionType { get; private set; } = string.Empty;
    public string? BeforeStateJson { get; private set; }
    public string? AfterStateJson { get; private set; }
    public string? CorrelationId { get; private set; }
    public string PreviousHash { get; private set; } = string.Empty;
    public string Hash { get; private set; } = string.Empty;

    private AuditEntry() { }

    public static AuditEntry Create(Guid tenantId, Guid actorId, string actionType,
                                     Guid? contractId = null, string? before = null, string? after = null,
                                     string? correlationId = null, string previousHash = "")
    {
        if (tenantId == Guid.Empty) throw new DomainException("TenantId is required.");
        if (actorId == Guid.Empty) throw new DomainException("ActorId is required.");
        if (string.IsNullOrWhiteSpace(actionType)) throw new DomainException("ActionType is required.");

        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActorId = actorId,
            ActionType = actionType.Trim(),
            ContractId = contractId,
            BeforeStateJson = before,
            AfterStateJson = after,
            CorrelationId = correlationId,
            PreviousHash = previousHash,
            CreatedAt = DateTime.UtcNow,
        };
        entry.Hash = entry.ComputeHash();
        return entry;
    }

    private string ComputeHash()
    {
        var payload = string.Join("|", Id, TenantId, ActorId, ActionType, ContractId, BeforeStateJson, AfterStateJson, CreatedAt, PreviousHash);
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }

    // Intentionally no Update / Delete — AuditEntry is append-only.
    // Any attempt to mutate fields is blocked by private setters above.
}
