using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Core;

public interface IFileSystem : IAsyncDisposable, IDisposable
{
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken);
    Task<ObjectMetadata?> GetFileMetadataAsync(Uri path, CancellationToken cancellationToken);
    Task<Stream> GetFileAsync(Uri path, CancellationToken cancellationToken);
    Task PutFileAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken);
    Task<bool> DeleteFileAsync(Uri path, CancellationToken cancellationToken);
    Task MoveFileAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken);
    Task CopyFileAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken);
    Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken);
}