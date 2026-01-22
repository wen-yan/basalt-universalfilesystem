using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Disposing;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Basalt.UniversalFileSystem.GoogleCloudStorage;

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

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        (string bucketName, string keyPrefix) = DeconstructUri(prefix);

        if (!await this.DoesBucketExistAsync(bucketName).ConfigureAwait(false))
            yield break;

        ListObjectsOptions options = new()
        {
            Delimiter = "/",
            IncludeFoldersAsPrefixes = true,
            IncludeTrailingDelimiter = true,
        };

        Queue<string> keyPrefixQueue = new();
        keyPrefixQueue.Enqueue(keyPrefix);

        while (keyPrefixQueue.Count > 0)
        {
            string currentKeyPrefix = keyPrefixQueue.Dequeue();
            await foreach (Objects objs in this.Client.ListObjectsAsync(bucketName, currentKeyPrefix, options).AsRawResponses().WithCancellation(cancellationToken))
            {
                foreach (Object obj in objs.Items ?? Enumerable.Empty<Object>())
                {
                    yield return GetMetadataFromObject(prefix, obj);
                }

                foreach (var pre in objs.Prefixes ?? Enumerable.Empty<string>())
                {
                    Uri uri = ConstructUir(prefix.Scheme, bucketName, pre);
                    yield return new ObjectMetadata(uri, ObjectType.Prefix, null, null);

                    if (recursive)
                        keyPrefixQueue.Enqueue(pre);
                }
            }
        }
    }

    public async Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
    {
        (string bucketName, string key) = DeconstructUri(uri);

        try
        {
            Object obj = await this.Client.GetObjectAsync(bucketName, key, cancellationToken: cancellationToken).ConfigureAwait(false);
            return GetMetadataFromObject(uri, obj);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotExistsException(uri, inner: ex);
        }
    }

    public async Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(uri);

        (string bucketName, string key) = DeconstructUri(uri);

        MemoryStream stream = new();
        await this.Client.DownloadObjectAsync(bucketName, key, stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public async Task PutFileAsync(Uri uri, Stream content, bool overwrite, CancellationToken cancellationToken)
    {
        if (!overwrite && await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(uri);

        (string bucketName, string key) = DeconstructUri(uri);
        await this.TryCreateBucketIfNotExistsAsync(bucketName, cancellationToken).ConfigureAwait(false);
        await this.Client.UploadObjectAsync(bucketName, key, null, content, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            return false;

        (string bucketName, string key) = DeconstructUri(uri);
        await this.Client.DeleteObjectAsync(bucketName, key, cancellationToken: cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken)
    {
        await this.CopyFileAsync(oldUri, newUri, overwrite, cancellationToken).ConfigureAwait(false);
        await this.DeleteFileAsync(oldUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
    {
        if (sourceUri == destUri)
            throw new ArgumentException("Can't copy file to itself.");

        if (!await this.DoesFileExistAsync(sourceUri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(sourceUri);

        if (!overwrite && await this.DoesFileExistAsync(destUri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(destUri);

        (string sourceBucket, string sourceKey) = DeconstructUri(sourceUri);
        (string destinationBucket, string destinationKey) = DeconstructUri(destUri);

        await this.TryCreateBucketIfNotExistsAsync(destinationBucket, cancellationToken).ConfigureAwait(false);
        await this.Client.CopyObjectAsync(sourceBucket, sourceKey, destinationBucket, destinationKey, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
    {
        (string bucketName, string key) = DeconstructUri(uri);
        try
        {
            await this.Client.GetObjectAsync(bucketName, key, cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    #endregion

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

    private static ObjectMetadata GetMetadataFromObject(Uri uri, Object obj)
    {
        Uri objectUri = ConstructUir(uri.Scheme, obj.Bucket, obj.Name);
        // Object.UpdatedDateTimeOffset has a bug.
        // It throws exceptions when parsing time like 2026-01-01T00:00:00Z
        DateTime lastModifiedTimeUtc = DateTime.Parse(obj.UpdatedRaw).ToUniversalTime();
        return new(objectUri, ObjectType.File, (long?)obj.Size, lastModifiedTimeUtc);
    }

    private async Task<bool> DoesBucketExistAsync(string bucketName)
    {
        try
        {
            await this.Client.GetBucketAsync(bucketName).ConfigureAwait(false);
            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private async Task TryCreateBucketIfNotExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        try
        {
            Bucket bucket = await this.Client.GetBucketAsync(bucketName, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            if (this.Settings.GetBoolValue("CreateBucketIfNotExists", () => null) ?? false)
            {
                string projectId = this.Settings.GetValue<string>("ProjectId");
                await this.Client
                    .CreateBucketAsync(projectId, bucketName, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}