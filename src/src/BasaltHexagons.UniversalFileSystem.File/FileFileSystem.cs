using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Disposing;

namespace BasaltHexagons.UniversalFileSystem.File;

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

        async IAsyncEnumerable<ObjectMetadata> EnumerateDirectoryAsync(string directory, string startsWith, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (string entry in Directory.EnumerateFiles(directory, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!entry.Contains(startsWith)) continue;

                ObjectMetadata? metadata = await this.GetObjectMetadataInternalAsync(new Uri(entry), false);
                if (metadata != null)
                    yield return metadata;
            }

            foreach (string entry in Directory.EnumerateDirectories(directory, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!entry.Contains(startsWith)) continue;

                ObjectMetadata? metadata = await this.GetObjectMetadataInternalAsync(new Uri(entry), true);
                if (metadata != null)
                    yield return metadata;

                if (!recursive) continue;

                await foreach (ObjectMetadata objectMetadata in EnumerateDirectoryAsync(entry, string.Empty, cancellationToken))
                {
                    yield return objectMetadata;
                }
            }
        }

        int lastSlashIndex = prefix.AbsolutePath.LastIndexOf('/');
        string directory = prefix.AbsolutePath.Substring(0, lastSlashIndex);
        string startsWith = prefix.AbsolutePath.Substring(lastSlashIndex + 1);

        if (!Directory.Exists(directory))
            yield break;

        await foreach (ObjectMetadata objectMetadata in EnumerateDirectoryAsync(directory, startsWith, cancellationToken))
        {
            yield return objectMetadata;
        }
    }

    public Task<ObjectMetadata?> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        return this.GetObjectMetadataInternalAsync(path, false);
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        return Task.FromResult((Stream)new FileStream(path.AbsolutePath, FileMode.Open, FileAccess.Read));
    }

    public async Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        string? dir = Path.GetDirectoryName(path.AbsolutePath);
        if (dir != null)
            Directory.CreateDirectory(dir);
        await using FileStream fileStream = new(path.AbsolutePath, overwriteIfExists ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> DeleteObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        if (!System.IO.File.Exists(path.AbsolutePath))
            return Task.FromResult(false);

        System.IO.File.Delete(path.AbsolutePath);
        return Task.FromResult(true);
    }

    public Task MoveObjectAsync(Uri oldPath, Uri newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(newPath.AbsolutePath);
        if (directory == null)
            return Task.FromException(new ArgumentException($"Can't get directory from path {newPath}"));
        Directory.CreateDirectory(directory);
        System.IO.File.Move(oldPath.AbsolutePath, newPath.AbsolutePath, overwriteIfExists);
        return Task.CompletedTask;
    }

    public Task CopyObjectAsync(Uri sourcePath, Uri destPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        System.IO.File.Copy(sourcePath.AbsolutePath, destPath.AbsolutePath, overwriteIfExists);
        return Task.CompletedTask;
    }

    #endregion

    private Task<ObjectMetadata?> GetObjectMetadataInternalAsync(Uri path, bool returnDirectory)
    {
        if (System.IO.File.Exists(path.AbsolutePath))
            return Task.FromResult<ObjectMetadata?>(new ObjectMetadata(path, ObjectType.File, new FileInfo(path.AbsolutePath).Length, System.IO.File.GetLastWriteTimeUtc(path.AbsolutePath)));
        if (returnDirectory && System.IO.Directory.Exists(path.AbsolutePath))
            return Task.FromResult<ObjectMetadata?>(new ObjectMetadata(new Uri($"{path}/"), ObjectType.Prefix, null, null));

        return Task.FromResult<ObjectMetadata?>(null);
    }
}