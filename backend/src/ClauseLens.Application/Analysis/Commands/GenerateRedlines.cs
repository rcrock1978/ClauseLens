using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using MediatR;

namespace ClauseLens.Application.Analysis.Commands;

/// <summary>
/// Per FR-004: orchestrates redline generation for every RiskFlag on a
/// contract. Each call is parallelized via the AI service. Resulting Redline
/// entities are persisted in batch.
/// </summary>
public sealed record GenerateRedlinesCommand(Guid ContractId) : ICommand<Result<GenerateRedlinesResponse>>;
public sealed record GenerateRedlinesResponse(int RedlinesEmitted);

public sealed class GenerateRedlinesHandler : IRequestHandler<GenerateRedlinesCommand, Result<GenerateRedlinesResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IContractRepository _contracts;
    private readonly IRiskFlagRepository _riskFlags;
    private readonly IRedlineRepository _redlines;
    private readonly IAiOrchestrator _ai;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public GenerateRedlinesHandler(IUnitOfWork uow, IContractRepository contracts, IRiskFlagRepository riskFlags,
                                    IRedlineRepository redlines, IAiOrchestrator ai, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _contracts = contracts; _riskFlags = riskFlags; _redlines = redlines; _ai = ai; _current = current; _audit = audit; }

    public async Task<Result<GenerateRedlinesResponse>> Handle(GenerateRedlinesCommand cmd, CancellationToken ct)
    {
        if (_current.TenantId is null) return Result<GenerateRedlinesResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<GenerateRedlinesResponse>.Failure("Contract not found.", "NOT_FOUND");

        var flags = await _riskFlags.ListByContractAsync(cmd.ContractId, ct);
        if (flags.Count == 0) return Result<GenerateRedlinesResponse>.Success(new GenerateRedlinesResponse(0));

        // Parallel calls to the AI service
        var tasks = flags.Select(async flag =>
        {
            var clause = contract.Clauses.FirstOrDefault(c => c.Id == flag.ClauseId);
            if (clause is null) return null;
            var result = await _ai.GenerateRedlineAsync(new AiRedlineRequest(
                RiskFlagId: flag.Id,
                ClauseText: clause.Text,
                MatchedRuleStandardLanguage: $"[rule:{flag.RuleId}]", // real impl would join the rule's StandardLanguage
                RuleGuideline: flag.Rationale
            ), ct);
            return Redline.Create(flag.Id, result.SuggestedText, result.Rationale, result.Citations, flag.Confidence);
        }).ToList();

        var redlines = (await Task.WhenAll(tasks)).Where(r => r is not null).Cast<Redline>().ToList();
        await _redlines.AddRangeAsync(redlines, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<GenerateRedlinesResponse>.Success(new GenerateRedlinesResponse(redlines.Count));
    }
}
