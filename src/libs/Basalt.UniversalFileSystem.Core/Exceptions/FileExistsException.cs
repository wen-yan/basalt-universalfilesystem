using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// Exception to file already exists.
/// </summary>
public class FileExistsException : FileException
{
    /// <inheritdoc />
    public FileExistsException(Uri fileUri, string? message = null, Exception? inner = null)
        : base(fileUri, message ?? $"File {fileUri} already exists.", inner)
    {
    }
}