using System;
using System.Collections.Generic;
using System.Linq;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public abstract class UniversalFileSystemStore
{
    public static IEnumerable<object[]> GetAllUniversalFileSystems()
    {
        yield return [CreateFileUniversalFileSystem()];
    }

    private static IUniversalFileSystem CreateFileUniversalFileSystem()
    {
        return CreateUniversalFileSystem(
            builder => { builder.AddInMemoryCollection(new Dictionary<string, string?> { ["UniversalFileSystem:Schemes:file:ImplementationFactoryClass"] = "BasaltHexagons.UniversalFileSystem.File.FileFileSystemFactory" }); },
            services => services.AddFileFileSystem(),
            $"file://{Environment.CurrentDirectory}/IntegrationTests/file/");
    }

    private static IUniversalFileSystem CreateUniversalFileSystem(Action<IConfigurationBuilder> configurationBuilder, Func<IServiceCollection, IServiceCollection> servicesBuilder, string baseUri)
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) => configurationBuilder(builder))
            .ConfigureServices((context, services) =>
            {
                servicesBuilder(services)
                    .AddTransient<IUniversalFileSystem>(serviceProvider =>
                    {
                        IConfigurationSection config = serviceProvider.GetRequiredService<IConfiguration>().GetSection("UniversalFileSystem");
                        return UniversalFileSystemFactory.Create(serviceProvider, config);
                    });
            })
            .Build();

        IUniversalFileSystem ufs = new MethodTestsUniversalFileSystemWrapper(host, new Uri(baseUri), host.Services.GetRequiredService<IUniversalFileSystem>());

        // delete all files
        List<ObjectMetadata> allFiles = ufs.ListObjectsAsync("", true, default).ToListAsync().Result;
        foreach(ObjectMetadata file in allFiles)
            ufs.DeleteObjectAsync(file.Path, default).Wait();
        return ufs;
    }
}