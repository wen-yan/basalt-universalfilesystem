using BasaltHexagons.UniversalFileSystem.AwsS3;
using BasaltHexagons.UniversalFileSystem.AzureBlob;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.File;
using BasaltHexagons.UniversalFileSystem.Memory;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public abstract class UniversalFileSystemStore
{
    public static IEnumerable<object[]> GetSingleUniversalFileSystem() => GetUniversalFileSystemWrappers()
        .Select(x => new object[] { x });

    private static IEnumerable<UniversalFileSystemTestWrapper> GetUniversalFileSystemWrappers(IUniversalFileSystem? ufs = null)
    {
        ufs ??= CreateUniversalFileSystem();

        List<UriWrapper> uriWrappers = CreateUriWrappers(ufs).ToList();
        return uriWrappers.Select(x => new UniversalFileSystemTestWrapper(ufs, x));
    }

    private static IEnumerable<UriWrapper> CreateUriWrappers(IUniversalFileSystem ufs)
    {
        static UriWrapper CreateUriWrapper(IUniversalFileSystem ufs, string baseUri)
        {
            UriWrapper uriWrapper = new(baseUri);
            // delete all files
            List<ObjectMetadata> allFiles = ufs.ListObjectsAsync(uriWrapper.Apply(""), true, CancellationToken.None).ToListAsync().Result;
            foreach (ObjectMetadata file in allFiles)
                ufs.DeleteFileAsync(file.Uri, CancellationToken.None).Wait();
            return uriWrapper;
        }

        static UriWrapper CreateFileUriWrapper(IUniversalFileSystem ufs)
        {
            string root = $"{Environment.CurrentDirectory}/ufs-integration-test-file";

            // Delete all files
            if (Directory.Exists(root))
            {
                foreach (string file in Directory.GetFiles(root))
                    System.IO.File.Delete(file);

                // Delete all subdirectories and their contents
                foreach (string subDirectory in Directory.GetDirectories(root))
                    Directory.Delete(subDirectory, true); // true for recursive deletion
            }

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