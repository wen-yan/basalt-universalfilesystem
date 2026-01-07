using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Disposing;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Renci.SshNet;
using Renci.SshNet.Async;
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
        Queue<string> prefixQueue = new();
        prefixQueue.Enqueue(DeconstructUri(prefix));

        while (prefixQueue.Count > 0)
        {
            string path = prefixQueue.Dequeue();
            IEnumerable<SftpFile> files = await this.Client.ListDirectoryAsync(path).ConfigureAwait(false);

            foreach (SftpFile file in files)
            {
                Uri uri = ConstructUri(prefix.Scheme, file.FullName);

                if (file.IsDirectory)
                {
                    if (SpecialDirectories.Contains(file.Name))
                        continue;

                    yield return ToObjectMetadata(uri, file.Attributes)!;

                    if (recursive)
                        prefixQueue.Enqueue(file.FullName);
                }
                else if (file.IsRegularFile)
                    yield return ToObjectMetadata(uri, file.Attributes)!;
            }
        }
    }

    public Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
    {
        string path = DeconstructUri(uri);
        SftpFileAttributes attributes = this.Client.GetAttributes(path);

        ObjectMetadata? metadata = ToObjectMetadata(uri, attributes);
        if (metadata == null || metadata.ObjectType == ObjectType.Prefix)
            return Task.FromException<ObjectMetadata>(new FileNotExistsException(uri));

        return Task.FromResult(metadata);
    }

    public async Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            throw new FileNotExistsException(uri);

        string path = DeconstructUri(uri);

        MemoryStream memoryStream = new();
        await this.Client.DownloadAsync(path, memoryStream).ConfigureAwait(false);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    public async Task PutFileAsync(Uri uri, Stream content, bool overwrite, CancellationToken cancellationToken)
    {
        string path = DeconstructUri(uri);
        await this.Client.UploadAsync(content, path, overwrite).ConfigureAwait(false);
    }

    public async Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(uri, cancellationToken).ConfigureAwait(false))
            return false;

        string path = DeconstructUri(uri);
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

        string oldPath = DeconstructUri(oldUri);
        string newPath = DeconstructUri(newUri);
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

        await using (Stream stream = await this.GetFileAsync(sourceUri, cancellationToken).ConfigureAwait(false))
        {
            await this.PutFileAsync(destUri, stream, overwrite, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken)
    {
        string path = DeconstructUri(uri);
        return Task.FromResult(this.Client.Exists(path));
    }

    #endregion IFileSystem

    #region AsyncDisposable

    protected override void DisposeManagedObjects()
    {
        this.Client.Dispose();
        base.DisposeManagedObjects();
    }

    #endregion AsyncDisposable

    private static string DeconstructUri(Uri uri)
    {
        return uri.AbsolutePath;
    }

    private static Uri ConstructUri(string scheme, string path)
    {
        UriBuilder builder = new()
        {
            Scheme = scheme,
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