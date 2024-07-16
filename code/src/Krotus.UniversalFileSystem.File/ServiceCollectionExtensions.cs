using Krotus.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Krotus.UniversalFileSystem.File;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemImplCreator, FileFileSystemImplCreator>(typeof(FileFileSystemImpl).FullName);
    }
}