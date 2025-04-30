using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public class UniversalFileSystemTestWrapper : AsyncDisposable
{
    public UniversalFileSystemTestWrapper(IUniversalFileSystem inner, UriWrapper uriWrapper)
    {
        this.Inner = inner;
        this.UriWrapper = uriWrapper;
    }

    private IUniversalFileSystem Inner { get; }
    private UriWrapper UriWrapper { get; }

    #region Helpers

    public Uri GetFullUri(string uri) => this.UriWrapper.Apply(uri);

    public ObjectMetadata MakeObjectMetadata(string uri, ObjectType objectType, long? contentSize, DateTime? lastModifiedTimeUtc)
    {
        return new(this.GetFullUri(uri), objectType, contentSize, lastModifiedTimeUtc);
    }

    public ObjectMetadata MakeObjectMetadata(string uri, ObjectType objectType, long? contentSize)
        => this.MakeObjectMetadata(uri, objectType, contentSize, DateTime.UtcNow);


    public Task CopyFileAsync(string sourceUri, string destUri, bool overwrite, CancellationToken cancellationToken = default)
        => this.Inner.CopyFileAsync(this.GetFullUri(sourceUri), this.GetFullUri(destUri), overwrite, cancellationToken);

    public Task<bool> DeleteFileAsync(string uri, CancellationToken cancellationToken = default)
        => this.Inner.DeleteFileAsync(this.GetFullUri(uri), cancellationToken);

    public async Task<string> GetFileAsync(string uri, CancellationToken cancellationToken = default)
    {
        await using Stream stream = await this.Inner.GetFileAsync(this.GetFullUri(uri), cancellationToken);
        using TextReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public Task<ObjectMetadata> GetFileMetadataAsync(string uri, CancellationToken cancellationToken = default)
        => this.Inner.GetFileMetadataAsync(this.GetFullUri(uri), cancellationToken);


    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive, CancellationToken cancellationToken = default)
        => this.Inner.ListObjectsAsync(this.GetFullUri(prefix), recursive, cancellationToken);


    public Task MoveFileAsync(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default)
        => this.Inner.MoveFileAsync(this.GetFullUri(oldPath), this.GetFullUri(newPath), overwrite, cancellationToken);


    public async Task PutFileAsync(string uri, string content, bool overwrite, CancellationToken cancellationToken = default)
    {
        await using MemoryStream stream = new();
        await using (TextWriter writer = new StreamWriter(stream, leaveOpen: true))
        {
            await writer.WriteAsync(content);
        }

        stream.Seek(0, SeekOrigin.Begin);
        await this.Inner.PutFileAsync(this.GetFullUri(uri), stream, overwrite, cancellationToken);
    }

    public Task<bool> DoesFileExistAsync(string uri, CancellationToken cancellationToken = default)
        => this.Inner.DoesFileExistAsync(this.GetFullUri(uri), cancellationToken);

    #endregion

    protected override async ValueTask DisposeManagedObjectsAsync()
    {
        await this.Inner.DisposeAsync();
        await base.DisposeManagedObjectsAsync();
    }

    // make test case easy to read
    // public override string ToString() => this.BaseUri.Scheme;
}