using System;
using Amazon.S3;
using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AwsS3;

/// <summary>
/// Extension methods of ServiceCollection to add AWS S3 filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add AWS S3 filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddAwsS3FileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedSingleton<IFileSystemFactory, AwsS3FileSystemFactory>(typeof(AwsS3FileSystemFactory).FullName)
            .AddSingleton<IFileSystemFactory>(serviceProvider => serviceProvider.GetRequiredKeyedService<IFileSystemFactory>(typeof(AwsS3FileSystemFactory).FullName));
    }

    /// <summary>
    /// Add AWS S3 custom client to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <param name="name">Name of custom client.</param>
    /// <param name="implementationFactory">Custom client factory.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddAwsS3CustomClient(this IServiceCollection services, string name, Func<IServiceProvider, IAmazonS3> implementationFactory)
    {
        string key = AwsS3FileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<IAmazonS3>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}