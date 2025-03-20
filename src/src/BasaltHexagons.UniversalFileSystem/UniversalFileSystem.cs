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

    public Task<ObjectMetadata?> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(uri).GetFileMetadataAsync(uri, cancellationToken);
    }

    public Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(uri).GetFileAsync(uri, cancellationToken);
    }

    public Task PutFileAsync(Uri uri, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(uri).PutFileAsync(uri, stream, overwrite, cancellationToken);
    }

    public Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(uri).DeleteFileAsync(uri, cancellationToken);
    }

    public async Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        IFileSystem impl1 = this.GetImpl(oldUri);
        IFileSystem impl2 = this.GetImpl(newUri);

        if (!ReferenceEquals(impl1, impl2))
        {
            await using Stream stream = await impl1.GetFileAsync(oldUri, cancellationToken);
            await impl2.PutFileAsync(newUri, stream, overwrite, cancellationToken);
            await impl2.DeleteFileAsync(oldUri, cancellationToken);
        }
        else
        {
            await impl1.MoveFileAsync(oldUri, newUri, overwrite, cancellationToken);
        }
    }

    public async Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        IFileSystem impl1 = this.GetImpl(sourceUri);
        IFileSystem impl2 = this.GetImpl(destUri);

        if (!ReferenceEquals(impl1, impl2))
        {
            await using Stream stream = await impl1.GetFileAsync(sourceUri, cancellationToken);
            await impl2.PutFileAsync(destUri, stream, overwrite, cancellationToken);
        }
        else
        {
            await impl1.CopyFileAsync(sourceUri, destUri, overwrite, cancellationToken);
        }
    }

    public Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
    {
        this.CheckIsDisposed();
        return this.GetImpl(uri).DoesFileExistAsync(uri, cancellationToken);
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