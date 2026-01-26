using Amazon.Runtime;
using Amazon.S3;
using Azure.Storage;
using Azure.Storage.Blobs;
using Basalt.UniversalFileSystem.AliyunOss;
using Basalt.UniversalFileSystem.AwsS3;
using Basalt.UniversalFileSystem.AzureBlob;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.File;
using Basalt.UniversalFileSystem.GoogleCloudStorage;
using Basalt.UniversalFileSystem.Memory;
using Basalt.UniversalFileSystem.Sftp;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Basalt.UniversalFileSystem.IntegrationTests;

public static class UniversalFileSystemStore
{
    public static IEnumerable<object[]> GetSingleUniversalFileSystem()
    {
        IUniversalFileSystem ufs = CreateUniversalFileSystem();
        List<UriWrapper> uriWrappers = CreateUriWrappers(ufs);

        return uriWrappers
            .Select(x =>
            {
                InitializeUfsWrapperAsync(ufs, x).Wait();
                return new object[] { ufs, x };
            });
    }

    public static IEnumerable<object[]> GetTwoUniversalFileSystem()
    {
        IUniversalFileSystem ufs = CreateUniversalFileSystem();
        List<UriWrapper> uriWrappers = CreateUriWrappers(ufs);
        UriWrapper memoryUriWrapper = uriWrappers.First(x => x.Name == "memory");
        List<UriWrapper> nonMemoryUriWrappers = uriWrappers.Where(x => x.Name != "memory").ToList();

        var firstUriWrappers = nonMemoryUriWrappers.Select(x => (x, memoryUriWrapper));
        var secondUriWrappers = nonMemoryUriWrappers.Select(x => (memoryUriWrapper, x));
        var selfUriWrappers = uriWrappers.Select(x => (x, x));

        return firstUriWrappers.Concat(secondUriWrappers).Concat(selfUriWrappers)
            .Select(x =>
            {
                InitializeUfsWrappersAsync(ufs, x.Item1, x.Item2).Wait();
                return new object[] { ufs, x.Item1, x.Item2 };
            });
    }

    private static List<UriWrapper> CreateUriWrappers(IUniversalFileSystem ufs)
    {
        UriWrapper CreateUriWrapper(IUniversalFileSystem ufs, string name, string baseUri) => new(name, baseUri);

        // UriWrapper CreateFileUriWrapper(IUniversalFileSystem ufs, string name)
        // {
        //     string root = $"{Environment.CurrentDirectory}/ufs-it-file";
        //     return new(name, $"file://{root}/");
        // }

        List<UriWrapper> wrappers =
        [
            CreateUriWrapper(ufs, "memory", "memory://"),
            // CreateFileUriWrapper(ufs, "file"),       // TODO: #38
            CreateUriWrapper(ufs, "s3", "s3://ufs-it-s3"),
            CreateUriWrapper(ufs, "s3-custom-client", "s3://ufs-it-s3-custom-client"),
            CreateUriWrapper(ufs, "abfss", "abfss://ufs-it-abfss"),
            CreateUriWrapper(ufs, "abfss-custom-client", "abfss://ufs-it-abfss-custom-client"),
            CreateUriWrapper(ufs, "gs", "gs://ufs-it-gs"),
            CreateUriWrapper(ufs, "gs-custom-client", "gs://ufs-it-gs-custom-client"),
            // CreateUriWrapper(ufs, "oss", "oss://ufs-it-oss"),   // Can't find an oss emulator which works
            CreateUriWrapper(ufs, "sftp", "sftp://localhost/sftp/ufs-it-sftp/"),
            CreateUriWrapper(ufs, "sftp-custom-client", "sftp://localhost/sftp/ufs-it-sftp-custom-client/"),
        ];
        return wrappers;
    }

    private static IUniversalFileSystem CreateUniversalFileSystem()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) => { builder.AddYamlFile("it-settings.yaml", false, false); })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddUniversalFileSystem("UniversalFileSystemStore")
                    .AddMemoryFileSystem()
                    .AddFileFileSystem()
                    .AddAwsS3FileSystem()
                    .AddAwsS3CustomClient("S3CustomClient", _ =>
                        new AmazonS3Client(new BasicAWSCredentials("test", "test"),
                            new AmazonS3Config()
                            {
                                ServiceURL = "http://localhost:4566",
                                ForcePathStyle = true,
                            }))
                    .AddAzureBlobFileSystem()
                    .AddAzureBlobCustomClient("AzureBlobCustomClient", _ =>
                        new BlobServiceClient(new Uri("http://localhost:10000/account2"),
                            new StorageSharedKeyCredential("account2", "a2V5Mg==")))
                    .AddGoogleCloudStorageFileSystem()
                    .AddGoogleCloudStorageCustomClient("GoogleCloudStorageCustomClient", _ =>
                        new StorageClientBuilder()
                        {
                            UnauthenticatedAccess = true,
                            BaseUri = "http://localhost:4443/storage/v1/",
                        }.Build())
                    .AddAliyunOssFileSystem()
                    .AddSftpFileSystem()
                    .AddSftpCustomClient("SftpCustomClient", _ =>
                    {
                        SftpClient client = new("localhost", 2222, "demo", "demo");
                        client.Connect();
                        return client;
                    });
            })
            .Build();
        return host.Services.GetRequiredService<IUniversalFileSystem>();
    }

    private static async Task InitializeUfsWrappersAsync(IUniversalFileSystem ufs, params UriWrapper[] uriWrappers)
    {
        foreach (UriWrapper uriWrapper in uriWrappers)
        {
            await InitializeUfsWrapperAsync(ufs, uriWrapper);
        }
    }

    private static async Task InitializeUfsWrapperAsync(IUniversalFileSystem ufs, UriWrapper uriWrapper)
    {
        if (uriWrapper.BaseUri.Scheme.StartsWith("file"))
        {
            string root = uriWrapper.BaseUri.LocalPath;

            // Delete all files
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
        else if (uriWrapper.BaseUri.Scheme.StartsWith("sftp"))
        {
            string root = uriWrapper.BaseUri.LocalPath.TrimEnd('/');

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
        }
        else
        {
            // delete all files
            IAsyncEnumerable<ObjectMetadata> allFiles = ufs.ListObjectsAsync(uriWrapper.GetFullUri(""), true);
            await foreach (ObjectMetadata file in allFiles)
                await ufs.DeleteFileAsync(file.Uri);
        }
    }
}