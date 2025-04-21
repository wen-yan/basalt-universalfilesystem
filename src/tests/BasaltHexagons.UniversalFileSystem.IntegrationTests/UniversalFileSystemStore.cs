using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

        yield return CreateMemoryUniversalFileSystem(ufs);
        yield return CreateFileUniversalFileSystem(ufs);
        yield return CreateAwsS3UniversalFileSystem(ufs);
        yield return CreateAzureBlobUniversalFileSystem(ufs);
        yield return CreateAzureBlob2UniversalFileSystem(ufs);
    }

    private static UniversalFileSystemTestWrapper CreateMemoryUniversalFileSystem(IUniversalFileSystem ufs) => CreateUniversalFileSystemTestWrapper(ufs, "memory://");

    private static UniversalFileSystemTestWrapper CreateFileUniversalFileSystem(IUniversalFileSystem ufs)
    {
        string root = $"{Environment.CurrentDirectory}/ufs-integration-test-file";

        // Delete all files
        if (Directory.Exists(root))
        {
            foreach (string file in Directory.GetFiles(root))
            {
                System.IO.File.Delete(file);
            }

            // Delete all subdirectories and their contents
            foreach (string subDirectory in Directory.GetDirectories(root))
            {
                Directory.Delete(subDirectory, true); // true for recursive deletion
            }
        }

        return CreateUniversalFileSystemTestWrapper(ufs, $"file://{root}/");
    }

    private static UniversalFileSystemTestWrapper CreateAwsS3UniversalFileSystem(IUniversalFileSystem ufs)
        => CreateUniversalFileSystemTestWrapper(ufs, "s3://ufs-integration-test-s3");

    private static UniversalFileSystemTestWrapper CreateAzureBlobUniversalFileSystem(IUniversalFileSystem ufs)
        => CreateUniversalFileSystemTestWrapper(ufs, "abfss://ufs-integration-test-abfss");
    
    private static UniversalFileSystemTestWrapper CreateAzureBlob2UniversalFileSystem(IUniversalFileSystem ufs)
        => CreateUniversalFileSystemTestWrapper(ufs, "abfss2://ufs-integration-test-abfss");

    private static UniversalFileSystemTestWrapper CreateUniversalFileSystemTestWrapper(IUniversalFileSystem ufs, string baseUri)
    {
        UniversalFileSystemTestWrapper wrapper = new(ufs, new Uri(baseUri));

        // delete all files
        List<ObjectMetadata> allFiles = wrapper.ListObjectsAsync("", true).ToListAsync().Result;
        foreach (ObjectMetadata file in allFiles)
            ufs.DeleteFileAsync(file.Uri, CancellationToken.None).Wait();

        return wrapper;
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