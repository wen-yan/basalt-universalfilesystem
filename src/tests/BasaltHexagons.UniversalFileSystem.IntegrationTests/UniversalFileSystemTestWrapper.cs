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

    public Uri GetRelativePath(Uri path) => this.BaseUri.MakeRelativeUri(path);
    public string GetRelativePath(string path) => this.GetRelativePath(new Uri(path)).ToString();
    public Uri GetFullPath(Uri relativePath) => new(this.BaseUri, relativePath);

    public ObjectMetadata MakeObjectMetadata(string path, ObjectType objectType, long? contentSize, DateTime? lastModifiedTimeUtc)
    {
        return new(this.GetFullPath(new Uri(path, UriKind.Relative)), objectType, contentSize, lastModifiedTimeUtc);
    }

    public ObjectMetadata MakeObjectMetadata(string path, ObjectType objectType, long? contentSize)
        => this.MakeObjectMetadata(path, objectType, contentSize, DateTime.UtcNow);



    public Task CopyObjectAsync(string sourcePath, string destPath, bool overwriteIfExists, CancellationToken cancellationToken = default)
        => this.CopyObjectAsync(new Uri(sourcePath, UriKind.RelativeOrAbsolute), new Uri(destPath, UriKind.RelativeOrAbsolute), overwriteIfExists, cancellationToken);

    public Task<bool> DeleteObjectAsync(string path, CancellationToken cancellationToken = default)
        => this.DeleteObjectAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);

    public async Task<string> GetObjectAsync(string path, CancellationToken cancellationToken = default)
    {
        await using Stream stream = await this.GetObjectAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);
        using TextReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public Task<ObjectMetadata?> GetObjectMetadataAsync(string path, CancellationToken cancellationToken = default)
        => this.GetObjectMetadataAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);


    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive, CancellationToken cancellationToken = default)
        => this.ListObjectsAsync(new Uri(prefix, UriKind.RelativeOrAbsolute), recursive, cancellationToken);


    public Task MoveObjectAsync(string oldPath, string newPath, bool overwriteIfExists, CancellationToken cancellationToken = default)
        => this.MoveObjectAsync(new Uri(oldPath, UriKind.RelativeOrAbsolute), new Uri(newPath, UriKind.RelativeOrAbsolute), overwriteIfExists, cancellationToken);


    public async Task PutObjectAsync(string path, string content, bool overwriteIfExists, CancellationToken cancellationToken = default)
    {
        await using MemoryStream stream = new();
        await using (TextWriter writer = new StreamWriter(stream, leaveOpen: true))
        {
            await writer.WriteAsync(content);
        }

        stream.Seek(0, SeekOrigin.Begin);
        await this.PutObjectAsync(new Uri(path, UriKind.RelativeOrAbsolute), stream, overwriteIfExists, cancellationToken);
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        => this.ExistsAsync(new Uri(path, UriKind.Relative), cancellationToken);

    #endregion

    #region IUniversalFileSystem

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
        => this.Inner.CopyObjectAsync(this.GetFullPath(sourcePath), new Uri(this.BaseUri, destPath), overwriteIfExists, cancellationToken);

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.DeleteObjectAsync(this.GetFullPath(path), cancellationToken);

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.GetObjectAsync(this.GetFullPath(path), cancellationToken);

    public Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.GetObjectMetadataAsync(this.GetFullPath(path), cancellationToken);

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
        => this.Inner.ListObjectsAsync(this.GetFullPath(prefix), recursive, cancellationToken);

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
        => this.Inner.MoveObjectAsync(this.GetFullPath(oldPath), new Uri(this.BaseUri, newPath), overwriteIfExists, cancellationToken);

    public Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
        => this.Inner.PutObjectAsync(this.GetFullPath(path), stream, overwriteIfExists, cancellationToken);

    #endregion

    protected override async ValueTask AsyncDisposeManagedObjects()
    {
        await this.Inner.DisposeAsync();
        this.Host.Dispose();
        await base.AsyncDisposeManagedObjects();
    }

    // make test case easy to read
    public override string ToString() => this.BaseUri.Scheme;
}