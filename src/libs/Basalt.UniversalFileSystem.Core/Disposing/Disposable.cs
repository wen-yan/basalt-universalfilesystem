using System;

namespace Basalt.UniversalFileSystem.Core.Disposing;

/// <summary>
/// Base implementation of IDisposable
/// </summary>
public abstract class Disposable : IDisposable
{
    /// <summary>
    /// Destructor.
    /// </summary>
    ~Disposable() => this.Dispose(false);

    /// <summary>
    /// If object is already disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    #region IDisposable Members

    /// <inheritdoc />
    public void Dispose()
    {
        if (!this.IsDisposed)
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    #endregion

    /// <summary>
    /// Dispose object.
    /// </summary>
    /// <param name="disposing">True for disposing, otherwise destructor.</param>
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

    /// <summary>
    /// Check if object is already disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when object is already disposed.</exception>
    protected void CheckIsDisposed()
    {
        if (this.IsDisposed)
            throw new ObjectDisposedException("Object is disposed");
    }

    /// <summary>
    /// Dispose managed objects.
    /// </summary>
    protected virtual void DisposeManagedObjects() { }
    
    /// <summary>
    /// Dispose unmanaged objects.
    /// </summary>
    protected virtual void DisposeUnmanagedObjects() { }
}
