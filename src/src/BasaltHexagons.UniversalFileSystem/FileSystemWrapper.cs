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

    public Task CopyFileAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.CopyFileAsync(sourcePath, destPath, overwrite, cancellationToken);
    }

    public Task<bool> DeleteFileAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.DeleteFileAsync(path, cancellationToken);
    }

    public Task<Stream> GetFileAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.GetFileAsync(path, cancellationToken);
    }

    public Task<ObjectMetadata?> GetFileMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.GetFileMetadataAsync(path, cancellationToken);
    }

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.ListObjectsAsync(prefix, recursive, cancellationToken);
    }

    public Task MoveFileAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.MoveFileAsync(oldPath, newPath, overwrite, cancellationToken);
    }

    public Task PutFileAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.PutFileAsync(path, stream, overwrite, cancellationToken);
    }

    public Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.FileSystem.DoesFileExistAsync(path, cancellationToken);
    }

    #endregion

    protected override async ValueTask DisposeManagedObjectsAsync()
    {
        await this.FileSystem.DisposeAsync();
        await base.DisposeManagedObjectsAsync();
    }
}
