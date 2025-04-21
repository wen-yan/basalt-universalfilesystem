using System;
using System.Runtime.CompilerServices;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Configuration;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AzureBlob;

enum ClientCredentialType
{
    Default, // DefaultAzureCredential
    SharedKey, // StorageSharedKeyCredential
}

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
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
            ClientCredentialType.Default => CreateDefaultCredentialClient(implementationConfiguration),
            ClientCredentialType.SharedKey => CreateSharedKeyCredentialClient(implementationConfiguration),
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