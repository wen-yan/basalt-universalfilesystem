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


    public Task CopyFileAsync(string sourcePath, string destPath, bool overwrite, CancellationToken cancellationToken = default)
        => this.CopyFileAsync(new Uri(sourcePath, UriKind.RelativeOrAbsolute), new Uri(destPath, UriKind.RelativeOrAbsolute), overwrite, cancellationToken);

    public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        => this.DeleteFileAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);

    public async Task<string> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await using Stream stream = await this.GetFileAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);
        using TextReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public Task<ObjectMetadata?> GetFileMetadataAsync(string path, CancellationToken cancellationToken = default)
        => this.GetFileMetadataAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);


    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive, CancellationToken cancellationToken = default)
        => this.ListObjectsAsync(new Uri(prefix, UriKind.RelativeOrAbsolute), recursive, cancellationToken);


    public Task MoveFileAsync(string oldPath, string newPath, bool overwrite, CancellationToken cancellationToken = default)
        => this.MoveFileAsync(new Uri(oldPath, UriKind.RelativeOrAbsolute), new Uri(newPath, UriKind.RelativeOrAbsolute), overwrite, cancellationToken);


    public async Task PutFileAsync(string path, string content, bool overwrite, CancellationToken cancellationToken = default)
    {
        await using MemoryStream stream = new();
        await using (TextWriter writer = new StreamWriter(stream, leaveOpen: true))
        {
            await writer.WriteAsync(content);
        }

        stream.Seek(0, SeekOrigin.Begin);
        await this.PutFileAsync(new Uri(path, UriKind.RelativeOrAbsolute), stream, overwrite, cancellationToken);
    }

    public Task<bool> DoesFileExistAsync(string path, CancellationToken cancellationToken = default)
        => this.DoesFileExistAsync(new Uri(path, UriKind.Relative), cancellationToken);

    #endregion

    #region IUniversalFileSystem

    public Task CopyFileAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken)
        => this.Inner.CopyFileAsync(this.GetFullPath(sourcePath), new Uri(this.BaseUri, destPath), overwrite, cancellationToken);

    public Task<bool> DeleteFileAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.DeleteFileAsync(this.GetFullPath(path), cancellationToken);

    public Task<Stream> GetFileAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.GetFileAsync(this.GetFullPath(path), cancellationToken);

    public Task<ObjectMetadata?> GetFileMetadataAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.GetFileMetadataAsync(this.GetFullPath(path), cancellationToken);

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
        => this.Inner.ListObjectsAsync(this.GetFullPath(prefix), recursive, cancellationToken);

    public Task MoveFileAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken)
        => this.Inner.MoveFileAsync(this.GetFullPath(oldPath), new Uri(this.BaseUri, newPath), overwrite, cancellationToken);

    public Task PutFileAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken)
        => this.Inner.PutFileAsync(this.GetFullPath(path), stream, overwrite, cancellationToken);

    public Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.DoesFileExistAsync(this.GetFullPath(path), cancellationToken);

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