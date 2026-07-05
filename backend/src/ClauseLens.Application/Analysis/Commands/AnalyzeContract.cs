using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Application.Analysis.Commands;

/// <summary>
/// Per FR-003 + FR-004 + FR-006 + FR-009a + FR-012 + US3 + US5:
///   1. For every clause, call the AI service to score against published rules.
///   2. Aggregate matching rules (FR-009a) into a single RiskFlag with highest
///      severity and all matching rule IDs.
///   3. Apply confidence-based clause routing (FR-012): Low confidence →
///      NeedsDiscussion; mix with higher confidence → Flagged.
///   4. Persist the generated Redlines (FR-004) and Obligations (FR-006) so
///      the read endpoints return real data.
/// </summary>
public sealed record AnalyzeContractCommand(Guid ContractId) : ICommand<Result<AnalyzeContractResponse>>;
public sealed record AnalyzeContractResponse(int ClausesAnalyzed, int FlagsEmitted, int RedlinesEmitted, int ObligationsExtracted);

public sealed class AnalyzeContractHandler : IRequestHandler<AnalyzeContractCommand, Result<AnalyzeContractResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IContractRepository _contracts;
    private readonly IPlaybookRepository _playbooks;
    private readonly IRiskFlagRepository _riskFlags;
    private readonly IRedlineRepository _redlines;
    private readonly IObligationRepository _obligations;
    private readonly IAiOrchestrator _ai;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;
    private readonly ILogger<AnalyzeContractHandler> _logger;

    public AnalyzeContractHandler(IUnitOfWork uow, IContractRepository contracts, IPlaybookRepository playbooks,
                                   IRiskFlagRepository riskFlags, IRedlineRepository redlines, IObligationRepository obligations,
                                   IAiOrchestrator ai, ICurrentUser current, IAuditEventDispatcher audit,
                                   ILogger<AnalyzeContractHandler> logger)
    { _uow = uow; _contracts = contracts; _playbooks = playbooks; _riskFlags = riskFlags; _redlines = redlines; _obligations = obligations; _ai = ai; _current = current; _audit = audit; _logger = logger; }

    public async Task<Result<AnalyzeContractResponse>> Handle(AnalyzeContractCommand cmd, CancellationToken ct)
    {
        if (_current.TenantId is null) return Result<AnalyzeContractResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        var contract = await _contracts.FindByIdAsync(cmd.ContractId, ct);
        if (contract is null) return Result<AnalyzeContractResponse>.Failure("Contract not found.", "NOT_FOUND");

        var flags = new List<RiskFlag>();
        var flagByClause = new Dictionary<Guid, RiskFlag>();
        foreach (var clause in contract.Clauses)
        {
            var rules = await _playbooks.ListPublishedRulesForClauseTypeAsync(_current.TenantId.Value, InferClauseType(clause), ct);
            var ruleSummaries = rules.Select(r => new AiPlaybookRuleSummary(r.Id, r.ClauseType, r.Condition, r.StandardLanguage, (int)r.Severity)).ToList();
            var aiResult = await _ai.AnalyzeClauseAsync(new AiClauseAnalysisRequest(clause.Text, InferClauseType(clause), ruleSummaries), ct);
            if (aiResult.IsCompliant) { clause.MarkCompliant(); continue; }

            var matched = aiResult.MatchedRuleIds.Select(Guid.Parse).ToList();
            var primary = matched.First();
            var confidence = Enum.Parse<ConfidenceLevel>(aiResult.Confidence, ignoreCase: true);
            var severity = (RiskSeverity)aiResult.HighestSeverity;
            var flag = RiskFlag.Create(clause.Id, primary, matched, severity, confidence, aiResult.Rationale);
            clause.ApplyAutoRouting(new[] { flag });
            flags.Add(flag);
            flagByClause[clause.Id] = flag;
        }

        if (flags.Count > 0)
            await _riskFlags.AddRangeAsync(flags, ct);
        contract.MarkReadyForReview(DateTime.UtcNow);
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);

        // ── FR-004: redline generation (T176 + T184 — real StandardLanguage)
        var redlineEntities = new List<Redline>();
        var ruleLookup = (await Task.WhenAll(flagByClause.Values.Select(async f =>
            new { f.Id, Rule = await _playbooks.ListByTenantAsync(_current.TenantId.Value, ct) })))
            .SelectMany(x => x.Rule)
            .SelectMany(p => p.Rules)
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var flag in flags)
        {
            var clause = contract.Clauses.FirstOrDefault(c => c.Id == flag.ClauseId);
            if (clause is null) continue;
            var standard = ruleLookup.TryGetValue(flag.RuleId, out var rule) ? rule.StandardLanguage : $"[rule:{flag.RuleId}]";
            var guideline = ruleLookup.TryGetValue(flag.RuleId, out var r2) ? r2.Guideline : flag.Rationale;
            var aiRedline = await _ai.GenerateRedlineAsync(new AiRedlineRequest(flag.Id, clause.Text, standard, guideline), ct);
            redlineEntities.Add(Redline.Create(flag.Id, aiRedline.SuggestedText, aiRedline.Rationale, aiRedline.Citations, flag.Confidence));
        }
        if (redlineEntities.Count > 0)
            await _redlines.AddRangeAsync(redlineEntities, ct);

        // ── FR-006: obligation extraction
        var obligations = new List<Obligation>();
        foreach (var clause in contract.Clauses)
        {
            var ob = await _ai.ExtractObligationsAsync(new AiObligationsRequest(clause.Text), ct);
            foreach (var item in ob.Obligations)
                obligations.Add(Obligation.Create(clause.Id, item.Description, item.ResponsibleParty, item.DueDate, item.TriggerCondition));
        }
        if (obligations.Count > 0)
            await _obligations.AddRangeAsync(obligations, ct);

        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);

        return Result<AnalyzeContractResponse>.Success(new AnalyzeContractResponse(contract.Clauses.Count, flags.Count, redlineEntities.Count, obligations.Count));
    }

    private static string InferClauseType(Clause c) => string.IsNullOrEmpty(c.Heading) ? "general" : c.Heading.ToLowerInvariant();
}
