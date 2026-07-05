using ClauseLens.Application.Abstractions;
using ClauseLens.Application.Behaviors;
using ClauseLens.Domain.Common;
using ClauseLens.Domain.Events;
using MassTransit;
using MediatR;

namespace ClauseLens.Infrastructure.Audit;

/// <summary>
/// Collects domain events from all aggregate roots loaded by the current
/// DbContext and dispatches them to the AuditEntry writer AND to the
/// MassTransit outbox. Honors FR-008 (append-only) and SC-005 (no data
/// loss, no tampering). Per-tenant hash chain is sourced from the request's
/// tenant context (or the background-job system tenant context) — never
/// from Guid.Empty.
/// </summary>
public sealed class AuditEventDispatcher : IAuditEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _uow;
    private readonly IAuditRepository _audit;
    private readonly Infrastructure.Persistence.ClauseLensDbContext _db;
    private readonly ISystemTenantContext _systemTenant;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuditEventDispatcher(IMediator mediator, IUnitOfWork uow, IAuditRepository audit,
                                 Infrastructure.Persistence.ClauseLensDbContext db,
                                 ISystemTenantContext systemTenant,
                                 IPublishEndpoint publishEndpoint)
    { _mediator = mediator; _uow = uow; _audit = audit; _db = db; _systemTenant = systemTenant; _publishEndpoint = publishEndpoint; }

    public async Task DispatchPendingAsync(ICurrentUser current, CancellationToken ct = default)
    {
        var aggregates = _db.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Any())
            .ToList();

        if (aggregates.Count == 0) return;

        var tenantId = current.TenantId ?? _systemTenant.CurrentTenantId;
        if (tenantId is null || tenantId == Guid.Empty)
        {
            foreach (var agg in aggregates) agg.ClearDomainEvents();
            return;
        }

        var previousHash = await _audit.GetLatestHashAsync(tenantId.Value, ct);
        var capturedEvents = new List<IDomainEvent>();

        foreach (var agg in aggregates)
        {
            foreach (var @event in agg.DomainEvents)
            {
                var entry = AuditEntry.Create(
                    tenantId: @event.TenantId == Guid.Empty ? tenantId.Value : @event.TenantId,
                    actorId: current.UserId ?? Guid.Empty,
                    actionType: @event.GetType().Name,
                    contractId: ExtractContractId(@event),
                    before: null,
                    after: System.Text.Json.JsonSerializer.Serialize(@event),
                    correlationId: System.Diagnostics.Activity.Current?.TraceId.ToString(),
                    previousHash: previousHash
                );
                await _audit.AddAsync(entry, ct);
                previousHash = entry.Hash;
                capturedEvents.Add(@event);
            }
            agg.ClearDomainEvents();
        }

        // T180: enqueue every captured event to the MassTransit outbox so
        // downstream consumers (analytics, billing, etc.) can react.
        foreach (var @event in capturedEvents)
            await _publishEndpoint.Publish(@event, ct);

        await _uow.SaveChangesAsync(ct);
    }

    private static Guid? ExtractContractId(IDomainEvent @event) => @event switch
    {
        ContractUploadedDomainEvent e => e.ContractId,
        ContractAnalyzedDomainEvent e => e.ContractId,
        ReviewTaskAssignedDomainEvent e => e.ContractId,
        ReviewTaskSubmittedDomainEvent e => e.ContractId,
        ReviewerSlaNudgedDomainEvent e => null,
        ReviewerRevisionsRequestedDomainEvent e => null,
        ContractResubmittedDomainEvent e => e.ContractId,
        _ => null,
    };
}
