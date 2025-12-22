using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

public abstract class UniversalFileSystemException : Exception
{
    protected UniversalFileSystemException(string? message, Exception? inner = null) : base(message, inner)
    {
    }
}