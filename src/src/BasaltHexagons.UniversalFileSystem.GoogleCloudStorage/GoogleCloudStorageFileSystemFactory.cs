using System.Runtime.CompilerServices;
using BasaltHexagons.UniversalFileSystem.Core;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.GoogleCloudStorage;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class GoogleCloudStorageFileSystemFactory : IFileSystemFactory
{
    public IFileSystem Create(IConfiguration implementationConfiguration)
    {
        StorageClient client = StorageClient.CreateUnauthenticated();
        
        client.ListBuckets(null)
    }
}