using System;
using System.Runtime.CompilerServices;
using Aliyun.OSS;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Configuration;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AliyunOss;

enum ClientCredentialType
{
    Default,
}

/// <summary>
/// UriRegexPattern: ^oss://.*$
/// FileSystemFactoryClass: BasaltHexagons.UniversalFileSystem.AliyunOss.AliyunOssFileSystemFactory
/// Client:     # use custom client if missing
///     Endpoint:
///     Credentials:
///         Type: Default
///         AccessKey:     # Type = Default
///         SecretKey:     # Type = Default
///         SecurityToken: # Type = Default
/// Settings:
///     CreateBucketIfNotExists: false
/// </summary>
[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class AliyunOssFileSystemFactory : IFileSystemFactory
{
    public AliyunOssFileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfigurationSection configuration)
    {
        string name = configuration.Key;
        IConfigurationSection clientConfig = configuration.GetSection("Client");

        IOss client = clientConfig.Exists()
            ? this.CreateOssClientFromConfiguration(clientConfig)
            : this.ServiceProvider.GetRequiredKeyedService<IOss>(GetCustomClientServiceKey(name));

        return new AliyunOssFileSystem(client, configuration.GetSection("Settings"));
    }
    
    internal static string GetCustomClientServiceKey(string name) => $"{typeof(AliyunOssFileSystemFactory).FullName!}.CustomOssClient.{name}";

    private IOss CreateOssClientFromConfiguration(IConfiguration clientConfiguration)
    {
        ClientCredentialType clientCredentialType = clientConfiguration.GetEnumValue<ClientCredentialType>("Credentials:Type");

        IOss client = clientCredentialType switch
        {
            ClientCredentialType.Default => this.CreateDefaultCredentialClient(clientConfiguration),
            _ => throw new InvalidEnumConfigurationValueException<ClientCredentialType>("Credentials:Type", clientCredentialType),
        };
        return client;
    }

    private IOss CreateDefaultCredentialClient(IConfiguration clientConfiguration)
    {
        string endpoint = clientConfiguration.GetValue<string>("Endpoint");
        string accessKey = clientConfiguration.GetValue<string>("Credentials:AccessKey");
        string secretKey = clientConfiguration.GetValue<string>("Credentials:SecretKey");
        string? securityToken = clientConfiguration.GetValue<string>("Credentials:SecurityToken", () => null);

        return new OssClient(endpoint, accessKey, secretKey, securityToken);
    }
}