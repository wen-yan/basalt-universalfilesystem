using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Core;

/// <summary>
/// Filesystem interface. Supports basic filesystem operations.
/// </summary>
public interface IFileSystem : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// List objects from prefix.
    /// </summary>
    /// <param name="prefix">Prefix to start list objects.</param>
    /// <param name="recursive">Recursively list object from prefix and its sub-prefixes.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Enumerable of object metadata.</returns>
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get filesystem object metadata by its URI.
    /// </summary>
    /// <param name="uri">URI of object.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Object metadata.</returns>
    Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get file content stream.
    /// </summary>
    /// <param name="uri">URI of file.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>File content stream.</returns>
    Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken);

    /// <summary>
    /// Create or overwrite a file.
    /// </summary>
    /// <param name="uri">File URI.</param>
    /// <param name="content">File content stream.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if file already exists.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Operation task.</returns>
    Task PutFileAsync(Uri uri, Stream content, bool overwrite, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="uri">File URI.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Boolean value, true for file exists and deleted, false for file doesn't exist.</returns>
    Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken);
    
    /// <summary>
    /// Move/rename file.
    /// </summary>
    /// <param name="oldUri">Old file URI.</param>
    /// <param name="newUri">New file URI.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if target file already exists.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Operation tasks.</returns>
    Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken);
    
    /// <summary>
    /// Copy file.
    /// </summary>
    /// <param name="sourceUri">Source file URI.</param>
    /// <param name="destUri">Destination file URI.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if target file already exists.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Operation tasks.</returns>
    Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken);
    
    /// <summary>
    /// Check if file exists.
    /// </summary>
    /// <param name="uri">File URI.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>True for file exists, otherwise false.</returns>
    Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken);
}