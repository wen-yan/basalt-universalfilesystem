using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core.Disposing;

/// <summary>
/// Base implementation of IAsyncDisposable and IDisposable
/// </summary>
public abstract class AsyncDisposable : Disposable, IAsyncDisposable
{
    #region IAsyncDisposable

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeManagedObjectsAsync().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Dispose managed objects async.
    /// </summary>
    /// <returns>ValueTask object.</returns>
    protected virtual ValueTask DisposeManagedObjectsAsync() => ValueTask.CompletedTask;
}
