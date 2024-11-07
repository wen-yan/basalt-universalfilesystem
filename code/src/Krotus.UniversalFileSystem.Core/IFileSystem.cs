using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Krotus.UniversalFileSystem.Core;

public interface IFileSystem : IAsyncDisposable
{
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive, CancellationToken cancellationToken);
    Task<ObjectMetadata> GetObjectMetadataAsync(string path, CancellationToken cancellationToken);
    Task<Stream> GetObjectAsync(string path, CancellationToken cancellationToken);
    Task PutObjectAsync(string path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken);
    Task<bool> DeleteObjectAsync(string path, CancellationToken cancellationToken);
    Task RenameObjectAsync(string oldPath, string newPath, bool overwriteIfExists, CancellationToken cancellationToken);
    Task CopyObjectAsync(string sourcePath, string destPath, bool overwriteIfExists, CancellationToken cancellationToken);
}