using System;

namespace BasaltHexagons.UniversalFileSystem.Core.Exceptions;

public class FileExistsException : FileException
{
    public FileExistsException(Uri fileUri, string? message = null, Exception? inner = null)
        : base(fileUri, message ?? $"File {fileUri} already exists.", inner)
    {
    }
}