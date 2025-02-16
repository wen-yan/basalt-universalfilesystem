using System;
using System.Collections.Generic;

using BasaltHexagons.UniversalFileSystem.File;
using BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.Implementations;

public interface IFileFileSystemTests : IFileSystemMethodTests
{
    IUniversalFileSystem IFileSystemMethodTests.GetUniversalFileSystem()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(static (context, builder) =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UniversalFileSystem:Schemes:file:ImplementationFactoryClass"] = "BasaltHexagons.UniversalFileSystem.File.FileFileSystemFactory"
                });
            })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddFileFileSystem()
                    // UniversalFileSystem
                    .AddTransient<IUniversalFileSystem>(serviceProvider =>
                    {
                        IConfigurationSection config = serviceProvider.GetRequiredService<IConfiguration>().GetSection("UniversalFileSystem");
                        return UniversalFileSystemFactory.Create(serviceProvider, config);
                    });

            })
            .Build();

        string testDir = $"file://{Environment.CurrentDirectory}/it/file/";
        return new MethodTestsUniversalFileSystemWrapper(new Uri(testDir), host.Services.GetRequiredService<IUniversalFileSystem>());
    }
}

[TestClass]
public class FileFileSystem_ListObjectsTests : ListObjectsTests, IFileFileSystemTests
{
}

[TestClass]
public class FileFileSystem_GetObjectMetadataTests : GetObjectMetadataTests, IFileFileSystemTests
{
}
