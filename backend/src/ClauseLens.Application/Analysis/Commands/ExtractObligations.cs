using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using MediatR;

namespace ClauseLens.Application.Analysis.Commands;

/// <summary>
/// Per US5 / FR-006: extracts obligations from every clause in a contract
/// and persists the resulting Obligation entities.
/// </summary>
public sealed record ExtractObligationsCommand(Guid ContractId) : ICommand<Result<ExtractObligationsResponse>>;
public sealed record ExtractObligationsResponse(int ObligationsExtracted);

public sealed class ExtractObligationsHandler : IRequestHandler<ExtractObligationsCommand, Result<ExtractObligationsResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IContractRepository _contracts;
    private readonly IObligationRepository _obligations;
    private readonly IAiOrchestrator _ai;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public ExtractObligationsHandler(IUnitOfWork uow, IContractRepository contracts, IObligationRepository obligations,
                                      IAiOrchestrator ai, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _contracts = contracts; _obligations = obligations; _ai = ai; _current = current; _audit = audit; }

    public async Task<Result<ExtractObligationsResponse>> Handle(ExtractObligationsCommand cmd, CancellationToken ct)
    {
        if (_current.TenantId is null) return Result<ExtractObligationsResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<ExtractObligationsResponse>.Failure("Contract not found.", "NOT_FOUND");

        var obligations = new List<Obligation>();
        foreach (var clause in contract.Clauses)
        {
            var result = await _ai.ExtractObligationsAsync(new AiObligationsRequest(clause.Text), ct);
            foreach (var ob in result.Obligations)
                obligations.Add(Obligation.Create(clause.Id, ob.Description, ob.ResponsibleParty, ob.DueDate, ob.TriggerCondition));
        }
        await _obligations.AddRangeAsync(obligations, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<ExtractObligationsResponse>.Success(new ExtractObligationsResponse(obligations.Count));
    }
}
