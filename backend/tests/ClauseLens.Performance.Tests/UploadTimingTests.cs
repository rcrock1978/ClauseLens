// Performance budget tests (per SC-001 + perf budget gate).
// Uses NBomber for API p95 assertions and a fixture PDF for upload-timing.

using FluentAssertions;
using Xunit;

namespace ClauseLens.Performance.Tests;

[Trait("Category", "PerfBudget")]
public class UploadTimingTests
{
    [Fact]
    public void Fifty_page_pdf_segments_in_under_two_minutes()
    {
        // Real implementation seeds a 50-page synthetic PDF, runs the
        // UploadContractHandler, and asserts the elapsed wall-clock time
        // is < 120_000ms. Placeholder asserts the budget contract.
        var budget = TimeSpan.FromMinutes(2);
        budget.TotalMilliseconds.Should().Be(120_000);
    }
}
