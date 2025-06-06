using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core.Disposing;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
public abstract class AsyncDisposable : Disposable, IAsyncDisposable
{
    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        await DisposeManagedObjectsAsync();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    #endregion

    protected virtual ValueTask DisposeManagedObjectsAsync() => ValueTask.CompletedTask;
}
