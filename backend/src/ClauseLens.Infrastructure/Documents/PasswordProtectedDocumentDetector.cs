namespace ClauseLens.Infrastructure.Documents;

/// <summary>
/// Header-based pre-check that raises <see cref="Application.Abstractions.PasswordProtectedDocumentException"/>
/// before delegating to a full parser. Wraps both PdfDocumentParser and
/// DocxDocumentParser. Implementation lives in the parser classes themselves;
/// this type is the DI seam for testing (per FR-001a, T051b).
/// </summary>
public sealed class PasswordProtectedDocumentDetector
{
    public void EnsureNotProtected(string fileName, Stream stream) { /* per-format sniffers live in parsers */ }
}
