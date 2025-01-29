using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.File;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, FileFileSystemFactory>(typeof(FileFileSystem).FullName);
    }
}