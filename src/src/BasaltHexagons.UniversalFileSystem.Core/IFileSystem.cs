using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Core;

public interface IFileSystem : IAsyncDisposable, IDisposable
{
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken);
    Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken);
    Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken);
    Task PutObjectAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken);
    Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken);
    Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken);
    Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken);
    Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken);
}