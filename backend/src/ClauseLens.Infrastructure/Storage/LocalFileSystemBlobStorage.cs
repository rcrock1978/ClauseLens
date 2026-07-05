using ClauseLens.Application.Abstractions;

namespace ClauseLens.Infrastructure.Storage;

/// <summary>
/// Local filesystem blob storage for dev. Stores under
/// <c>{root}/{container}/{fileName}</c> and returns a file:// URI.
/// Production swaps this for an Azure Blob adapter.
/// </summary>
public sealed class LocalFileSystemBlobStorage : IBlobStorage
{
    private readonly string _root;
    public LocalFileSystemBlobStorage(string root) => _root = root;
    public LocalFileSystemBlobStorage() : this(Path.Combine(AppContext.BaseDirectory, "blobs")) { }

    public async Task<string> UploadAsync(string container, string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var dir = Path.Combine(_root, container);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, fileName);
        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, ct);
        return new Uri(path).AbsoluteUri;
    }

    public Task<Stream> DownloadAsync(string uri, CancellationToken ct = default)
        => Task.FromResult<Stream>(File.OpenRead(new Uri(uri).LocalPath));

    public Task DeleteAsync(string uri, CancellationToken ct = default)
    {
        File.Delete(new Uri(uri).LocalPath);
        return Task.CompletedTask;
    }
}
