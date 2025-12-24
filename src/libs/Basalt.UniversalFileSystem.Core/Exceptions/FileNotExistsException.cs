using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

/// <summary>
/// File doesn't exist exception.
/// </summary>
public class FileNotExistsException : FileException
{
    /// <inheritdoc />
    public FileNotExistsException(Uri fileUri, string? message = null, Exception? inner = null)
        : base(fileUri, message ?? $"File {fileUri} does not exist.", inner)
    {
    }
}