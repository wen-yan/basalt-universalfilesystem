using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.File;

/// <summary>
/// Extension methods of ServiceCollection to add local filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add local filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddFileFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, FileFileSystemFactory>(typeof(FileFileSystemFactory).FullName);
    }
}