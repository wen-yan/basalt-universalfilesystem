using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Core.Disposing;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
public abstract class AsyncDisposable : Disposable, IAsyncDisposable
{
    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        await AsyncDisposeManagedObjects();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    #endregion

    protected virtual ValueTask AsyncDisposeManagedObjects() => ValueTask.CompletedTask;
}
