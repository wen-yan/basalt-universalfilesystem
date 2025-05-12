using System;
using Aliyun.OSS;
using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AliyunOss;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAliyunOssFileSystem(this IServiceCollection services)
    {
        return services
            .AddKeyedTransient<IFileSystemFactory, AliyunOssFileSystemFactory>(typeof(AliyunOssFileSystemFactory).FullName);
    }

    public static IServiceCollection AddAliyunOssServiceClient(this IServiceCollection services, string name, Func<IServiceProvider, IOss> implementationFactory)
    {
        string key = AliyunOssFileSystemFactory.GetCustomClientServiceKey(name);
        return services
            .AddKeyedTransient<IOss>(key, (serviceProvider, serviceKey) => implementationFactory(serviceProvider));
    }
}