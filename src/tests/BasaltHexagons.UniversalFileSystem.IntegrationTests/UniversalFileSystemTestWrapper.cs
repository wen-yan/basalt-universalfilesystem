using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public class UniversalFileSystemTestWrapper : AsyncDisposable, IUniversalFileSystem
{
    public UniversalFileSystemTestWrapper(IHost host, Uri baseUri, IUniversalFileSystem inner)
    {
        this.Host = host;
        this.BaseUri = baseUri;
        this.Inner = inner;
    }

    private IHost Host { get; }
    private Uri BaseUri { get; }
    private IUniversalFileSystem Inner { get; }

    #region Helpers

    public Uri GetRelativeUri(Uri uri) => this.BaseUri.MakeRelativeUri(uri);
    public string GetRelativeUri(string uri) => this.GetRelativeUri(new Uri(uri)).ToString();
    public Uri GetFullUri(Uri relativeUri) => new(this.BaseUri, relativeUri);

    public ObjectMetadata MakeObjectMetadata(string uri, ObjectType objectType, long? contentSize, DateTime? lastModifiedTimeUtc)
    {
        return new(this.GetFullUri(new Uri(uri, UriKind.Relative)), objectType, contentSize, lastModifiedTimeUtc);
    }

    public ObjectMetadata MakeObjectMetadata(string uri, ObjectType objectType, long? contentSize)
        => this.MakeObjectMetadata(uri, objectType, contentSize, DateTime.UtcNow);


    public Task CopyFileAsync(string sourceUri, string destUri, bool overwrite, CancellationToken cancellationToken = default)
        => this.CopyFileAsync(new Uri(sourceUri, UriKind.RelativeOrAbsolute), new Uri(destUri, UriKind.RelativeOrAbsolute), overwrite, cancellationToken);

    public Task<bool> DeleteFileAsync(string uri, CancellationToken cancellationToken = default)
        => this.DeleteFileAsync(new Uri(uri, UriKind.RelativeOrAbsolute), cancellationToken);

    public async Task<string> GetFileAsync(string uri, CancellationToken cancellationToken = default)
    {
        await using Stream stream = await this.GetFileAsync(new Uri(uri, UriKind.RelativeOrAbsolute), cancellationToken);
        using TextReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public Task<ObjectMetadata?> GetFileMetadataAsync(string uri, CancellationToken cancellationToken = default)
        => this.GetFileMetadataAsync(new Uri(uri, UriKind.RelativeOrAbsolute), cancellationToken);


    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive, CancellationToken cancellationToken = default)
        => this.ListObjectsAsync(new Uri(prefix, UriKind.RelativeOrAbsolute), recursive, cancellationToken);


    public Task MoveFileAsync(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default)
        => this.MoveFileAsync(new Uri(oldPath, UriKind.RelativeOrAbsolute), new Uri(newPath, UriKind.RelativeOrAbsolute), overwrite, cancellationToken);


    public async Task PutFileAsync(string uri, string content, bool overwrite, CancellationToken cancellationToken = default)
    {
        await using MemoryStream stream = new();
        await using (TextWriter writer = new StreamWriter(stream, leaveOpen: true))
        {
            await writer.WriteAsync(content);
        }

        stream.Seek(0, SeekOrigin.Begin);
        await this.PutFileAsync(new Uri(uri, UriKind.RelativeOrAbsolute), stream, overwrite, cancellationToken);
    }

    public Task<bool> DoesFileExistAsync(string uri, CancellationToken cancellationToken = default)
        => this.DoesFileExistAsync(new Uri(uri, UriKind.Relative), cancellationToken);

    #endregion

    #region IUniversalFileSystem

    public Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
        => this.Inner.CopyFileAsync(this.GetFullUri(sourceUri), new Uri(this.BaseUri, destUri), overwrite, cancellationToken);

    public Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
        => this.Inner.DeleteFileAsync(this.GetFullUri(uri), cancellationToken);

    public Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
        => this.Inner.GetFileAsync(this.GetFullUri(uri), cancellationToken);

    public Task<ObjectMetadata?> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
        => this.Inner.GetFileMetadataAsync(this.GetFullUri(uri), cancellationToken);

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
        => this.Inner.ListObjectsAsync(this.GetFullUri(prefix), recursive, cancellationToken);

    public Task MoveFileAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken)
        => this.Inner.MoveFileAsync(this.GetFullUri(oldPath), new Uri(this.BaseUri, newPath), overwrite, cancellationToken);

    public Task PutFileAsync(Uri uri, Stream stream, bool overwrite, CancellationToken cancellationToken)
        => this.Inner.PutFileAsync(this.GetFullUri(uri), stream, overwrite, cancellationToken);

    public Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
        => this.Inner.DoesFileExistAsync(this.GetFullUri(uri), cancellationToken);

    #endregion

    protected override async ValueTask DisposeManagedObjectsAsync()
    {
        await this.Inner.DisposeAsync();
        this.Host.Dispose();
        await base.DisposeManagedObjectsAsync();
    }

    // make test case easy to read
    public override string ToString() => this.BaseUri.Scheme;
}