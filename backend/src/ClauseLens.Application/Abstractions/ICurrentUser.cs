namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Tenant and current-user scope for the executing request. Populated by the
/// auth middleware and consumed by handlers + the global query filter.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;
    public Guid? TenantId => null;
    public string? Role => null;
    public bool IsAuthenticated => false;
    public bool IsInRole(string role) => false;
}
