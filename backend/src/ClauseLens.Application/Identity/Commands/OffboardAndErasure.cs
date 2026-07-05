using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using MediatR;

namespace ClauseLens.Application.Identity.Commands;

public sealed record OffboardTenantCommand(Guid TenantId) : ICommand<Result<OffboardTenantResponse>>;
public sealed record OffboardTenantResponse(Guid TenantId, DateTime SoftDeleteScheduledAt);

public sealed class OffboardTenantHandler : IRequestHandler<OffboardTenantCommand, Result<OffboardTenantResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantRepository _tenants;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public OffboardTenantHandler(IUnitOfWork uow, ITenantRepository tenants, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _tenants = tenants; _current = current; _audit = audit; }

    public async Task<Result<OffboardTenantResponse>> Handle(OffboardTenantCommand cmd, CancellationToken ct)
    {
        if (!_current.IsInRole(nameof(UserRole.Admin))) return Result<OffboardTenantResponse>.Failure("Admins only.", "FORBIDDEN");
        var t = await _tenants.FindByIdAsync(cmd.TenantId, ct);
        if (t is null) return Result<OffboardTenantResponse>.Failure("Tenant not found.", "NOT_FOUND");
        var now = DateTime.UtcNow;
        t.ScheduleOffboarding(now);
        t.RaiseDomainEvent(new TenantOffboardedDomainEvent(t.Id));
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<OffboardTenantResponse>.Success(new OffboardTenantResponse(t.Id, t.SoftDeleteScheduledAt!.Value));
    }
}

public sealed record RequestErasureCommand(string SubjectEmail, ErasureScope Scope) : ICommand<Result<RequestErasureResponse>>;
public sealed record RequestErasureResponse(string RequestId, DateTime SlaDeadline);

public sealed class RequestErasureHandler : IRequestHandler<RequestErasureCommand, Result<RequestErasureResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IErasureRequestRepository _erasure;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public RequestErasureHandler(IUnitOfWork uow, IErasureRequestRepository erasure, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _erasure = erasure; _current = current; _audit = audit; }

    public async Task<Result<RequestErasureResponse>> Handle(RequestErasureCommand cmd, CancellationToken ct)
    {
        if (_current.TenantId is null || _current.UserId is null)
            return Result<RequestErasureResponse>.Failure("Auth required.", "UNAUTHENTICATED");

        var request = ErasureRequest.Create(_current.TenantId.Value, cmd.SubjectEmail, cmd.Scope, _current.UserId.Value, DateTime.UtcNow);
        await _erasure.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<RequestErasureResponse>.Success(new RequestErasureResponse(request.Id.ToString("N"), request.SlaDeadline));
    }
}
