namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Anti-Corruption Layer wrapping external AI/Search providers (Constitution
/// Principle II). Translates between provider DTOs and our domain DTOs.
/// </summary>
public interface IAntiCorruptionLayer<TExternal, TDomain>
{
    TDomain Translate(TExternal external);
}

public interface IAiOrchestrator
{
    Task<AiClauseAnalysisResult> AnalyzeClauseAsync(AiClauseAnalysisRequest request, CancellationToken ct = default);
    Task<AiRedlineResult> GenerateRedlineAsync(AiRedlineRequest request, CancellationToken ct = default);
    Task<AiObligationsResult> ExtractObligationsAsync(AiObligationsRequest request, CancellationToken ct = default);
    Task<AiCompareResult> CompareToStandardAsync(AiCompareRequest request, CancellationToken ct = default);
}

public sealed record AiClauseAnalysisRequest(
    string ClauseText,
    string ClauseType,
    IReadOnlyList<AiPlaybookRuleSummary> ApplicableRules
);

public sealed record AiPlaybookRuleSummary(Guid RuleId, string ClauseType, string Condition, string StandardLanguage, int Severity);

public sealed record AiClauseAnalysisResult(
    bool IsCompliant,
    Guid? MatchedRuleId,
    IReadOnlyList<Guid> MatchedRuleIds,
    int HighestSeverity,
    string Confidence,
    string Rationale
);

public sealed record AiRedlineRequest(Guid RiskFlagId, string ClauseText, string MatchedRuleStandardLanguage, string RuleGuideline);
public sealed record AiRedlineResult(string SuggestedText, string Rationale, string Citations);

public sealed record AiObligationsRequest(string ClauseText);
public sealed record AiObligationsResult(IReadOnlyList<AiObligation> Obligations);
public sealed record AiObligation(string Description, string ResponsibleParty, DateTime? DueDate, string? TriggerCondition);

public sealed record AiCompareRequest(string ClauseText, string StandardLanguage);
public sealed record AiCompareResult(IReadOnlyList<AiDiffSpan> Spans);
public sealed record AiDiffSpan(int Start, int Length, string Operation);
