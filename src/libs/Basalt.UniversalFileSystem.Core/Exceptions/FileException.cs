using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Base exception class of all file exceptions.
/// </summary>
public abstract class FileException : UniversalFileSystemException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fileUri">File URI.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception object.</param>
    protected FileException(Uri fileUri, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        this.FileUri = fileUri;
    }

    /// <summary>
    /// File URI.
    /// </summary>
    public Uri FileUri { get; }
}