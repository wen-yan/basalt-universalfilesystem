using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class UniversalFileSystem : AsyncDisposable, IUniversalFileSystem
{
    private readonly ConcurrentDictionary<string /*scheme*/, IFileSystem> _impls = new();

    public UniversalFileSystem(IFileSystemCreator implCreator)
    {
        this.ImplCreator = implCreator;
    }

    private IFileSystemCreator ImplCreator { get; }

    private IFileSystem GetImpl(string scheme) => _impls.GetOrAdd(scheme, _ => ImplCreator.Create(scheme));

    private IFileSystem GetImpl(Uri uri) => this.GetImpl(uri.Scheme);


    #region IUniversalFileSystem

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(prefix).ListObjectsAsync(prefix, recursive, cancellationToken);
    }

    public Task<ObjectMetadata?> GetFileMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(path).GetFileMetadataAsync(path, cancellationToken);
    }

    public Task<Stream> GetFileAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(path).GetFileAsync(path, cancellationToken);
    }

    public Task PutFileAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(path).PutFileAsync(path, stream, overwrite, cancellationToken);
    }

    public Task<bool> DeleteFileAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(path).DeleteFileAsync(path, cancellationToken);
    }

    public async Task MoveFileAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        IFileSystem impl1 = this.GetImpl(oldPath);
        IFileSystem impl2 = this.GetImpl(newPath);

        if (!ReferenceEquals(impl1, impl2))
        {
            await using Stream stream = await impl1.GetFileAsync(oldPath, cancellationToken);
            await impl2.PutFileAsync(newPath, stream, overwrite, cancellationToken);
            await impl2.DeleteFileAsync(oldPath, cancellationToken);
        }
        else
        {
            await impl1.MoveFileAsync(oldPath, newPath, overwrite, cancellationToken);
        }
    }

    public async Task CopyFileAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        IFileSystem impl1 = this.GetImpl(sourcePath);
        IFileSystem impl2 = this.GetImpl(destPath);

        if (!ReferenceEquals(impl1, impl2))
        {
            await using Stream stream = await impl1.GetFileAsync(sourcePath, cancellationToken);
            await impl2.PutFileAsync(destPath, stream, overwrite, cancellationToken);
        }
        else
        {
            await impl1.CopyFileAsync(sourcePath, destPath, overwrite, cancellationToken);
        }
    }

    public Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(path).DoesFileExistAsync(path, cancellationToken);
    }

    #endregion

    #region AsyncDisposable

    protected override async ValueTask DisposeManagedObjectsAsync()
    {
        foreach (IFileSystem fileSystem in _impls.Values)
            await fileSystem.DisposeAsync();
        _impls.Clear();
        await base.DisposeManagedObjectsAsync();
    }

    #endregion
}