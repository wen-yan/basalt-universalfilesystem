using System;
using Amazon.S3;
using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AwsS3;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAwsS3FileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedSingleton<IFileSystemFactory, AwsS3FileSystemFactory>(typeof(AwsS3FileSystemFactory).FullName)
            .AddSingleton<IFileSystemFactory>(serviceProvider => serviceProvider.GetRequiredKeyedService<IFileSystemFactory>(typeof(AwsS3FileSystemFactory).FullName));
    }

    public static IServiceCollection AddAwsS3CustomClient(this IServiceCollection services, string name, Func<IServiceProvider, IAmazonS3> implementationFactory)
    {
        string key = AwsS3FileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<IAmazonS3>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}