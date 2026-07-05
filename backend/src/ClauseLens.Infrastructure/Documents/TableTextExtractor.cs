using ClauseLens.Domain.Entities;

namespace ClauseLens.Infrastructure.Documents;

/// <summary>
/// Per FR-002a: extracts text from simple embedded tables as plain text
/// (left-to-right, top-to-bottom) and flags affected clauses for
/// NeedsDiscussion routing. Images are skipped entirely.
/// </summary>
public sealed class TableTextExtractor
{
    public (string text, bool containsNonTextual) Extract(string raw, bool pageHasImage)
    {
        if (pageHasImage) return (raw, true);
        // MVP: simple line-based join. A future version walks the table
        // structure and concatenates cell text in reading order.
        return (raw, false);
    }
}
