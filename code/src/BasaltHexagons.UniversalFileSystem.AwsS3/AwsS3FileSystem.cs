using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem.AwsS3;

class AwsS3FileSystem : AsyncDisposable, IFileSystem
{
    public AwsS3FileSystem(IAmazonS3 client)
    {
        this.Client = client;
    }

    private IAmazonS3 Client { get; }


    #region IFileSystem

    public async Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        // TODO: overwriteIfExists
        CopyObjectRequest request = new CopyObjectRequest();
        (request.SourceBucket, request.SourceKey) = this.DeconstructUri(sourcePath);
        (request.DestinationBucket, request.DestinationKey) = this.DeconstructUri(destPath);

        await this.Client.CopyObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        DeleteObjectRequest request = new DeleteObjectRequest();
        (request.BucketName, request.Key) = this.DeconstructUri(path);

        DeleteObjectResponse response = await this.Client.DeleteObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        GetObjectRequest request = new GetObjectRequest();
        (request.BucketName, request.Key) = this.DeconstructUri(path);

        GetObjectResponse response = await this.Client.GetObjectAsync(request, cancellationToken);

        // TODO: how to dispose `response` object
        return response.ResponseStream;
    }

    public async Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        GetObjectMetadataRequest request = new();
        (request.BucketName, request.Key) = this.DeconstructUri(path);
        GetObjectMetadataResponse response = await this.Client.GetObjectMetadataAsync(request, cancellationToken);
        return new ObjectMetadata(path, ObjectType.File, response.ContentLength, response.LastModified);
    }

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // TODO: how to handle recursive?
        ListObjectsV2Request request = new ListObjectsV2Request();
        (request.BucketName, request.Prefix) = this.DeconstructUri(prefix);

        while (true)
        {
            ListObjectsV2Response response = await this.Client.ListObjectsV2Async(request, cancellationToken);

            foreach (S3Object obj in response.S3Objects)
            {
                Uri path = this.ConstructUir(prefix.Scheme, obj.BucketName, obj.Key);
                yield return new ObjectMetadata(path, ObjectType.File, obj.Size, obj.LastModified);
            }

            if (!response.IsTruncated)
                break;

            request.ContinuationToken = response.ContinuationToken;
        }
    }

    public async Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        await this.CopyObjectAsync(oldPath, newPath, overwriteIfExists, cancellationToken);
        await this.DeleteObjectAsync(oldPath, cancellationToken);
    }

    public async Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        // TODO: overwriteIfExists
        PutObjectRequest request = new PutObjectRequest() { InputStream = stream };
        (request.BucketName, request.Key) = this.DeconstructUri(path);
        PutObjectResponse response = await this.Client.PutObjectAsync(request, cancellationToken);
    }

    #endregion IFileSystem


    #region AsyncDisposable

    protected override ValueTask AsyncDisposeManagedObjects()
    {
        this.Client.Dispose();
        return base.AsyncDisposeManagedObjects();
    }

    #endregion AsyncDisposable

    private Uri ConstructUir(string scheme, string bucket, string key)
    {
        UriBuilder builder = new UriBuilder();
        builder.Scheme = scheme;
        builder.Host = bucket;
        builder.Path = key;
        return builder.Uri;
    }


    private (string Bucket, string Key) DeconstructUri(Uri uri)
    {
        string bucket = uri.Host;
        string key = uri.AbsolutePath.TrimStart('/');
        return (bucket, key);
    }
}