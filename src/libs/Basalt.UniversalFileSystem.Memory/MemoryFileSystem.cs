using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Disposing;
using Basalt.UniversalFileSystem.Core.Exceptions;

namespace Basalt.UniversalFileSystem.Memory;

record File(byte[] Content, DateTime LastModifiedTimeUtc);

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class MemoryFileSystem : AsyncDisposable, IFileSystem
{
    private ConcurrentDictionary<string, File> _files = new();

    #region IFileSystem

    public IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, CancellationToken cancellationToken)
    {
        async IAsyncEnumerable<ObjectMetadata> GetObjectsAsync(string pre)
        {
            HashSet<string> subDirectories = new();

            foreach ((string uri, File file) in _files)
            {
                if (!uri.StartsWith(pre)) continue;

                int nextSeparatorIndex = uri.IndexOf('/', pre.Length);
                if (nextSeparatorIndex == -1)
                {
                    yield return new ObjectMetadata(MakeUri(prefix, uri), ObjectType.File, file.Content.Length, file.LastModifiedTimeUtc);
                }
                else
                {
                    string subDirectory = uri.Substring(0, nextSeparatorIndex + 1);
                    subDirectories.Add(subDirectory);
                }
            }

            foreach (string subDirectory in subDirectories)
            {
                yield return new ObjectMetadata(MakeUri(prefix, subDirectory), ObjectType.Prefix, null, null);
                if (recursive)
                {
                    await foreach (ObjectMetadata objectMetadata in GetObjectsAsync(subDirectory))
                    {
                        yield return objectMetadata;
                    }
                }
            }
        }

        return GetObjectsAsync(prefix.AbsolutePath);
    }

    public Task<ObjectMetadata> GetFileMetadataAsync(Uri uri, CancellationToken cancellationToken)
    {
        return _files.TryGetValue(uri.AbsolutePath, out File? file)
            ? Task.FromResult<ObjectMetadata>(new ObjectMetadata(uri, ObjectType.File, file.Content.Length, file.LastModifiedTimeUtc))
            : Task.FromException<ObjectMetadata>(new FileNotExistsException(uri));
    }

    public Task<Stream> GetFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        return _files.TryGetValue(uri.AbsolutePath, out File? file)
            ? Task.FromResult<Stream>(new MemoryStream(file.Content))
            : throw new FileNotExistsException(uri);
    }

    public async Task PutFileAsync(Uri uri, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        if (!overwrite && await this.DoesFileExistAsync(uri, cancellationToken))
            throw new FileExistsException(uri);

        await using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        _files[uri.AbsolutePath] = new File(memoryStream.ToArray(), DateTime.UtcNow);
    }

    public Task<bool> DeleteFileAsync(Uri uri, CancellationToken cancellationToken)
    {
        return Task.FromResult(_files.Remove(uri.AbsolutePath, out File? _));
    }

    public async Task MoveFileAsync(Uri oldUri, Uri newUri, bool overwrite, CancellationToken cancellationToken)
    {
        if (oldUri == newUri)
            throw new ArgumentException("Can't move file to itself");

        bool oldExists = _files.TryGetValue(oldUri.AbsolutePath, out File? file);
        if (!oldExists)
            throw new FileNotExistsException(oldUri);

        if (!overwrite && await this.DoesFileExistAsync(newUri, cancellationToken))
            throw new FileExistsException(newUri);

        _files.Remove(oldUri.AbsolutePath, out _);
        _files[newUri.AbsolutePath] = file!;
    }

    public async Task CopyFileAsync(Uri sourceUri, Uri destUri, bool overwrite, CancellationToken cancellationToken)
    {
        if (sourceUri == destUri)
            throw new ArgumentException("Can't copy file to itself");

        if (!_files.TryGetValue(sourceUri.AbsolutePath, out File? file))
            throw new FileNotExistsException(sourceUri);

        if (!overwrite && await this.DoesFileExistAsync(destUri, cancellationToken))
            throw new FileExistsException(destUri);

        _files[destUri.AbsolutePath] = new File(file!.Content, DateTime.UtcNow);
    }

    public Task<bool> DoesFileExistAsync(Uri uri, CancellationToken cancellationToken) => Task.FromResult(_files.ContainsKey(uri.AbsolutePath));

    #endregion

    private static Uri MakeUri(Uri seed, string uri) => new(seed, uri);
}