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
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Disposing;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Basalt.UniversalFileSystem.Core.IO;
using Microsoft.Extensions.Configuration;

namespace Basalt.UniversalFileSystem.AwsS3;

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

    public async Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
    {
        if (sourceUri == destUri)
            throw new ArgumentException("Can't copy file to itself.");

        if (!await this.DoesFileExistAsync(sourceUri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(sourceUri);

        if (!overwrite && await this.DoesFileExistAsync(destUri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(destUri);

        CopyObjectRequest request = new();
        (request.SourceBucket, request.SourceKey) = DeconstructUri(sourceUri);
        (request.DestinationBucket, request.DestinationKey) = DeconstructUri(destUri);

        await this.TryCreateBucketIfNotExistsAsync(request.DestinationBucket, cancellationToken).ConfigureAwait(false);
        await this.Client.CopyObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            return false;

        DeleteObjectRequest request = new();
        (request.BucketName, request.Key) = DeconstructUri(uri);

        DeleteObjectResponse response = await this.Client.DeleteObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(uri);

        GetObjectRequest request = new();
        (request.BucketName, request.Key) = DeconstructUri(uri);

        GetObjectResponse response = await this.Client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return new LinkedDisposingStream(
            new PositionSupportedStream(response.ResponseStream, 0, null),
            [], [response]);
    }

    public async Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
    {
        GetObjectMetadataRequest request = new();
        (request.BucketName, request.Key) = DeconstructUri(uri);
        try
        {
            GetObjectMetadataResponse response = await this.Client.GetObjectMetadataAsync(request, cancellationToken).ConfigureAwait(false);
            return new ObjectMetadata(uri, ObjectType.File, response.ContentLength,
                response.LastModified.ToUniversalTime());
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotExistsException(uri, inner: ex);
        }
    }

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        (string bucketName, string keyPrefix) = DeconstructUri(prefix);

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(this.Client, bucketName).ConfigureAwait(false))
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
                ListObjectsV2Response response = await this.Client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

                foreach (S3Object obj in response.S3Objects)
                {
                    Uri uri = ConstructUir(prefix.Scheme, obj.BucketName, obj.Key);
                    yield return new ObjectMetadata(uri, ObjectType.File, obj.Size, obj.LastModified.ToUniversalTime());
                }

                foreach (string commonPrefix in response.CommonPrefixes)
                {
                    Uri uri = ConstructUir(prefix.Scheme, bucketName, commonPrefix);
                    yield return new ObjectMetadata(uri, ObjectType.Prefix, null, null);

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

    public async Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken)
    {
        await this.CopyFileAsync(oldUri, newUri, overwrite, cancellationToken).ConfigureAwait(false);
        await this.DeleteFileAsync(oldUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task PutFileAsync(Uri uri, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        if (!overwrite && await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(uri);

        (string bucketName, string key) = DeconstructUri(uri);
        await this.TryCreateBucketIfNotExistsAsync(bucketName, cancellationToken).ConfigureAwait(false);

        PutObjectRequest request = new()
        {
            InputStream = stream,
            BucketName = bucketName,
            Key = key,
        };
        PutObjectResponse response = await this.Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
    {
        (string bucket, _) = DeconstructUri(uri);

        bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(this.Client, bucket).ConfigureAwait(false);
        if (!bucketExists) return false;

        try
        {
            await this.GetFileMetadataAsync(uri, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (FileNotExistsException)
        {
            return false;
        }
    }

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
            await this.Client.EnsureBucketExistsAsync(bucketName).ConfigureAwait(false);
        }
    }
}