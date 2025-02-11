using System;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Core.Disposing;

public abstract class AsyncDisposable : Disposable, IAsyncDisposable
{
    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        await AsyncDisposeManagedObjects().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    #endregion

    protected virtual ValueTask AsyncDisposeManagedObjects() => ValueTask.CompletedTask;
}
