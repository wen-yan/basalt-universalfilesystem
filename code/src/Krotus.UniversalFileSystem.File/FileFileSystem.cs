using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem.File;

public class FileFileSystem : IFileSystem
{
    #region IFileSystem

    public async IAsyncEnumerable<ObjectMetadata> ListObjectsAsync(string prefix, bool recursive, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(prefix, "*", new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            ReturnSpecialDirectories = true
        });
        foreach (string entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(entry))
                yield return new ObjectMetadata(entry, ObjectType.Prefix, null, null);
            else if (System.IO.File.Exists(entry)) yield return await this.GetObjectMetadataAsync(entry, cancellationToken);
        }

        await ValueTask.CompletedTask;
    }

    public Task<ObjectMetadata> GetObjectMetadataAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ObjectMetadata(path, ObjectType.File, new FileInfo(path).Length, System.IO.File.GetLastWriteTime(path)));
    }

    public Task<Stream> GetObjectAsync(string path, CancellationToken cancellationToken)
    {
        Uri pathUri = new(path);
        return Task.FromResult((Stream)new FileStream(pathUri.AbsolutePath, FileMode.Open, FileAccess.Read));
    }

    public async Task PutObjectAsync(string path, Stream stream, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        Uri pathUri = new(path);
        await using FileStream fileStream = new(pathUri.AbsolutePath, overwriteIfExists ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<bool> DeleteObjectAsync(string path, CancellationToken cancellationToken)
    {
        Uri pathUri = new(path);

        if (!System.IO.File.Exists(pathUri.AbsolutePath))
            return Task.FromResult(false);

        System.IO.File.Delete(pathUri.AbsolutePath);
        return Task.FromResult(true);
    }

    public Task RenameObjectAsync(string oldPath, string newPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        Uri oldPathUri = new(oldPath);
        Uri newPathUri = new(newPath);
        System.IO.File.Move(oldPathUri.AbsolutePath, newPathUri.AbsolutePath, overwriteIfExists);
        return Task.CompletedTask;
    }

    public Task CopyObjectAsync(string sourcePath, string destPath, bool overwriteIfExists, CancellationToken cancellationToken)
    {
        Uri sourcePathUri = new(sourcePath);
        Uri destPathUri = new(destPath);
        System.IO.File.Copy(sourcePathUri.AbsolutePath, destPathUri.AbsolutePath, overwriteIfExists);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #endregion
}