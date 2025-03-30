using Amazon.Runtime;
using Amazon.S3;
using Azure.Storage;
using Azure.Storage.Blobs;
using BasaltHexagons.UniversalFileSystem.AliyunOss;
using BasaltHexagons.UniversalFileSystem.AwsS3;
using BasaltHexagons.UniversalFileSystem.AzureBlob;
using BasaltHexagons.UniversalFileSystem.File;
using BasaltHexagons.UniversalFileSystem.Memory;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public abstract class UniversalFileSystemStore
{
    public static IEnumerable<object[]> GetSingleUniversalFileSystem()
    {
        IUniversalFileSystem ufs = CreateUniversalFileSystem();
        List<UriWrapper> uriWrappers = CreateUriWrappers(ufs).ToList();

        return uriWrappers
            .Select(x =>
            {
                ufs.InitializeAsync(x).Wait();
                return new object[] { ufs, x };
            });
    }

    public static IEnumerable<object[]> GetTwoUniversalFileSystem()
    {
        IUniversalFileSystem ufs = CreateUniversalFileSystem();
        List<UriWrapper> uriWrappers = CreateUriWrappers(ufs).ToList();

        return uriWrappers.Join(uriWrappers, _ => true, _ => true, (x, y) =>
            {
                // This is used for debugging
                // if (x.ToString() != "abfss" || y.ToString() != "abfss2")
                //     return null;
                ufs.InitializeAsync(x, y).Wait();
                return new object[] { ufs, x, y };
            })
            .Where(x => x != null)
            .Cast<object[]>();
    }

    private static IEnumerable<UriWrapper> CreateUriWrappers(IUniversalFileSystem ufs)
    {
        static UriWrapper CreateUriWrapper(IUniversalFileSystem ufs, string name, string baseUri) => new(name, baseUri);

        static UriWrapper CreateFileUriWrapper(IUniversalFileSystem ufs, string name)
        {
            string root = $"{Environment.CurrentDirectory}/ufs-integration-test-file";
            return CreateUriWrapper(ufs, name, $"file://{root}/");
        }

        yield return CreateUriWrapper(ufs, "memory", "memory://");
        yield return CreateFileUriWrapper(ufs, "file");
        yield return CreateUriWrapper(ufs, "s3", "s3://ufs-it-s3");
        yield return CreateUriWrapper(ufs, "s3-custom-client", "s3://ufs-it-s3-custom-client");
        yield return CreateUriWrapper(ufs, "abfss", "abfss://ufs-it-abfss");
        yield return CreateUriWrapper(ufs, "abfss-custom-client", "abfss://ufs-it-abfss-custom-client");
        // yield return CreateUriWrapper(ufs, "oss", "oss://ufs-it-oss");   # Can't find oss-emulator
    }

    private static IUniversalFileSystem CreateUniversalFileSystem()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) => { builder.AddYamlFile("test-settings.yaml", false, false); })
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
                    .AddAliyunOssFileSystem();
            })
            .Build();
        return host.Services.GetRequiredService<IUniversalFileSystem>();
    }
}