using System.Collections.Concurrent;
using ClauseLens.Application.Abstractions;

namespace ClauseLens.Infrastructure.Compliance;

/// <summary>
/// Async-local implementation of <see cref="ISystemTenantContext"/>. Background
/// services set the tenant at the start of each per-tenant tick and clear it
/// at the end. Replaces the Guid.Empty fallback in AuditEventDispatcher.
/// </summary>
public sealed class SystemTenantContext : ISystemTenantContext
{
    private static readonly AsyncLocal<Guid?> _current = new();
    public Guid? CurrentTenantId => _current.Value;
    public void SetCurrentTenant(Guid tenantId) => _current.Value = tenantId;
    public void Clear() => _current.Value = null;
}
