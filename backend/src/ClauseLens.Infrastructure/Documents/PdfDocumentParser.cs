using ClauseLens.Application.Abstractions;
using UglyToad.PdfPig;

namespace ClauseLens.Infrastructure.Documents;

/// <summary>
/// PDF parser using PdfPig. Detects password-protected PDFs and throws
/// <see cref="PasswordProtectedDocumentException"/> (per FR-001a).
/// </summary>
public sealed class PdfDocumentParser : IDocumentParser
{
    public bool SupportsFormat(string fmt) => string.Equals(fmt, "pdf", StringComparison.OrdinalIgnoreCase);

    public Task<ParsedDocument> ParseAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        // PdfPig does not natively expose password-protection. We sniff the
        // header for the /Encrypt entry as a best-effort signal.
        if (IsPasswordProtected(stream))
            throw new PasswordProtectedDocumentException(fileName);

        stream.Position = 0;
        using var doc = PdfDocument.Open(stream);
        var segments = new List<ParsedSegment>();
        var pageCount = doc.NumberOfPages;
        var nonTextual = false;
        var idx = 0;
        foreach (var page in doc.GetPages())
        {
            var text = page.Text ?? string.Empty;
            // Per FR-002a: detect images by checking for any XObject image
            // entries on the page resources. A real implementation would walk
            // the resource dictionary; for MVP we use a coarse check.
            if (page.Resources?.ExperimentalAccess?.ContainsKey("XObject") == true)
                nonTextual = true;
            if (string.IsNullOrWhiteSpace(text)) continue;
            segments.Add(new ParsedSegment(idx++, string.Empty, text, nonTextual));
        }
        return Task.FromResult(new ParsedDocument(segments, nonTextual, pageCount));
    }

    private static bool IsPasswordProtected(Stream s)
    {
        var start = s.Position;
        Span<byte> buf = stackalloc byte[2048];
        var read = s.Read(buf);
        s.Position = start;
        var text = System.Text.Encoding.ASCII.GetString(buf[..read]);
        return text.Contains("/Encrypt", StringComparison.Ordinal);
    }
}
