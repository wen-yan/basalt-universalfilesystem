using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Exceptions;

namespace Basalt.UniversalFileSystem;

/// <summary>
/// Universal filesystem interface. This is the only entry to access filesystems.
/// </summary>
public interface IUniversalFileSystem : IAsyncDisposable
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

/// <summary>
/// IUniversalFileSystem extension methods (omit cancellation token)
/// </summary>
public static class UniversalFileSystemNoCancellationExtensions
{
    /// <summary>
    /// List objects from prefix.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="prefix">Prefix to start list objects.</param>
    /// <param name="recursive">Recursively list object from prefix and its sub-prefixes.</param>
    /// <returns>Enumerable of object metadata.</returns>
    public static IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(this IUniversalFileSystem ufs, Uri prefix, bool recursive)
        => ufs.ListObjectsAsync(prefix, recursive, CancellationToken.None);

    /// <summary>
    /// Get filesystem object metadata by its URI.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">URI of object.</param>
    /// <returns>Object metadata.</returns>
    public static Task<ObjectMetadata> GetFileMetadataAsync(this IUniversalFileSystem ufs, Uri uri)
        => ufs.GetFileMetadataAsync(uri, CancellationToken.None);

    /// <summary>
    /// Get file content stream.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">URI of file.</param>
    /// <returns>File content stream.</returns>
    public static Task<Stream> GetFileAsync(this IUniversalFileSystem ufs, Uri uri)
        => ufs.GetFileAsync(uri, CancellationToken.None);

    /// <summary>
    /// Create or overwrite a file.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">File URI.</param>
    /// <param name="content">File content stream.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if file already exists.</param>
    /// <returns>Operation task.</returns>
    public static Task PutFileAsync(this IUniversalFileSystem ufs, Uri uri, Stream content, bool overwrite)
        => ufs.PutFileAsync(uri, content, overwrite, CancellationToken.None);

    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">File URI.</param>
    /// <returns>Boolean value, true for file exists and deleted, false for file doesn't exist.</returns>
    public static Task<bool> DeleteFileAsync(this IUniversalFileSystem ufs, Uri uri)
        => ufs.DeleteFileAsync(uri, CancellationToken.None);

    /// <summary>
    /// Move/rename file.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="oldUri">Old file URI.</param>
    /// <param name="newUri">New file URI.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if target file already exists.</param>
    /// <returns>Operation tasks.</returns>
    public static Task MoveFileAsync(this IUniversalFileSystem ufs, Uri oldUri, Uri newUri, bool overwrite)
        => ufs.MoveFileAsync(oldUri, newUri, overwrite, CancellationToken.None);

    /// <summary>
    /// Copy file.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="sourceUri">Source file URI.</param>
    /// <param name="destUri">Destination file URI.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if target file already exists.</param>
    /// <returns>Operation tasks.</returns>
    public static Task CopyFileAsync(this IUniversalFileSystem ufs, Uri sourceUri, Uri destUri, bool overwrite)
        => ufs.CopyFileAsync(sourceUri, destUri, overwrite, CancellationToken.None);

    /// <summary>
    /// Check if file exists.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">File URI.</param>
    /// <returns>True for file exists, otherwise false.</returns>
    public static Task<bool> DoesFileExistAsync(this IUniversalFileSystem ufs, Uri uri)
        => ufs.DoesFileExistAsync(uri, CancellationToken.None);
}

/// <summary>
/// IUniversalFileSystem extension methods
/// </summary>
public static class UniversalFileSystemExtensions
{
    /// <summary>
    /// Get file content as string.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">URI of file.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>File content.</returns>
    public static async Task<string> GetFileStringAsync(this IUniversalFileSystem ufs, Uri uri, CancellationToken cancellationToken = default)
    {
        await using Stream stream = await ufs.GetFileAsync(uri, cancellationToken).ConfigureAwait(false);
        using TextReader reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Create or overwrite a file from string content.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="uri">File URI.</param>
    /// <param name="content">File content stream.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if file already exists.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Operation task.</returns>
    public static async Task PutFileAsync(this IUniversalFileSystem ufs, Uri uri, string content, bool overwrite, CancellationToken cancellationToken = default)
    {
        await using MemoryStream stream = new();
        await using (TextWriter writer = new StreamWriter(stream, leaveOpen: true))
        {
            await writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(false);
        }

        stream.Seek(0, SeekOrigin.Begin);
        await ufs.PutFileAsync(uri, stream, overwrite, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Copy files recursively.
    /// </summary>
    /// <param name="ufs">IUniversalFileSystem object.</param>
    /// <param name="sourceUri">Source file or prefix URI.</param>
    /// <param name="destUri">Destination file or prefix URI.</param>
    /// <param name="overwrite">Boolean value, true for overwrite if target file already exists.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Operation tasks.</returns>
    public static async Task CopyFilesRecursivelyAsync(this IUniversalFileSystem ufs, Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<ObjectMetadata> files = ufs.ListObjectsAsync(sourceUri, true, cancellationToken)
            .Where(x => x.ObjectType == ObjectType.File);

        await foreach (ObjectMetadata file in files.WithCancellation(cancellationToken))
        {
            Uri relativeUri = sourceUri.MakeRelativeUri(file.Uri);
            Uri targetUri = new(destUri, relativeUri);

            try
            {
                await ufs.CopyFileAsync(file.Uri, targetUri, overwrite, cancellationToken).ConfigureAwait(false);
            }
            catch (FileExistsException) when (!overwrite)
            {
                // Ignore this exception, as it's expected.
            }
        }
    }
}