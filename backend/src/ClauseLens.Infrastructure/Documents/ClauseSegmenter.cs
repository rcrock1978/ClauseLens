using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;

namespace ClauseLens.Infrastructure.Documents;

/// <summary>
/// Heuristic clause segmenter. Groups text into clauses by detecting:
///   - Numbered headings (e.g. "1.", "1.1", "Article IV")
///   - ALL-CAPS short lines
///   - Paragraph breaks (blank lines)
///
/// Each emitted Clause carries a flag for non-textual content (per FR-002a).
/// </summary>
public sealed class ClauseSegmenter
{
    public IReadOnlyList<Clause> Segment(Guid contractId, ParsedDocument parsed, string heading = "")
    {
        var clauses = new List<Clause>();
        var idx = 0;
        var buffer = new System.Text.StringBuilder();
        string currentHeading = heading;

        foreach (var seg in parsed.Segments)
        {
            if (IsHeading(seg.Text))
            {
                if (buffer.Length > 0)
                {
                    clauses.Add(Clause.Create(contractId, idx++, currentHeading, buffer.ToString().Trim(),
                        containsNonTextualContent: seg.ContainsNonTextualContent));
                    buffer.Clear();
                }
                currentHeading = seg.Text.Trim();
            }
            else
            {
                if (buffer.Length > 0) buffer.Append(' ');
                buffer.Append(seg.Text.Trim());
            }
        }
        if (buffer.Length > 0)
            clauses.Add(Clause.Create(contractId, idx++, currentHeading, buffer.ToString().Trim(),
                containsNonTextualContent: parsed.ContainsNonTextualContent));

        return clauses;
    }

    private static readonly System.Text.RegularExpressions.Regex HeadingPattern =
        new(@"^(\d+(\.\d+)*\.|Article\s+[IVXLCDM]+|Section\s+\d+|Schedule\s+[A-Z])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private static bool IsHeading(string text)
    {
        var trimmed = text.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 200) return false;
        return HeadingPattern.IsMatch(trimmed) || (trimmed == trimmed.ToUpperInvariant() && trimmed.Length <= 80 && trimmed.Length >= 3);
    }
}
