using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem.File;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class FileFileSystem : AsyncDisposable, IFileSystem
{
    #region IFileSystem

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        EnumerationOptions enumerationOptions = new()
        {
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false
        };

        async IAsyncEnumerable<ObjectMetadata> EnumerateDirectoryAsync(string directory, string startsWith)
        {
            foreach (string entry in Directory.EnumerateFiles(directory, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!entry.Contains(startsWith)) continue;  // TODO: is it correct?

                ObjectMetadata? metadata = await this.GetObjectMetadataInternalAsync(new Uri(entry), false, cancellationToken);
                if (metadata != null)
                    yield return metadata;
            }

            foreach (string entry in Directory.EnumerateDirectories(directory, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!entry.Contains(startsWith)) continue;  // TODO: is it correct?

                ObjectMetadata? metadata = await this.GetObjectMetadataInternalAsync(new Uri(entry), true, cancellationToken);
                if (metadata != null)
                    yield return metadata;

                if (!recursive) continue;

                await foreach (ObjectMetadata objectMetadata in EnumerateDirectoryAsync(entry, string.Empty))
                {
                    yield return objectMetadata;
                }
            }
        }

        int lastSlashIndex = prefix.AbsolutePath.LastIndexOf('/');
        string directory = prefix.AbsolutePath.Substring(0, lastSlashIndex);
        string startsWith = prefix.AbsolutePath.Substring(lastSlashIndex + 1);

        Uri directoryUri = new UriBuilder()
        {
            Scheme = prefix.Scheme,
            Host = null,
            Path = directory,
        }.Uri;

        if (!await this.DoesDirectoryExistAsync(directoryUri, cancellationToken))
            yield break;

        await foreach (ObjectMetadata objectMetadata in EnumerateDirectoryAsync(directory, startsWith))
        {
            yield return objectMetadata;
        }
    }

    public Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        return this.GetObjectMetadataInternalAsync(path, false, cancellationToken);
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        return Task.FromResult((Stream)new FileStream(path.AbsolutePath, FileMode.Open, FileAccess.Read));
    }

    public async Task PutObjectAsync(Uri path, Stream stream, bool overwrite, CancellationToken cancellationToken)
    {
        string? dir = Path.GetDirectoryName(path.AbsolutePath);
        if (dir != null)
            Directory.CreateDirectory(dir);
        await using FileStream fileStream = new(path.AbsolutePath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public async Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        if (!await this.DoesFileExistAsync(path, cancellationToken))
            return false;

        System.IO.File.Delete(path.AbsolutePath);
        return true;
    }

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwrite, CancellationToken cancellationToken)
    {
        if (oldPath == newPath)
            throw new ArgumentException("Can't move object to itself.");

        string? directory = Path.GetDirectoryName(newPath.AbsolutePath);
        if (directory == null)
            return Task.FromException(new ArgumentException($"Can't get directory from path {newPath}"));
        Directory.CreateDirectory(directory);

        System.IO.File.Move(oldPath.AbsolutePath, newPath.AbsolutePath, overwrite);
        return Task.CompletedTask;
    }

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwrite, CancellationToken cancellationToken)
    {
        if (sourcePath == destPath)
            throw new ArgumentException("Can't move object to itself.");

        string? directory = Path.GetDirectoryName(destPath.AbsolutePath);
        if (directory == null)
            return Task.FromException(new ArgumentException($"Can't get directory from path {destPath}"));
        Directory.CreateDirectory(directory);

        System.IO.File.Copy(sourcePath.AbsolutePath, destPath.AbsolutePath, overwrite);
        return Task.CompletedTask;
    }

    public Task<bool> DoesFileExistAsync(Uri path, CancellationToken cancellationToken) => Task.FromResult(System.IO.File.Exists(path.AbsolutePath));

    #endregion

    private Task<bool> DoesDirectoryExistAsync(Uri path, CancellationToken cancellationToken) => Task.FromResult(System.IO.Directory.Exists(path.AbsolutePath));

    private async Task<ObjectMetadata?> GetObjectMetadataInternalAsync(Uri path, bool returnDirectory, CancellationToken cancellationToken)
    {
        if (await this.DoesFileExistAsync(path, cancellationToken))
            return new ObjectMetadata(path, ObjectType.File, new FileInfo(path.AbsolutePath).Length, System.IO.File.GetLastWriteTimeUtc(path.AbsolutePath));
        if (returnDirectory && await this.DoesDirectoryExistAsync(path, cancellationToken))
            return new ObjectMetadata(new Uri($"{path}/"), ObjectType.Prefix, null, null);

        return null;
    }
}