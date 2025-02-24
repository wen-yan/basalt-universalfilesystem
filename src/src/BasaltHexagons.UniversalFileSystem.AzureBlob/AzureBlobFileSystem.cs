using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem.AzureBlob;

public class AzureBlobFileSystem : AsyncDisposable, IFileSystem
{
    public AzureBlobFileSystem(BlobServiceClient client)
    {
        this.Client = client;
    }

    private BlobServiceClient Client { get; }

    #region IFileSystem

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        (string container, string key) = this.DeconstructUri(prefix);

        BlobContainerClient containerClient = this.Client.GetBlobContainerClient("ufs-test");

        if (!await containerClient.ExistsAsync(cancellationToken)) yield break;

        await foreach (BlobHierarchyItem blobHierarchyItem in containerClient.GetBlobsByHierarchyAsync(prefix: key, delimiter: "/", cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (blobHierarchyItem.IsPrefix)
            {
                Uri path = new(prefix, new Uri(blobHierarchyItem.Prefix, UriKind.Relative));
                yield return new ObjectMetadata(path, ObjectType.Prefix, null, null);
            }
            else
            {
                // Uri path = this.ConstructUir(prefix.Scheme, obj.BucketName, obj.Key);
                Uri path = new(prefix, new Uri(blobHierarchyItem.Blob.Name, UriKind.Relative));
                yield return new ObjectMetadata(path, ObjectType.File, blobHierarchyItem.Blob.Properties.ContentLength, blobHierarchyItem.Blob.Properties.LastModified?.UtcDateTime);
            }
        }
    }

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    #endregion

    private (string Container, string Key) DeconstructUri(Uri uri)
    {
        string bucket = uri.Host;
        string key = uri.AbsolutePath.TrimStart('/');
        return (bucket, key);
    }
}