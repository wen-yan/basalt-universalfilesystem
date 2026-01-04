using System;
using System.Runtime.CompilerServices;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AzureBlob;

enum ClientCredentialType
{
    DefaultAzure,       // DefaultAzureCredential
    StorageSharedKey,   // StorageSharedKeyCredential
}

[FileSystemFactoryConfigurationTemplate(
    """
    AzureBlob:
      UriRegexPattern: ^abfss://.*$     # Use regex to match different buckets and/or paths
      FileSystemFactoryClass: Basalt.UniversalFileSystem.AzureBlob.AzureBlobFileSystemFactory
      Client:                           # Use custom client if missing
        ServiceUri:                     # For example, http://localhost:10000/account1
        Credentials:
          Type:                         # DefaultAzure | StorageSharedKey
          AccountName:                  # Not Required | Required
          AccountKey:                   # Not Required | Required
      Settings:
        CreateBlobContainerIfNotExists: false   # Optional, boolean, default is false
    """)]
class AzureBlobFileSystemFactory : IFileSystemFactory
{
    public AzureBlobFileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfigurationSection configuration)
    {
        string name = configuration.Key;
        IConfigurationSection clientConfig = configuration.GetSection("Client");

        BlobServiceClient client = clientConfig.Exists()
            ? CreateBlobServiceClientFromConfiguration(clientConfig)
            : this.ServiceProvider.GetRequiredKeyedService<BlobServiceClient>(GetCustomClientServiceKey(name));

        return new AzureBlobFileSystem(client, configuration.GetSection("Settings"));
    }

    internal static string GetCustomClientServiceKey(string name) => $"{typeof(AzureBlobFileSystemFactory).FullName!}.CustomBlobServiceClient.{name}";

    private static BlobServiceClient CreateBlobServiceClientFromConfiguration(IConfiguration implementationConfiguration)
    {
        ClientCredentialType clientCredentialType = implementationConfiguration.GetEnumValue<ClientCredentialType>("Credentials:Type");

        BlobServiceClient client = clientCredentialType switch
        {
            ClientCredentialType.DefaultAzure => CreateDefaultCredentialClient(implementationConfiguration),
            ClientCredentialType.StorageSharedKey => CreateSharedKeyCredentialClient(implementationConfiguration),
            _ => throw new InvalidEnumConfigurationValueException<ClientCredentialType>("Credentials:Type", clientCredentialType),
        };
        return client;
    }

    // Create client
    private static BlobServiceClient CreateDefaultCredentialClient(IConfiguration implementationConfiguration)
    {
        string serviceUri = implementationConfiguration.GetValue<string>("ServiceUri");
        return new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
    }

    private static BlobServiceClient CreateSharedKeyCredentialClient(IConfiguration implementationConfiguration)
    {
        string serviceUri = implementationConfiguration.GetValue<string>("ServiceUri");
        string accountName = implementationConfiguration.GetValue<string>("Credentials:AccountName");
        string accountKey = implementationConfiguration.GetValue<string>("Credentials:AccountKey");
        return new BlobServiceClient(new Uri(serviceUri), new StorageSharedKeyCredential(accountName, accountKey));
    }
}