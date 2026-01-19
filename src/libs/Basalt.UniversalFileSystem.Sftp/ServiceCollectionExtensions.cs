using System;
using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;

namespace Basalt.UniversalFileSystem.Sftp;

/// <summary>
/// Extension methods of ServiceCollection to add SFTP filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add SFTP filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddSftpFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, SftpFileSystemFactory>(typeof(SftpFileSystemFactory).FullName);
    }

    /// <summary>
    /// Add SFTP custom client to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <param name="name">Name of custom client.</param>
    /// <param name="implementationFactory">Custom client factory.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddSftpCustomClient(this IServiceCollection services, string name, Func<IServiceProvider, SftpClient> implementationFactory)
    {
        string key = SftpFileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<SftpClient>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}