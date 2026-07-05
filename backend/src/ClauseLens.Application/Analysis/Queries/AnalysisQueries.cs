using ClauseLens.Application.Abstractions;
using MediatR;

namespace ClauseLens.Application.Analysis.Queries;

public sealed record GetRisksQuery(Guid ContractId) : IQuery<Result<IReadOnlyList<RiskFlagDto>>>;

public sealed record RiskFlagDto(
    Guid Id, Guid ClauseId, Guid RuleId, IReadOnlyList<Guid> MatchedRuleIds,
    int Severity, string Confidence, string Rationale, DateTime CreatedAt,
    string RuleClauseType, string RuleCondition, string RuleStandardLanguage, string RuleGuideline
);

public sealed class GetRisksHandler : IRequestHandler<GetRisksQuery, Result<IReadOnlyList<RiskFlagDto>>>
{
    private readonly IRiskFlagRepository _riskFlags;
    public GetRisksHandler(IRiskFlagRepository riskFlags) => _riskFlags = riskFlags;

    public async Task<Result<IReadOnlyList<RiskFlagDto>>> Handle(GetRisksQuery q, CancellationToken ct)
    {
        var rows = await _riskFlags.ListByContractWithRulesAsync(q.ContractId, ct);
        return Result<IReadOnlyList<RiskFlagDto>>.Success(rows
            .Select(x => new RiskFlagDto(
                x.Item1.Id, x.Item1.ClauseId, x.Item1.RuleId, x.Item1.MatchedRuleIds,
                (int)x.Item1.Severity,
                x.Item1.Confidence.ToString().ToLowerInvariant(),
                x.Item1.Rationale, x.Item1.CreatedAt,
                x.Item2.ClauseType, x.Item2.Condition, x.Item2.StandardLanguage, x.Item2.Guideline))
            .ToList());
    }
}

public sealed record GetRedlinesQuery(Guid ContractId) : IQuery<Result<IReadOnlyList<RedlineDto>>>;

public sealed record RedlineDto(
    Guid Id, Guid RiskFlagId, string SuggestedText, string Rationale, string Citations,
    string Confidence, string Status
);

public sealed class GetRedlinesHandler : IRequestHandler<GetRedlinesQuery, Result<IReadOnlyList<RedlineDto>>>
{
    private readonly IRedlineRepository _redlines;
    public GetRedlinesHandler(IRedlineRepository redlines) => _redlines = redlines;

    public async Task<Result<IReadOnlyList<RedlineDto>>> Handle(GetRedlinesQuery q, CancellationToken ct)
    {
        var redlines = await _redlines.ListByContractAsync(q.ContractId, ct);
        return Result<IReadOnlyList<RedlineDto>>.Success(redlines
            .Select(r => new RedlineDto(r.Id, r.RiskFlagId, r.SuggestedText, r.Rationale, r.Citations, r.Confidence.ToString().ToLowerInvariant(), r.Status.ToString()))
            .ToList());
    }
}

public sealed record GetObligationsQuery(Guid ContractId) : IQuery<Result<IReadOnlyList<ObligationDto>>>;

public sealed record ObligationDto(Guid Id, string Description, string ResponsibleParty, DateTime? DueDate, string? TriggerCondition);

public sealed class GetObligationsHandler : IRequestHandler<GetObligationsQuery, Result<IReadOnlyList<ObligationDto>>>
{
    private readonly IObligationRepository _obligations;
    public GetObligationsHandler(IObligationRepository obligations) => _obligations = obligations;

    public async Task<Result<IReadOnlyList<ObligationDto>>> Handle(GetObligationsQuery q, CancellationToken ct)
    {
        var list = await _obligations.ListByContractAsync(q.ContractId, ct);
        return Result<IReadOnlyList<ObligationDto>>.Success(list
            .Select(o => new ObligationDto(o.Id, o.Description, o.ResponsibleParty, o.DueDate, o.TriggerCondition))
            .ToList());
    }
}

public sealed record GetClauseComparisonQuery(Guid ContractId, Guid ClauseId) : IQuery<Result<ComparisonDto>>;
public sealed record ComparisonDto(Guid ClauseId, string ClauseText, string StandardLanguage, IReadOnlyList<DiffSpanDto> Spans);
public sealed record DiffSpanDto(int Start, int Length, string Operation);

public sealed class GetClauseComparisonHandler : IRequestHandler<GetClauseComparisonQuery, Result<ComparisonDto>>
{
    private readonly IContractRepository _contracts;
    private readonly IAiOrchestrator _ai;

    public GetClauseComparisonHandler(IContractRepository contracts, IAiOrchestrator ai)
    { _contracts = contracts; _ai = ai; }

    public async Task<Result<ComparisonDto>> Handle(GetClauseComparisonQuery q, CancellationToken ct)
    {
        var contract = await _contracts.FindByIdAsync(q.ContractId, ct);
        if (contract is null) return Result<ComparisonDto>.Failure("Contract not found.", "NOT_FOUND");
        var clause = contract.Clauses.FirstOrDefault(c => c.Id == q.ClauseId);
        if (clause is null) return Result<ComparisonDto>.Failure("Clause not found.", "NOT_FOUND");
        // Real implementation would resolve the matching rule's StandardLanguage.
        // For MVP we call the AI compare endpoint with a placeholder.
        var ai = await _ai.CompareToStandardAsync(new AiCompareRequest(clause.Text, "[standard-language-placeholder]"), ct);
        return Result<ComparisonDto>.Success(new ComparisonDto(
            clause.Id, clause.Text, "[standard-language-placeholder]",
            ai.Spans.Select(s => new DiffSpanDto(s.Start, s.Length, s.Operation)).ToList()
        ));
    }
}
