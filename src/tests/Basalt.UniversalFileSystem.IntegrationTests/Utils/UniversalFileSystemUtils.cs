using Amazon.Runtime;
using Amazon.S3;
using Azure.Storage;
using Azure.Storage.Blobs;
using Basalt.UniversalFileSystem.AliyunOss;
using Basalt.UniversalFileSystem.AwsS3;
using Basalt.UniversalFileSystem.AzureBlob;
using Basalt.UniversalFileSystem.File;
using Basalt.UniversalFileSystem.GoogleCloudStorage;
using Basalt.UniversalFileSystem.Memory;
using Basalt.UniversalFileSystem.Sftp;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Renci.SshNet;

namespace Basalt.UniversalFileSystem.IntegrationTests.Utils;

static class UniversalFileSystemUtils
{
    private static readonly Lazy<IHost> _host = new(() =>
    {
        return Host.CreateDefaultBuilder()
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
    });

    public static IUniversalFileSystem GetUniversalFileSystem()
    {
        return _host.Value.Services.GetRequiredService<IUniversalFileSystem>();
    }

    public static Task<IDisposable> InitializeFileSystemsAsync(IUniversalFileSystem ufs, params UriWrapper[] uriWrappers)
    {
        IEnumerable<IDisposable> releases = uriWrappers
            .Distinct(ReferenceEqualityComparer.Instance)
            .Cast<UriWrapper>()
            .Select(x =>
            {
                IDisposable release = x.Lock();
                x.InitializeAsync(ufs).Wait();
                return release;
            })
            .ToList();

        return Task.FromResult<IDisposable>(new CompositeDisposable(releases));
    }

    class CompositeDisposable(IEnumerable<IDisposable> disposables) : IDisposable
    {
        public void Dispose()
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }
}