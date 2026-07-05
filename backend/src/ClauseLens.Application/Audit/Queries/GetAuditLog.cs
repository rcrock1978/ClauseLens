using ClauseLens.Application.Abstractions;
using MediatR;

namespace ClauseLens.Application.Audit.Queries;

public sealed record GetAuditLogQuery(Guid? ContractId, string? ActionType, int Skip, int Take)
    : IQuery<Result<IReadOnlyList<AuditEntryDto>>>;

public sealed record AuditEntryDto(Guid Id, Guid TenantId, Guid? ContractId, Guid ActorId, string ActionType, DateTime CreatedAt, string Hash);

public sealed class GetAuditLogHandler : IRequestHandler<GetAuditLogQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    private readonly IAuditRepository _audit;
    private readonly ICurrentUser _current;
    public GetAuditLogHandler(IAuditRepository audit, ICurrentUser current) { _audit = audit; _current = current; }

    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(GetAuditLogQuery q, CancellationToken ct)
    {
        if (_current.TenantId is null) return Result<IReadOnlyList<AuditEntryDto>>.Failure("Auth required.", "UNAUTHENTICATED");
        var entries = await _audit.ListAsync(_current.TenantId.Value, q.ContractId, q.ActionType, q.Skip, q.Take, ct);
        return Result<IReadOnlyList<AuditEntryDto>>.Success(entries
            .Select(e => new AuditEntryDto(e.Id, e.TenantId, e.ContractId, e.ActorId, e.ActionType, e.CreatedAt, e.Hash))
            .ToList());
    }
}
