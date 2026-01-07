using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Disposing;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Renci.SshNet;
using Renci.SshNet.Async;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Basalt.UniversalFileSystem.Sftp;

class SftpFileSystem : AsyncDisposable, IFileSystem
{
    private static readonly IReadOnlySet<string> SpecialDirectories = new HashSet<string>() { ".", ".." };

    public SftpFileSystem(SftpClient client)
    {
        this.Client = client;
    }

    private SftpClient Client { get; }

    #region IFileSystem

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        (string host, string rootPath) = DeconstructUri(prefix);

        Queue<string> prefixQueue = new();

        ObjectMetadata? ParseSftpFile(SftpFile file)
        {
            if (file.IsDirectory)
            {
                if (SpecialDirectories.Contains(file.Name))
                    return null;

                if (recursive)
                    prefixQueue.Enqueue(file.FullName);

                Uri uri = ConstructUri(prefix.Scheme, host, $"{file.FullName}/");
                return ToObjectMetadata(uri, file.Attributes)!;
            }
            else if (file.IsRegularFile)
            {
                Uri uri = ConstructUri(prefix.Scheme, host, file.FullName);
                return ToObjectMetadata(uri, file.Attributes)!;
            }

            return null;
        }

        if (rootPath.EndsWith('/'))
        {
            prefixQueue.Enqueue(rootPath);
        }
        else
        {
            // handle folder path is partially provided
            // get parent directory
            string? parent = GetParentDirectory(rootPath);
            if (parent != null)
            {
                string partialName = Path.GetFileName(rootPath);
                IEnumerable<SftpFile> files = await this.Client.ListDirectoryAsync(parent).ConfigureAwait(false);
                foreach (SftpFile file in files.Where(x => x.Name.StartsWith(partialName)))
                {
                    ObjectMetadata? metadata = ParseSftpFile(file);
                    if (metadata != null)
                        yield return metadata;
                }
            }
        }

        while (prefixQueue.Count > 0)
        {
            string path = prefixQueue.Dequeue();
            if (!this.Client.Exists(path)) continue;

            IEnumerable<SftpFile> files = await this.Client.ListDirectoryAsync(path).ConfigureAwait(false);

            foreach (SftpFile file in files)
            {
                ObjectMetadata? metadata = ParseSftpFile(file);
                if (metadata != null)
                    yield return metadata;
            }
        }
    }

    public Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
    {
        (string _, string path) = DeconstructUri(uri);

        SftpFileAttributes attributes;
        try
        {
            attributes = this.Client.GetAttributes(path);
        }
        catch (SftpPathNotFoundException ex)
        {
            return Task.FromException<ObjectMetadata>(new FileNotExistsException(uri, inner: ex));
        }

        ObjectMetadata? metadata = ToObjectMetadata(uri, attributes);
        if (metadata == null || metadata.ObjectType == ObjectType.Prefix)
            return Task.FromException<ObjectMetadata>(new FileNotExistsException(uri));

        return Task.FromResult(metadata);
    }

    public async Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(uri);

        (string _, string path) = DeconstructUri(uri);

        MemoryStream memoryStream = new();
        await this.Client.DownloadAsync(path, memoryStream).ConfigureAwait(false);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    public async Task PutFileAsync(Uri uri, Stream content, bool overwrite, CancellationToken cancellationToken)
    {
        if (!overwrite && await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(uri);

        (string _, string path) = DeconstructUri(uri);
        await CreateFileDirectoryIfMissingAsync(this.Client, path).ConfigureAwait(false);
        await this.Client.UploadAsync(content, path, overwrite).ConfigureAwait(false);
    }

    public async Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            return false;

        (string _, string path) = DeconstructUri(uri);
        this.Client.DeleteFile(path);
        return true;
    }

    public async Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken)
    {
        if (oldUri == newUri)
            throw new ArgumentException("Can't rename file to itself.");

        if (!await this.DoesFileExistAsync(oldUri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(oldUri);

        if (!overwrite && await this.DoesFileExistAsync(newUri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(newUri);

        (string _, string oldPath) = DeconstructUri(oldUri);
        (string _, string newPath) = DeconstructUri(newUri);

        await CreateFileDirectoryIfMissingAsync(this.Client, newPath).ConfigureAwait(false);
        this.Client.RenameFile(oldPath, newPath, overwrite);
    }

    public async Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
    {
        if (sourceUri == destUri)
            throw new ArgumentException("Can't copy file to itself.");

        if (!await this.DoesFileExistAsync(sourceUri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(sourceUri);

        if (!overwrite && await this.DoesFileExistAsync(destUri, cancellationToken).ConfigureAwait(false))
            throw new FileExistsException(destUri);

        await using Stream stream = await this.GetFileAsync(sourceUri, cancellationToken).ConfigureAwait(false);
        await this.PutFileAsync(destUri, stream, overwrite, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
    {
        (string _, string path) = DeconstructUri(uri);

        SftpFileAttributes attributes;
        try
        {
            attributes = this.Client.GetAttributes(path);
        }
        catch (SftpPathNotFoundException)
        {
            return Task.FromResult(false);
        }

        ObjectMetadata? metadata = ToObjectMetadata(uri, attributes);
        return Task.FromResult(metadata != null && metadata.ObjectType == ObjectType.File);
    }

    #endregion IFileSystem

    #region AsyncDisposable

    protected override void DisposeManagedObjects()
    {
        this.Client.Dispose();
        base.DisposeManagedObjects();
    }

    #endregion AsyncDisposable

    private static Task CreateFileDirectoryIfMissingAsync(SftpClient client, string filepath)
    {
        string? parent = GetParentDirectory(filepath);
        return CreateDirectoryIfMissingAsync(client, parent);
    }

    private static async Task CreateDirectoryIfMissingAsync(SftpClient client, string? directory)
    {
        if (string.IsNullOrEmpty(directory)) return;

        if (!client.Exists(directory))
        {
            // Recursively create parent first
            string? parent = GetParentDirectory(directory);
            if (parent != null)
                await CreateDirectoryIfMissingAsync(client, parent).ConfigureAwait(false);

            client.CreateDirectory(directory);
        }
    }

    private static string? GetParentDirectory(string path) => Path.GetDirectoryName(path)?.Replace("\\", "/");

    private static (string Host, string path) DeconstructUri(Uri uri)
    {
        return (uri.Host, uri.AbsolutePath);
    }

    private static Uri ConstructUri(string scheme, string host, string path)
    {
        UriBuilder builder = new()
        {
            Scheme = scheme,
            Host = host,
            Path = path,
        };
        return builder.Uri;
    }

    private static ObjectMetadata? ToObjectMetadata(Uri uri, SftpFileAttributes attributes)
    {
        if (attributes.IsRegularFile)
            return new(uri, ObjectType.File, attributes.Size, attributes.LastWriteTime.ToUniversalTime());
        else if (attributes.IsDirectory)
            return new(uri, ObjectType.Prefix, null, null);
        else
            return null;
    }
}