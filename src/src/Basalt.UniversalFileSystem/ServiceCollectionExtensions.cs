using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem;

public static class ServiceCollectionExtensions
{
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