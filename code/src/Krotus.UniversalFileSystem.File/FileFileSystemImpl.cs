using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Krotus.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace Krotus.UniversalFileSystem.File;

public class FileFileSystemImpl : IFileSystemImpl
{
    public FileFileSystemImpl(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }
    private IConfiguration Configuration { get; }
    
    #region MyRegion

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive)
    {
        throw new NotImplementedException();
    }

    public Task<ObjectMetadata> GetObjectMetadataAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> GetObjectAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task PutObjectAsync(string path, Stream stream, bool overwriteIfExists)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteObjectAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task RenameObjectAsync(string oldPath, string newPath, bool overwriteIfExists)
    {
        throw new NotImplementedException();
    }

    public Task CopyObjectAsync(string sourcePath, string destPath, bool overwriteIfExists)
    {
        throw new NotImplementedException();
    }

    #endregion

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}