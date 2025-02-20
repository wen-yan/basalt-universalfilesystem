

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
            .AddKeyedTransient<IFileSystemFactory, AwsS3FileSystemFactory>(typeof(AwsS3FileSystemFactory).FullName);
    }

    public static IServiceCollection AddAmazonS3Client(this IServiceCollection services, Func<IServiceProvider, IAmazonS3> implementationFactory)
    {
        return services
            .AddKeyedTransient<IAmazonS3>(AwsS3FileSystemFactory.CustomClientServiceKey,
                (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}
