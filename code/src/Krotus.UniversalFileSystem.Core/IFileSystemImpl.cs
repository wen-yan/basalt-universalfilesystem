
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Krotus.UniversalFileSystem.Core;

public interface IFileSystemImpl : IAsyncDisposable
{
    IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive);
    Task<ObjectMetadata> GetObjectMetadataAsync(string path);
    Task<Stream> GetObjectAsync(string path);
    Task PutObjectAsync(string path, Stream stream, bool overwriteIfExists);
    Task<bool> DeleteObjectAsync(string path);
    Task RenameObjectAsync(string oldPath, string newPath, bool overwriteIfExists);
    Task CopyObjectAsync(string sourcePath, string destPath, bool overwriteIfExists);
}