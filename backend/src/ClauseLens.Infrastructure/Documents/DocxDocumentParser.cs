using ClauseLens.Application.Abstractions;
using DocumentFormat.OpenXml.Packaging;

namespace ClauseLens.Infrastructure.Documents;

/// <summary>
/// DOCX parser using Open XML. Detects encrypted packages (per FR-001a).
/// </summary>
public sealed class DocxDocumentParser : IDocumentParser
{
    public bool SupportsFormat(string fmt) => string.Equals(fmt, "docx", StringComparison.OrdinalIgnoreCase);

    public Task<ParsedDocument> ParseAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(stream, openSettings: new OpenSettings { AutoSave = false });
            if (doc.PackageProperties.ContentStatus == "encrypted")
                throw new PasswordProtectedDocumentException(fileName);

            var segments = new List<ParsedSegment>();
            var idx = 0;
            foreach (var para in doc.MainDocumentPart!.Document.Body!.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                var text = para.InnerText;
                if (string.IsNullOrWhiteSpace(text)) continue;
                var heading = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? string.Empty;
                segments.Add(new ParsedSegment(idx++, heading, text, false));
            }
            return Task.FromResult(new ParsedDocument(segments, false, pageCount: -1));
        }
        catch (PasswordProtectedDocumentException) { throw; }
        catch (Exception ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            throw new PasswordProtectedDocumentException(fileName);
        }
    }
}
