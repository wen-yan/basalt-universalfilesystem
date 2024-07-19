using System;
using System.Collections.Generic;
using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem;

public interface IUniversalFileSystem
{
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive);
}

public class UniversalFileSystem : IUniversalFileSystem
{
    private readonly Dictionary<string /*scheme*/, IFileSystemImpl> _impls = new();

    public UniversalFileSystem(IUniversalFileSystemImplFactory implFactory)
    {
        this.ImplFactory = implFactory;
    }

    private IUniversalFileSystemImplFactory ImplFactory { get; }

    private IFileSystemImpl GetImpl(string scheme)
    {
        if (_impls.TryGetValue(scheme, out IFileSystemImpl? impl))
            return impl;

        impl = this.ImplFactory.Create(scheme);
        _impls.Add(scheme, impl);
        return impl;
    }

    private IFileSystemImpl GetImplByPath(string path)
    {
        Uri uri = new(path);
        return this.GetImpl(uri.Scheme);
    }

    #region IUniversalFileSystem

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive)
    {
        IFileSystemImpl impl = this.GetImplByPath(prefix);
        return impl.ListObjectsAsync(prefix, recursive);
    }

    #endregion
}