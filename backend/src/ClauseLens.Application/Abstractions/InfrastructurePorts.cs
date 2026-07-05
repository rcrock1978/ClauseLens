using ClauseLens.Domain.Entities;

namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Persistence abstraction over EF Core. Owned by Application; implemented in
/// Infrastructure (per Clean Architecture). Includes an Anti-Corruption Layer
/// wrapper (Constitution Principle II) for external-system reads.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Document parser port. Implemented in Infrastructure by PdfDocumentParser
/// and DocxDocumentParser. Detects password-protected files and throws
/// <see cref="PasswordProtectedDocumentException"/> per FR-001a.
/// </summary>
public interface IDocumentParser
{
    bool SupportsFormat(string fileFormat);
    Task<ParsedDocument> ParseAsync(Stream stream, string fileName, CancellationToken ct = default);
}

public sealed record ParsedDocument(
    IReadOnlyList<ParsedSegment> Segments,
    bool ContainsNonTextualContent,
    int PageCount
);

public sealed record ParsedSegment(int Index, string Heading, string Text, bool ContainsNonTextualContent);

public sealed class PasswordProtectedDocumentException : Exception
{
    public PasswordProtectedDocumentException(string fileName)
        : base($"Document '{fileName}' is password-protected and cannot be processed.")
    { }
}

/// <summary>
/// Blob storage port. Implemented by LocalFileSystemBlobStorage (dev) and
/// AzureBlobStorageAdapter (prod). Returns a URI that the Contract entity
/// stores for later retrieval.
/// </summary>
public interface IBlobStorage
{
    Task<string> UploadAsync(string container, string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string uri, CancellationToken ct = default);
    Task DeleteAsync(string uri, CancellationToken ct = default);
}
