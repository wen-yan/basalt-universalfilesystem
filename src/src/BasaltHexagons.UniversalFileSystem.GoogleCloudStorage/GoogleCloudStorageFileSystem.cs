using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.GoogleCloudStorage;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class GoogleCloudStorageFileSystem : AsyncDisposable, IFileSystem
{
    public GoogleCloudStorageFileSystem(StorageClient client, IConfiguration settings)
    {
        this.Client = client;
        this.Settings = settings;
    }

    private StorageClient Client { get; }
    private IConfiguration Settings { get; }
    
    #region IFileSystem
    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    #endregion
}