using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem;

/// <summary>
/// Extension methods of ServiceCollection to add universal filesystem support.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add universal filesystem to service collection.
    /// </summary>
    /// <param name="services">ServiceCollection object.</param>
    /// <param name="configurationRoot">Configuration root.</param>
    /// <returns>ServiceCollection object.</returns>
    public static IServiceCollection AddUniversalFileSystem(this IServiceCollection services, string configurationRoot)
    {
        return services
            .AddSingleton<IFileSystemStore>(serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection(configurationRoot);
                return new DefaultFileSystemStore(serviceProvider, configuration);
            })
            .AddSingleton<IUniversalFileSystem, UniversalFileSystem>();
    }
}