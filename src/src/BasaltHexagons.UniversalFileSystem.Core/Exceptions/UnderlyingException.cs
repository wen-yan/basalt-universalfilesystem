using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Exceptions;

public class UnderlyingException : UniversalFileSystemException
{
    public UnderlyingException(Exception? inner) : base("Underlying file system exception", inner)
    {
    }
}