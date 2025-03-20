using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Exceptions;

public class FileNotExistsException : FileException
{
    public FileNotExistsException(Uri fileUri, string? message = null, Exception? inner = null)
        : base(fileUri, message ?? $"File {fileUri} does not exist.", inner)
    {
    }
}