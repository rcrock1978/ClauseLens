using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// An isolated customer organization. Holds retention settings, lifecycle state,
/// and the seed of every tenant-scoped aggregate (Contracts, Playbooks, Users, etc.).
/// </summary>
public class Tenant : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public TenantStatus Status { get; private set; } = TenantStatus.Active;
    public DateTime? SoftDeleteScheduledAt { get; private set; }
    public DateTime? OffboardedAt { get; private set; }
    public int RetentionYearsContracts { get; private set; } = 7;
    public int RetentionYearsAudit { get; private set; } = 7;

    private Tenant() { }

    public static Tenant Create(string name, int retentionYearsContracts = 7, int retentionYearsAudit = 7)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name is required.");
        if (retentionYearsContracts < 1 || retentionYearsAudit < 1)
            throw new DomainException("Retention years must be >= 1.");

        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            RetentionYearsContracts = retentionYearsContracts,
            RetentionYearsAudit = retentionYearsAudit,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateRetention(int contractsYears, int auditYears)
    {
        if (Status != TenantStatus.Active)
            throw new DomainException("Cannot update retention on a non-active tenant.");
        RetentionYearsContracts = contractsYears;
        RetentionYearsAudit = auditYears;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ScheduleOffboarding(DateTime now)
    {
        if (Status != TenantStatus.Active)
            throw new DomainException("Tenant is already offboarding.");
        Status = TenantStatus.SoftDeleted;
        OffboardedAt = now;
        SoftDeleteScheduledAt = now.AddDays(30);
        UpdatedAt = now;
    }

    public void HardDelete(DateTime now)
    {
        if (Status != TenantStatus.SoftDeleted)
            throw new DomainException("Only soft-deleted tenants can be hard-deleted.");
        if (SoftDeleteScheduledAt is null || now < SoftDeleteScheduledAt)
            throw new DomainException("Hard delete scheduled time has not been reached.");
        Status = TenantStatus.HardDeleted;
        UpdatedAt = now;
    }
}

public enum TenantStatus
{
    Active = 0,
    SoftDeleted = 1,
    HardDeleted = 2,
}
