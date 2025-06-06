using System;
using System.Runtime.CompilerServices;
using Aliyun.OSS;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AliyunOss;

enum ClientCredentialType
{
    Basic,
}

[FileSystemFactoryConfigurationTemplate(
    """
    Oss:
      UriRegexPattern: ^oss://.*$       # Use regex to match different buckets and/or paths
      FileSystemFactoryClass: Basalt.UniversalFileSystem.AliyunOss.AliyunOssFileSystemFactory
      Client:                           # Use custom client if missing
        Endpoint:                       # For example, oss-cn-shanghai.aliyuncs.com
        Credentials:
          Type:                         # Basic
          AccessKey:                    # Required when Type = Basic
          SecretKey:                    # Required when Type = Basic
          SecurityToken:                # Type = Basic
      Settings:
        CreateBucketIfNotExists: false  # Optional, boolean, default is false
    """)]
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
            ClientCredentialType.Basic => this.CreateDefaultCredentialClient(clientConfiguration),
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