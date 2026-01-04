using System;
using Azure.Storage.Blobs;
using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AzureBlob;

/// <summary>
/// Extension methods of ServiceCollection to add Azure blob filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Azure blob filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddAzureBlobFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, AzureBlobFileSystemFactory>(typeof(AzureBlobFileSystemFactory).FullName);
    }

    /// <summary>
    /// Add Azure blob custom client to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <param name="name">Name of custom client.</param>
    /// <param name="implementationFactory">Custom client factory.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddAzureBlobCustomClient(this IServiceCollection services, string name, Func<IServiceProvider, BlobServiceClient> implementationFactory)
    {
        string key = AzureBlobFileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<BlobServiceClient>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}