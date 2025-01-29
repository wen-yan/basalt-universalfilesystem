using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.File;

public class FileFileSystem : IFileSystem
{
    #region IFileSystem

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(Uri prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(prefix.AbsolutePath, "*", new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            ReturnSpecialDirectories = false
        });
        foreach (string entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(entry))
                yield return new ObjectMetadata(new Uri(entry), ObjectType.Prefix, null, null);
            else if (System.IO.File.Exists(entry))
                yield return await this.GetObjectMetadataAsync(new Uri(entry), cancellationToken);
        }

        await ValueTask.CompletedTask;
    }

    public Task<ObjectMetadata> GetObjectMetadataAsync(Uri path, CancellationToken cancellationToken)
    {
        if (System.IO.File.Exists(path.AbsolutePath))
            return Task.FromResult(new ObjectMetadata(path, ObjectType.File, new FileInfo(path.AbsolutePath).Length, System.IO.File.GetLastWriteTime(path.AbsolutePath)));
        if (System.IO.Directory.Exists(path.AbsolutePath))
            return Task.FromResult(new ObjectMetadata(path, ObjectType.Prefix, null, null));

        return Task.FromException<ObjectMetadata>(new ArgumentException($"File or prefix doesn't exist, {path}"));
    }

    public Task<Stream> GetObjectAsync(Uri path, CancellationToken cancellationToken)
    {
        return Task.FromResult((Stream)new FileStream(path.AbsolutePath, FileMode.Open, FileAccess.Read));
    }

    public async Task PutObjectAsync(Uri path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        await using FileStream fileStream = new(path.AbsolutePath, overwriteIfExists ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);
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

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    #endregion
}