using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Core;

public interface IFileSystem : IAsyncDisposable, IDisposable
{
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken);
    Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken);
    Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken);
    Task PutFileAsync(Uri uri, Stream stream, bool overwrite, CancellationToken cancellationToken);
    Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken);
    Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken);
    Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken);
    Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken);
}