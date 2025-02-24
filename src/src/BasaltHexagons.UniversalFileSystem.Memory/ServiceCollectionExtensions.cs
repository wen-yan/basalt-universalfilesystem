using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.Memory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, MemoryFileSystemFactory>(typeof(MemoryFileSystemFactory).FullName);
    }
}