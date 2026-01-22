using System;
using Basalt.UniversalFileSystem.Core;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.GoogleCloudStorage;

/// <summary>
/// Extension methods of ServiceCollection to add AWS S3 filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Google Cloud Storage filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddGoogleCloudStorageFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, GoogleCloudStorageFileSystemFactory>(typeof(GoogleCloudStorageFileSystemFactory).FullName);
    }

    /// <summary>
    /// Add Google Cloud Storage custom client to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <param name="name">Name of custom client.</param>
    /// <param name="implementationFactory">Custom client factory.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddGoogleCloudStorageCustomClient(this IServiceCollection services, string name, Func<IServiceProvider, StorageClient> implementationFactory)
    {
        string key = GoogleCloudStorageFileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<StorageClient>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}