using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;

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

    public Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
        => this.Wrap<Task>(() => this.FileSystem.CopyFileAsync(sourceUri, destUri, overwrite, cancellationToken));

    public Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
        => this.Wrap(() => this.FileSystem.DeleteFileAsync(uri, cancellationToken));

    public Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
        => this.Wrap(() => this.FileSystem.GetFileAsync(uri, cancellationToken));

    public Task<ObjectMetadata?> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
        => this.Wrap(() => this.FileSystem.GetFileMetadataAsync(uri, cancellationToken));

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
        => this.Wrap(() => this.FileSystem.ListObjectsAsync(prefix, recursive, cancellationToken));

    public Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken)
        => this.Wrap<Task>(() => this.FileSystem.MoveFileAsync(oldUri, newUri, overwrite, cancellationToken));

    public Task PutFileAsync(Uri uri, Stream stream, bool overwrite, CancellationToken cancellationToken)
        => this.Wrap<Task>(() => this.FileSystem.PutFileAsync(uri, stream, overwrite, cancellationToken));

    public Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
        => this.Wrap(() => this.FileSystem.DoesFileExistAsync(uri, cancellationToken));

    #endregion

    protected override async ValueTask DisposeManagedObjectsAsync()
    {
        await this.FileSystem.DisposeAsync();
        await base.DisposeManagedObjectsAsync();
    }

    private async Task Wrap<T>(Func<Task> func)
    {
        this.CheckIsDisposed();
        try
        {
            await func();
        }
        catch (Exception ex) when (ex is not UniversalFileSystemException)
        {
            throw new UnderlyingException(ex);
        }
    }

    private async Task<T> Wrap<T>(Func<Task<T>> func)
    {
        this.CheckIsDisposed();
        try
        {
            return await func();
        }
        catch (Exception ex) when (ex is not UniversalFileSystemException)
        {
            throw new UnderlyingException(ex);
        }
    }

    private IAsyncEnumerable<T> Wrap<T>(Func<IAsyncEnumerable<T>> func)
    {
        this.CheckIsDisposed();

        return func();
    }
}