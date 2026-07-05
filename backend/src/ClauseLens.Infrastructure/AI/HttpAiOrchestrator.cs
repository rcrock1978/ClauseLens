using System.Net.Http.Json;
using ClauseLens.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClauseLens.Infrastructure.AI;

/// <summary>
/// HTTP adapter that forwards analysis requests to the Python AI service.
/// Implements the Anti-Corruption Layer boundary: provider DTOs are
/// translated into our domain DTOs (Constitution Principle II).
/// </summary>
public sealed class HttpAiOrchestrator : IAiOrchestrator
{
    private readonly HttpClient _http;
    private readonly AiServiceOptions _options;
    private readonly ILogger<HttpAiOrchestrator> _logger;

    public HttpAiOrchestrator(HttpClient http, IOptions<AiServiceOptions> options, ILogger<HttpAiOrchestrator> logger)
    { _http = http; _options = options.Value; _logger = logger; }

    public async Task<AiClauseAnalysisResult> AnalyzeClauseAsync(AiClauseAnalysisRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{_options.BaseUrl}/analyze-clause", req, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<AiClauseAnalysisResult>(cancellationToken: ct))!;
    }

    public async Task<AiRedlineResult> GenerateRedlineAsync(AiRedlineRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{_options.BaseUrl}/generate-redline", req, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<AiRedlineResult>(cancellationToken: ct))!;
    }

    public async Task<AiObligationsResult> ExtractObligationsAsync(AiObligationsRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{_options.BaseUrl}/extract-obligations", req, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<AiObligationsResult>(cancellationToken: ct))!;
    }

    public async Task<AiCompareResult> CompareToStandardAsync(AiCompareRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync($"{_options.BaseUrl}/compare", req, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<AiCompareResult>(cancellationToken: ct))!;
    }
}

public sealed class AiServiceOptions
{
    public string BaseUrl { get; set; } = "http://ai-service:8000";
    public int TimeoutSeconds { get; set; } = 30;
}
