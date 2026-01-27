using System.Threading;
using Basalt.UniversalFileSystem.Core;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Basalt.UniversalFileSystem.IntegrationTests.Utils;

public class UriWrapper
{
    private readonly SemaphoreSlim _lock;

    protected UriWrapper(string name, string baseUri)
    {
        this.Name = name;
        this.BaseUri = new(baseUri);

        _lock = new(1, 1);
    }

    public string Name { get; }
    public Uri BaseUri { get; }
    public Uri GetFullUri(string path) => new(this.BaseUri, new Uri(path, UriKind.RelativeOrAbsolute));
    public override string ToString() => this.Name;

    public IDisposable Lock()
    {
        Console.WriteLine($"Locking {this.Name}");
        _lock.Wait();
        return new ReleaseLock(this, _lock);
    }

    private class ReleaseLock(UriWrapper uriWrapper, SemaphoreSlim locker) : IDisposable
    {
        public void Dispose()
        {
            locker.Release();
            Console.WriteLine($"Released lock {uriWrapper.Name}");
        }
    }

    public virtual async Task InitializeAsync(IUniversalFileSystem ufs)
    {
        // delete all files
        IAsyncEnumerable<ObjectMetadata> allFiles = ufs.ListObjectsAsync(this.GetFullUri(""), true);
        await foreach (ObjectMetadata file in allFiles)
            await ufs.DeleteFileAsync(file.Uri);
    }

    public static readonly UriWrapper Memory = new("memory", "memory://");
    public static readonly UriWrapper File = new FileUriWrapper("file", $"file://{Environment.CurrentDirectory}/ufs-it-file/");
    public static readonly UriWrapper S3 = new("s3", "s3://ufs-it-s3");
    public static readonly UriWrapper S3CustomClient = new("s3-custom-client", "s3://ufs-it-s3-custom-client");
    public static readonly UriWrapper Abfss = new("abfss", "abfss://ufs-it-abfss");
    public static readonly UriWrapper AbfssCustomClient = new("abfss-custom-client", "abfss://ufs-it-abfss-custom-client");
    public static readonly UriWrapper Gs = new("gs", "gs://ufs-it-gs");
    public static readonly UriWrapper GsCustomClient = new("gs-custom-client", "gs://ufs-it-gs-custom-client");
    public static readonly UriWrapper Sftp = new SftpUriWrapper("sftp", "sftp://localhost/sftp/ufs-it-sftp/");
    public static readonly UriWrapper SftpCustomClient = new SftpUriWrapper("sftp-custom-client", "sftp://localhost/sftp/ufs-it-sftp-custom-client/");

    public static readonly IEnumerable<UriWrapper> NonMemoryUriWrappers =
    [
        // File,  // TODO: #38
        S3, S3CustomClient, Abfss, AbfssCustomClient, Gs, GsCustomClient, Sftp, SftpCustomClient
    ];

    public static readonly IEnumerable<UriWrapper> AllUriWrappers = NonMemoryUriWrappers.Concat([Memory]);
}

public class FileUriWrapper : UriWrapper
{
    public FileUriWrapper(string name, string baseUri) : base(name, baseUri)
    {
    }

    public override Task InitializeAsync(IUniversalFileSystem ufs)
    {
        string root = this.BaseUri.LocalPath;

        // Delete all files
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }

        return Task.CompletedTask;
    }
}

public class SftpUriWrapper : UriWrapper
{
    public SftpUriWrapper(string name, string baseUri) : base(name, baseUri)
    {
    }

    public override Task InitializeAsync(IUniversalFileSystem ufs)
    {
        string root = this.BaseUri.LocalPath.TrimEnd('/');

        using SftpClient client = new("localhost", 2222, "demo", "demo");
        client.Connect();

        void DeleteSftpDirectory(string directory)
        {
            if (!client.Exists(directory))
                return;

            foreach (SftpFile file in client.ListDirectory(directory))
            {
                if (file.Name == "." || file.Name == "..")
                    continue;

                Console.WriteLine(file.FullName);

                if (file.IsDirectory)
                    DeleteSftpDirectory(file.FullName);
                else
                    client.DeleteFile(file.FullName);
            }

            client.DeleteDirectory(directory);
        }

        DeleteSftpDirectory(root);
        return Task.CompletedTask;
    }
}