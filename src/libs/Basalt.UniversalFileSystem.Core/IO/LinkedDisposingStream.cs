using System;
using System.IO;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core.IO;

/// <summary>
/// DelegatedStream implementation which dispose linked items when disposing.
/// </summary>
public class LinkedDisposingStream : DelegatedStream
{
    private readonly IAsyncDisposable[] _asyncDisposables;
    private readonly IDisposable[] _disposables;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="inner">Inner Stream object.</param>
    /// <param name="asyncDisposables">Linked items supporting IAsyncDisposable.</param>
    /// <param name="disposables">Linked items support IDisposable</param>
    public LinkedDisposingStream(Stream inner, IAsyncDisposable[] asyncDisposables, IDisposable[] disposables) : base(inner)
    {
        _asyncDisposables = asyncDisposables;
        _disposables = disposables;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (IDisposable disposable in _disposables)
            disposable.Dispose();
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        foreach (IAsyncDisposable disposable in _asyncDisposables)
            await disposable.DisposeAsync().ConfigureAwait(false);
    }
}