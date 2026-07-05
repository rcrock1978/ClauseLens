namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Per-tenant scope provider. Unlike <see cref="ICurrentUser"/> (which is
/// per-request and null for background jobs), <see cref="ISystemTenantContext"/>
/// exposes the tenant under which a system job is currently running.
/// Background services MUST obtain the tenant before calling any code that
/// hashes audit chains or applies tenant-scoped queries.
/// </summary>
public interface ISystemTenantContext
{
    Guid? CurrentTenantId { get; }
    void SetCurrentTenant(Guid tenantId);
    void Clear();
}
