using System.Security.Claims;
using ClauseLens.Application.Abstractions;

namespace ClauseLens.Infrastructure.Auth;

/// <summary>
/// Resolves the current user from the JWT bearer claims. Anonymous contexts
/// (background jobs, signup flow) receive <see cref="SystemCurrentUser"/>.
/// </summary>
public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public HttpContextCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    public Guid? UserId => TryGetGuid(ClaimTypes.NameIdentifier) ?? TryGetGuid("sub");
    public Guid? TenantId => TryGetGuid("tenant_id");
    public string? Role => _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
    public bool IsInRole(string role) => _accessor.HttpContext?.User?.IsInRole(role) ?? false;

    private Guid? TryGetGuid(string type) => Guid.TryParse(_accessor.HttpContext?.User?.FindFirstValue(type), out var g) ? g : null;
}
