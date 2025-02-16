using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public class MethodTestsUniversalFileSystemWrapper : AsyncDisposable, IUniversalFileSystem
{
    public MethodTestsUniversalFileSystemWrapper(Uri baseUri, IUniversalFileSystem inner)
    {
        this.BaseUri = baseUri;
        this.Inner = inner;
    }

    private Uri BaseUri { get; }
    private IUniversalFileSystem Inner { get; }

    #region IUniversalFileSystem

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
        => this.Inner.CopyObjectAsync(new Uri(this.BaseUri, sourcePath), new Uri(this.BaseUri, destPath), overwriteIfExists, cancellationToken);

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.DeleteObjectAsync(new Uri(this.BaseUri, path), cancellationToken);

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.GetObjectAsync(new Uri(this.BaseUri, path), cancellationToken);

    public Task<ObjectMetadata> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
        => this.Inner.GetObjectMetadataAsync(new Uri(this.BaseUri, path), cancellationToken);

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
        => this.Inner.ListObjectsAsync(new Uri(this.BaseUri, prefix), recursive, cancellationToken);

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
        => this.Inner.MoveObjectAsync(new Uri(this.BaseUri, oldPath), new Uri(this.BaseUri, newPath), overwriteIfExists, cancellationToken);

    public Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
        => this.Inner.PutObjectAsync(new Uri(this.BaseUri, path), stream, overwriteIfExists, cancellationToken);

    #endregion

    protected override async ValueTask AsyncDisposeManagedObjects()
    {
        await this.Inner.DisposeAsync();
        await base.AsyncDisposeManagedObjects();
    }
}

static class UniversalFileSystemExtensions
{
    public static Task CopyObjectAsync(this IUniversalFileSystem ufs, string sourcePath, string destPath, bool overwriteIfExists, CancellationToken cancellationToken)
        => ufs.CopyObjectAsync(new Uri(sourcePath, UriKind.RelativeOrAbsolute), new Uri(destPath, UriKind.RelativeOrAbsolute), overwriteIfExists, cancellationToken);


    public static Task<bool> DeleteObjectAsync(this IUniversalFileSystem ufs, string path, CancellationToken cancellationToken)
        => ufs.DeleteObjectAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);


    public static Task<Stream> GetObjectAsync(this IUniversalFileSystem ufs, string path, CancellationToken cancellationToken)
        => ufs.GetObjectAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);


    public static Task<ObjectMetadata> GetObjectMetadataAsync(this IUniversalFileSystem ufs, string path, CancellationToken cancellationToken)
        => ufs.GetObjectMetadataAsync(new Uri(path, UriKind.RelativeOrAbsolute), cancellationToken);


    public static IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(this IUniversalFileSystem ufs, string prefix, bool recursive, CancellationToken cancellationToken)
        => ufs.ListObjectsAsync(new Uri(prefix, UriKind.RelativeOrAbsolute), recursive, cancellationToken);


    public static Task MoveObjectAsync(this IUniversalFileSystem ufs, string oldPath, string newPath, bool overwriteIfExists, CancellationToken cancellationToken)
        => ufs.MoveObjectAsync(new Uri(oldPath, UriKind.RelativeOrAbsolute), new Uri(newPath, UriKind.RelativeOrAbsolute), overwriteIfExists, cancellationToken);


    public static Task PutObjectAsync(this IUniversalFileSystem ufs, string path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
        => ufs.PutObjectAsync(new Uri(path, UriKind.RelativeOrAbsolute), stream, overwriteIfExists, cancellationToken);

}
