using System;
using System.Runtime.CompilerServices;
using Aliyun.OSS;
using Aliyun.OSS.Common.Authentication;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AliyunOss;

enum ClientCredentialType
{
    Basic,
    ConfigJsonProfile,
}

[FileSystemFactoryConfigurationTemplate(
    """
    Oss:
      UriRegexPattern: ^oss://.*$       # Use regex to match different buckets and/or paths
      FileSystemFactoryClass: Basalt.UniversalFileSystem.AliyunOss.AliyunOssFileSystemFactory
      Client:                           # Use custom client if missing
        Endpoint:                       # For example, https://oss-cn-shanghai.aliyuncs.com
        Credentials:
          Type:                         # Basic                    | ConfigJsonProfile
          AccessKey:                    # Required                 | Not Required
          SecretKey:                    # Required                 | Not Required
          SecurityToken:                # Optional, default: null  | Not Required
          Profile:                      # Not Required             | Optional, default: use `current` value in ConfigJsonPath
          ConfigJsonPath:               # Not Required             | Optional, default: ~/.aliyun/config.json
      Settings:
        CreateBucketIfNotExists: false  # Optional, boolean, default: false
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
        string endpoint = clientConfiguration.GetValue<string>("Endpoint");
        ClientCredentialType clientCredentialType = clientConfiguration.GetEnumValue<ClientCredentialType>("Credentials:Type");

        ICredentialsProvider credentialsProvider = clientCredentialType switch
        {
            ClientCredentialType.Basic => this.CreateDefaultCredentialProvider(clientConfiguration),
            ClientCredentialType.ConfigJsonProfile => this.CreateConfigJsonProfileCredentialProvider(clientConfiguration),
            _ => throw new InvalidEnumConfigurationValueException<ClientCredentialType>("Credentials:Type", clientCredentialType),
        };

        return new OssClient(endpoint, credentialsProvider);
    }

    private ICredentialsProvider CreateDefaultCredentialProvider(IConfiguration clientConfiguration)
    {
        string accessKey = clientConfiguration.GetValue<string>("Credentials:AccessKey");
        string secretKey = clientConfiguration.GetValue<string>("Credentials:SecretKey");
        string? securityToken = clientConfiguration.GetValue<string>("Credentials:SecurityToken", () => null);

        return new DefaultCredentialsProvider(new DefaultCredentials(accessKey, secretKey, securityToken));
    }

    private ICredentialsProvider CreateConfigJsonProfileCredentialProvider(IConfiguration clientConfiguration)
    {
        string? profile = clientConfiguration.GetValue<string>("Credentials:Profile", () => null);
        string? configJsonPath = clientConfiguration.GetValue<string>("Credentials:ConfigJsonPath", () => null);
        
        return new AliyunConfigJsonCredentialProvider(profile, configJsonPath);
    }
}