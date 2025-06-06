using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

public abstract class FileException : UniversalFileSystemException
{
    protected FileException(Uri fileUri, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        this.FileUri = fileUri;
    }

    public Uri FileUri { get; }
}