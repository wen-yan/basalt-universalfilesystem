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

    // private static IEnumerable<UniversalFileSystemTestWrapper> GetUniversalFileSystemWrappers(IUniversalFileSystem? ufs = null)
    // {
    //     ufs ??= CreateUniversalFileSystem();
    //
    //     List<UriWrapper> uriWrappers = CreateUriWrappers(ufs).ToList();
    //     return uriWrappers.Select(x =>
    //     {
    //         ufs.InitializeAsync(x).Wait();
    //         return new UniversalFileSystemTestWrapper(ufs, x);
    //     });
    // }

    private static IEnumerable<UriWrapper> CreateUriWrappers(IUniversalFileSystem ufs)
    {
        static UriWrapper CreateUriWrapper(IUniversalFileSystem ufs, string baseUri) => new(baseUri);

        static UriWrapper CreateFileUriWrapper(IUniversalFileSystem ufs)
        {
            string root = $"{Environment.CurrentDirectory}/ufs-integration-test-file";
            return CreateUriWrapper(ufs, $"file://{root}/");
        }

        yield return CreateUriWrapper(ufs, "memory://");
        yield return CreateFileUriWrapper(ufs);
        yield return CreateUriWrapper(ufs, "s3://ufs-integration-test-s3");
        yield return CreateUriWrapper(ufs, "abfss://ufs-integration-test-abfss");
        yield return CreateUriWrapper(ufs, "abfss2://ufs-integration-test-abfss2");
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
                    .AddAzureBlobFileSystem();
            })
            .Build();
        return host.Services.GetRequiredService<IUniversalFileSystem>();
    }
}