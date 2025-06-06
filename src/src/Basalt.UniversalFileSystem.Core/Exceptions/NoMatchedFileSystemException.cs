using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

public class NoMatchedFileSystemException : UniversalFileSystemException
{
    public NoMatchedFileSystemException(Uri uri, string? message = null, Exception? inner = null)
        : base(message ?? $"No matched file system for {uri}.", inner)
    {
        this.Uri = uri;
    }

    public Uri Uri { get; }
}