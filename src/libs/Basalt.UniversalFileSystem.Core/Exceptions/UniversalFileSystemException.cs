using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Universal file system base exception.
/// </summary>
public abstract class UniversalFileSystemException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception object.</param>
    protected UniversalFileSystemException(string? message, Exception? inner = null) : base(message, inner)
    {
    }
}