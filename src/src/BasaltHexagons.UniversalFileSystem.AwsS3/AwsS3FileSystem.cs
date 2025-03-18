using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Configuration;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;
using BasaltHexagons.UniversalFileSystem.Core.IO;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem.AwsS3;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class AwsS3FileSystem : AsyncDisposable, IFileSystem
{
    public AwsS3FileSystem(IAmazonS3 client, IConfiguration settings)
    {
        this.Client = client;
        this.Settings = settings;
    }

    private IAmazonS3 Client { get; }
    private IConfiguration Settings { get; }


    #region IFileSystem

    public async Task CopyFileAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken)
    {
        if (!overwrite && await this.DoesFileExistAsync(destPath, cancellationToken))
        {
            throw new ArgumentException($"Object {destPath} already exists.");
        }

        CopyObjectRequest request = new();
        (request.SourceBucket, request.SourceKey) = DeconstructUri(sourcePath);
        (request.DestinationBucket, request.DestinationKey) = DeconstructUri(destPath);

        await this.TryCreateBucketIfNotExistsAsync(request.DestinationBucket, cancellationToken);
        await this.Client.CopyObjectAsync(request, cancellationToken);
    }

    public async Task<bool> DeleteFileAsync(Uri path, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(path, cancellationToken))
            return false;

        DeleteObjectRequest request = new();
        (request.BucketName, request.Key) = DeconstructUri(path);

        DeleteObjectResponse response = await this.Client.DeleteObjectAsync(request, cancellationToken);
        return true;
    }

    public async Task<Stream> GetFileAsync(Uri path, CancellationToken cancellationToken)
    {
        GetObjectRequest request = new();
        (request.BucketName, request.Key) = DeconstructUri(path);

        GetObjectResponse response = await this.Client.GetObjectAsync(request, cancellationToken);
        return new StreamWrapper(response.ResponseStream, [], [response]);
    }

    public async Task<ObjectMetadata?> GetFileMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        GetObjectMetadataRequest request = new();
        (request.BucketName, request.Key) = DeconstructUri(path);
        try
        {
            GetObjectMetadataResponse response = await this.Client.GetObjectMetadataAsync(request, cancellationToken);
            return new ObjectMetadata(path, ObjectType.File, response.ContentLength, response.LastModified.ToUniversalTime());
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        (string bucketName, string keyPrefix) = DeconstructUri(prefix);

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(this.Client, bucketName))
            yield break;

        Queue<string> keyPrefixQueue = new();
        keyPrefixQueue.Enqueue(keyPrefix);

        while (keyPrefixQueue.Count > 0)
        {
            ListObjectsV2Request request = new()
            {
                BucketName = bucketName,
                Prefix = keyPrefixQueue.Dequeue(),
                Delimiter = "/",
            };

            while (true)
            {
                ListObjectsV2Response response = await this.Client.ListObjectsV2Async(request, cancellationToken);

                foreach (S3Object obj in response.S3Objects)
                {
                    Uri path = ConstructUir(prefix.Scheme, obj.BucketName, obj.Key);
                    yield return new ObjectMetadata(path, ObjectType.File, obj.Size, obj.LastModified.ToUniversalTime());
                }

                foreach (string commonPrefix in response.CommonPrefixes)
                {
                    Uri path = ConstructUir(prefix.Scheme, bucketName, commonPrefix);
                    yield return new ObjectMetadata(path, ObjectType.Prefix, null, null);

                    if (recursive)
                    {
                        keyPrefixQueue.Enqueue(commonPrefix);
                    }
                }

                if (!response.IsTruncated)
                    break;

                request.ContinuationToken = response.ContinuationToken;
            }
        }
    }

    public async Task MoveFileAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken)
    {
        await this.CopyFileAsync(oldPath, newPath, overwrite, cancellationToken);
        await this.DeleteFileAsync(oldPath, cancellationToken);
    }

    public async Task PutFileAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        if (!overwrite && await this.DoesFileExistAsync(path, cancellationToken))
        {
            throw new ArgumentException($"Object {path} already exists.");
        }

        (string bucketName, string key) = DeconstructUri(path);
        await this.TryCreateBucketIfNotExistsAsync(bucketName, cancellationToken);

        PutObjectRequest request = new()
        {
            InputStream = stream,
            BucketName = bucketName,
            Key = key,
        };
        PutObjectResponse response = await this.Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken) => (await this.GetFileMetadataAsync(path, cancellationToken)) != null;

    #endregion IFileSystem


    #region AsyncDisposable

    protected override ValueTask DisposeManagedObjectsAsync()
    {
        this.Client.Dispose();
        return base.DisposeManagedObjectsAsync();
    }

    #endregion AsyncDisposable

    private static Uri ConstructUir(string scheme, string bucket, string key)
    {
        UriBuilder builder = new()
        {
            Scheme = scheme,
            Host = bucket,
            Path = key,
        };
        return builder.Uri;
    }

    private static (string Bucket, string Key) DeconstructUri(Uri uri)
    {
        string bucket = uri.Host;
        string key = uri.AbsolutePath.TrimStart('/');
        return (bucket, key);
    }

    private async Task TryCreateBucketIfNotExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        if (this.Settings.GetBoolValue("CreateBucketIfNotExists", () => null) ?? false)
        {
            await this.Client.EnsureBucketExistsAsync(bucketName);
        }
    }
}