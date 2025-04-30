using BasaltHexagons.UniversalFileSystem.Core;

using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AzureBlob;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureBlobFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedSingleton<IFileSystemFactory, AzureBlobFileSystemFactory>(typeof(AzureBlobFileSystemFactory).FullName);
    }
}
