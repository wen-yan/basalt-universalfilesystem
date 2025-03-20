using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Exceptions;

public abstract class UniversalFileSystemException : Exception
{
    protected UniversalFileSystemException(string? message, Exception? inner = null) : base(message, inner)
    {
    }
}