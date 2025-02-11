using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem;

class UniversalFileSystem : IUniversalFileSystem
{
    private readonly Dictionary<string /*scheme*/, IFileSystem> _impls = new();

    public UniversalFileSystem(IFileSystemCreator implCreator)
    {
        this.ImplCreator = implCreator;
    }

    private IFileSystemCreator ImplCreator { get; }

    private IFileSystem GetImpl(string scheme)
    {
        if (_impls.TryGetValue(scheme, out IFileSystem? impl))
            return impl;

        impl = this.ImplCreator.Create(scheme);
        _impls.Add(scheme, impl);
        return impl;
    }

    private IFileSystem GetImpl(Uri uri) => this.GetImpl(uri.Scheme);


    #region IUniversalFileSystem

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
    {
        return this.GetImpl(prefix).ListObjectsAsync(prefix, recursive, cancellationToken);
    }

    public Task<ObjectMetadata> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        return this.GetImpl(path).GetObjectMetadataAsync(path, cancellationToken);
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        return this.GetImpl(path).GetObjectAsync(path, cancellationToken);
    }

    public Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        return this.GetImpl(path).PutObjectAsync(path, stream, overwriteIfExists, cancellationToken);
    }

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        return this.GetImpl(path).DeleteObjectAsync(path, cancellationToken);
    }

    public async Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        IFileSystem impl1 = this.GetImpl(oldPath);
        IFileSystem impl2 = this.GetImpl(newPath);

        if (!ReferenceEquals(impl1, impl2))
        {
            await using Stream stream = await impl1.GetObjectAsync(oldPath, cancellationToken);
            await impl2.PutObjectAsync(newPath, stream, overwriteIfExists, cancellationToken);
            await impl2.DeleteObjectAsync(oldPath, cancellationToken);
        }
        else
        {
            await impl1.MoveObjectAsync(oldPath, newPath, overwriteIfExists, cancellationToken);
        }
    }

    public async Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        IFileSystem impl1 = this.GetImpl(sourcePath);
        IFileSystem impl2 = this.GetImpl(destPath);

        if (!ReferenceEquals(impl1, impl2))
        {
            await using Stream stream = await impl1.GetObjectAsync(sourcePath, cancellationToken);
            await impl2.PutObjectAsync(destPath, stream, overwriteIfExists, cancellationToken);
        }
        else
        {
            await impl1.CopyObjectAsync(sourcePath, destPath, overwriteIfExists, cancellationToken);
        }
    }

    #endregion

    #region IAsyncDisposable
    public async ValueTask DisposeAsync()
    {
        foreach (IFileSystem fileSystem in _impls.Values)
            await fileSystem.DisposeAsync();
        _impls.Clear();
    }
    #endregion
}