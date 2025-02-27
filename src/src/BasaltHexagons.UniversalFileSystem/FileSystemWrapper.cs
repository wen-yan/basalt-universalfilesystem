using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem;

/// <summary>
/// Wrapper of IFileSystem
/// - Check if object is disposed
/// - Handle exceptions
/// </summary>
[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class FileSystemWrapper : AsyncDisposable, IFileSystem
{
    public FileSystemWrapper(IFileSystem fileSystem)
    {
        this.FileSystem = fileSystem;
    }

    private IFileSystem FileSystem { get; }

    #region IFileSystem

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.CopyObjectAsync(sourcePath, destPath, overwriteIfExists, cancellationToken);
    }

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.DeleteObjectAsync(path, cancellationToken);
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.GetObjectAsync(path, cancellationToken);
    }

    public Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.GetObjectMetadataAsync(path, cancellationToken);
    }

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.ListObjectsAsync(prefix, recursive, cancellationToken);
    }

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.MoveObjectAsync(oldPath, newPath, overwriteIfExists, cancellationToken);
    }

    public Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.PutObjectAsync(path, stream, overwriteIfExists, cancellationToken);
    }

    #endregion

    protected override async ValueTask AsyncDisposeManagedObjects()
    {
        await this.FileSystem.DisposeAsync();
        await base.AsyncDisposeManagedObjects();
    }
}
