using System;
using Aliyun.OSS;
using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AliyunOss;

/// <summary>
/// Extension methods of ServiceCollection to add Aliyun OSS filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Aliyun OSS filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddAliyunOssFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, AliyunOssFileSystemFactory>(typeof(AliyunOssFileSystemFactory).FullName)
            .AddSingleton<IFileSystemFactory>(serviceProvider => serviceProvider.GetRequiredKeyedService<IFileSystemFactory>(typeof(AliyunOssFileSystemFactory).FullName));
    }

    /// <summary>
    /// Add Aliyun OSS custom client to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <param name="name">Name of custom client.</param>
    /// <param name="implementationFactory">Custom client factory.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddAliyunOssServiceClient(this IServiceCollection services, string name, Func<IServiceProvider, IOss> implementationFactory)
    {
        string key = AliyunOssFileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<IOss>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}