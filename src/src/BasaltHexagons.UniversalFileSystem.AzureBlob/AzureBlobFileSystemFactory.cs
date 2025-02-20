using System;

using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;

using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AzureBlob;

enum ClientCredentialType
{
    Default, // DefaultAzureCredential
    SharedKey, // StorageSharedKeyCredential
}

/// <summary>
/// ImplementationConfiguration
///     Client:                  # if not exists, get it from depedency injection
///         ServiceUri: 
///         Credentials
///             Type: Default/SharedKey
///             AccountName:     # Type = SharedKey
///             AccountKey:      # Type = SharedKey
/// </summary>
class AzureBlobFileSystemFactory : IFileSystemFactory
{
    public const string CustomClientServiceKey = "BasaltHexagons.UniversalFileSystem.AzureBlob.AzureBlobFileSystemFactory.CustomBlobServiceClient";

    public AzureBlobFileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfiguration implementationConfiguration)
    {
        IConfigurationSection clientConfig = implementationConfiguration.GetSection("Client");

        BlobServiceClient client =  clientConfig.Exists()
            ? this.CreateBlobServiceClientFromConfiguration(clientConfig)
            : this.ServiceProvider.GetRequiredKeyedService<BlobServiceClient>(CustomClientServiceKey);

        return new AzureBlobFileSystem(client);
    }

    private BlobServiceClient CreateBlobServiceClientFromConfiguration(IConfiguration implementationConfiguration)
    {
        ClientCredentialType clientCredentialType = implementationConfiguration.GetEnumValue<ClientCredentialType>("Credentials:Type");

        BlobServiceClient client = clientCredentialType switch
        {
            ClientCredentialType.Default => CreateDefaultCredentialClient(implementationConfiguration),
            ClientCredentialType.SharedKey => CreateSharedKeyCredentialClient(implementationConfiguration),
            _ => throw new ConfigurationException($"Unknown client credential type [{clientCredentialType}]"),
        };
        return client;
    }

    // Create client
    private BlobServiceClient CreateDefaultCredentialClient(IConfiguration implementationConfiguration)
    {
        string serviceUri = GetServiceUri(implementationConfiguration);
        return new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
    }

    private BlobServiceClient CreateSharedKeyCredentialClient(IConfiguration implementationConfiguration)
    {
        string serviceUri = GetServiceUri(implementationConfiguration);
        string accountName = implementationConfiguration.GetValue<string>("Credentials:AccountName");
        string accountKey = implementationConfiguration.GetValue<string>("Credentials:AccountKey");
        return new BlobServiceClient(new Uri(serviceUri), new StorageSharedKeyCredential(accountName, accountKey));
    }

    private string GetServiceUri(IConfiguration implementationConfiguration) => implementationConfiguration.GetValue<string>("ServiceUri");
}