using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Disposing;

public abstract class Disposable : IDisposable
{
    ~Disposable() => this.Dispose(false);

    public bool IsDisposed { get; private set; } = false;

    #region IDisposable Members

    public void Dispose()
    {
        if (!this.IsDisposed)
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    #endregion

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Only handle managed objects here...
            this.DisposeManagedObjects();
        }

        // Handle unmanaged objects here...
        this.DisposeUnmanagedObjects();

        this.IsDisposed = true;
    }

    protected void CheckIsDisposed()
    {
        if (this.IsDisposed)
            throw new ObjectDisposedException("Object is disposed");
    }

    protected virtual void DisposeManagedObjects() { }
    protected virtual void DisposeUnmanagedObjects() { }
}
