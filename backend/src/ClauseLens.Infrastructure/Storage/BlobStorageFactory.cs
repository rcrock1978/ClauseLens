using Azure.Storage.Blobs;
using ClauseLens.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClauseLens.Infrastructure.Storage;

/// <summary>
/// Picks the right blob store per environment. LocalFileSystem in Development,
/// Azure Blob Storage in Production. Wired by <see cref="BlobStorageFactory"/>.
/// </summary>
public interface IBlobStorageFactory
{
    IBlobStorage Create();
}

public sealed class BlobStorageFactory : IBlobStorageFactory
{
    private readonly IConfiguration _cfg;
    private readonly IHostEnvironment _env;
    public BlobStorageFactory(IConfiguration cfg, IHostEnvironment env) { _cfg = cfg; _env = env; }

    public IBlobStorage Create()
    {
        if (_env.IsDevelopment())
            return new LocalFileSystemBlobStorage(_cfg["BlobStorage:LocalRoot"]);

        var conn = _cfg["BlobStorage:AzureConnectionString"]
                   ?? throw new InvalidOperationException("BlobStorage:AzureConnectionString is required outside Development.");
        return new AzureBlobStorage(conn);
    }
}

public sealed class AzureBlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _client;
    public AzureBlobStorage(string connectionString) => _client = new BlobServiceClient(connectionString);

    public async Task<string> UploadAsync(string container, string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var c = _client.GetBlobContainerClient(container);
        await c.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = c.GetBlobClient(fileName);
        await blob.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobUploadOptions { HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType } }, ct);
        return blob.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string uri, CancellationToken ct = default)
    {
        var blob = new BlobClient(new Uri(uri));
        var resp = await blob.DownloadStreamingAsync(cancellationToken: ct);
        return resp.Value.Content;
    }

    public async Task DeleteAsync(string uri, CancellationToken ct = default)
    {
        var blob = new BlobClient(new Uri(uri));
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
