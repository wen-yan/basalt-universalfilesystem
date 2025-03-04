using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BasaltHexagons.UniversalFileSystem.AwsS3;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.File;
using BasaltHexagons.UniversalFileSystem.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public abstract class UniversalFileSystemStore
{
    public static IEnumerable<object[]> GetAllUniversalFileSystems()
    {
        yield return [CreateMemoryUniversalFileSystem()];
        yield return [CreateFileUniversalFileSystem()];
        yield return [CreateAwsS3UniversalFileSystem()];
    }

    private static UniversalFileSystemTestWrapper CreateMemoryUniversalFileSystem()
    {
        return CreateUniversalFileSystem(
            builder => { builder.AddInMemoryCollection(new Dictionary<string, string?> { ["Schemes:memory:ImplementationFactoryClass"] = "BasaltHexagons.UniversalFileSystem.Memory.MemoryFileSystemFactory" }); },
            services => services.AddMemoryFileSystem(),
            $"memory://");
    }

    private static UniversalFileSystemTestWrapper CreateFileUniversalFileSystem()
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

        return CreateUniversalFileSystem(
            builder => { builder.AddInMemoryCollection(new Dictionary<string, string?> { ["Schemes:file:ImplementationFactoryClass"] = "BasaltHexagons.UniversalFileSystem.File.FileFileSystemFactory" }); },
            services => services.AddFileFileSystem(),
            $"file://{root}/");
    }

    private static UniversalFileSystemTestWrapper CreateAwsS3UniversalFileSystem()
    {
        return CreateUniversalFileSystem(
            builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Schemes:s3:ImplementationFactoryClass"] = "BasaltHexagons.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory",
                    ["Schemes:s3:Implementation:Client:Credentials:Type"] = "Basic",
                    ["Schemes:s3:Implementation:Client:Credentials:AccessKey"] = "test",
                    ["Schemes:s3:Implementation:Client:Credentials:SecretKey"] = "test",
                    ["Schemes:s3:Implementation:Client:Options:ServiceURL"] = "http://localhost:4566",
                    ["Schemes:s3:Implementation:Client:Options:ForcePathStyle"] = "true",
                    ["Schemes:s3:Implementation:Settings:CreateBucketIfNotExists"] = "true",
                });
            },
            services => services.AddAwsS3FileSystem(),
            $"s3://ufs-integration-test-s3");
    }

    private static UniversalFileSystemTestWrapper CreateUniversalFileSystem(Action<IConfigurationBuilder> configurationBuilder, Func<IServiceCollection, IServiceCollection> servicesBuilder, string baseUri)
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) => configurationBuilder(builder))
            .ConfigureServices((context, services) =>
            {
                servicesBuilder(services)
                    .AddTransient<IUniversalFileSystem>(serviceProvider =>
                    {
                        IConfiguration config = serviceProvider.GetRequiredService<IConfiguration>();
                        return UniversalFileSystemFactory.Create(serviceProvider, config);
                    });
            })
            .Build();

        UniversalFileSystemTestWrapper ufs = new(host, new Uri(baseUri), host.Services.GetRequiredService<IUniversalFileSystem>());

        // delete all files
        List<ObjectMetadata> allFiles = ufs.ListObjectsAsync("", true).ToListAsync().Result;
        foreach (ObjectMetadata file in allFiles)
            ufs.DeleteObjectAsync(file.Path, default).Wait();
        return ufs;
    }
}