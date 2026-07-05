namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Anti-Corruption Layer (Constitution Principle II). Translates provider DTOs
/// from the AI service into our domain DTOs. Implementations live in
/// Infrastructure/AI and protect the Domain from changes in provider shapes.
/// </summary>
public interface IAntiCorruptionLayer<TExternal, TDomain>
{
    TDomain Translate(TExternal external);
}

public interface IAiProviderTranslator
{
    AiClauseAnalysisResult TranslateClauseAnalysis(object providerPayload);
    AiRedlineResult TranslateRedline(object providerPayload);
    AiObligationsResult TranslateObligations(object providerPayload);
    AiCompareResult TranslateCompare(object providerPayload);
}
