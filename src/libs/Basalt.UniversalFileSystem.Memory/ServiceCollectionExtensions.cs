using Basalt.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.Memory;

/// <summary>
/// Extension methods of ServiceCollection to add memory filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add memory filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddMemoryFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, MemoryFileSystemFactory>(typeof(MemoryFileSystemFactory).FullName);
    }
}