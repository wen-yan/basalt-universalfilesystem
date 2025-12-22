using System;
using System.IO;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core.IO;

public class LinkedDisposingStream : DelegatedStream
{
    private IAsyncDisposable[] _asyncDisposables;
    private IDisposable[] _disposables;

    public LinkedDisposingStream(Stream inner, IAsyncDisposable[] asyncDisposables, IDisposable[] disposables) : base(inner)
    {
        _asyncDisposables = asyncDisposables;
        _disposables = disposables;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (IDisposable disposable in _disposables)
            disposable.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        foreach (IAsyncDisposable disposable in _asyncDisposables)
            await disposable.DisposeAsync();
    }
}