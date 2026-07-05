using System.Text.Json;
using ClauseLens.Application.Abstractions;

namespace ClauseLens.Infrastructure.AI;

/// <summary>
/// Concrete ACL implementation. Reads raw provider JSON (or `object` payload)
/// and produces our domain DTOs. When the provider changes shape, only this
/// class changes — the Domain stays pure.
/// </summary>
public sealed class HttpAiProviderTranslator : IAiProviderTranslator
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public AiClauseAnalysisResult TranslateClauseAnalysis(object providerPayload)
        => JsonSerializer.Deserialize<AiClauseAnalysisResult>(JsonSerializer.Serialize(providerPayload), Json)
           ?? throw new InvalidOperationException("Provider returned null clause analysis payload");

    public AiRedlineResult TranslateRedline(object providerPayload)
        => JsonSerializer.Deserialize<AiRedlineResult>(JsonSerializer.Serialize(providerPayload), Json)
           ?? throw new InvalidOperationException("Provider returned null redline payload");

    public AiObligationsResult TranslateObligations(object providerPayload)
        => JsonSerializer.Deserialize<AiObligationsResult>(JsonSerializer.Serialize(providerPayload), Json)
           ?? throw new InvalidOperationException("Provider returned null obligations payload");

    public AiCompareResult TranslateCompare(object providerPayload)
        => JsonSerializer.Deserialize<AiCompareResult>(JsonSerializer.Serialize(providerPayload), Json)
           ?? throw new InvalidOperationException("Provider returned null compare payload");
}
