using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Exception for no matched filesystem matched from URI.
/// </summary>
public class NoMatchedFileSystemException : UniversalFileSystemException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="uri">Filesystem object URI.</param>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception.</param>
    public NoMatchedFileSystemException(Uri uri, string? message = null, Exception? inner = null)
        : base(message ?? $"No matched file system for {uri}.", inner)
    {
        this.Uri = uri;
    }

    /// <summary>
    /// Filesystem object URI.
    /// </summary>
    public Uri Uri { get; }
}