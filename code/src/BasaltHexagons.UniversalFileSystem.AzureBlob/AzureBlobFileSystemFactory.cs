using System;

using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;

using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AzureBlob;

enum ImplementationConfigurationType
{
    // BlobServiceClient instance is provided by keyed service from IServiceProvider, using key `AzureBlobFileSystemFactory.CustomClientServiceKey`
    Custom,

    // Use configuration to create client instance
    Configuration,
}

enum CredentialType
{
    Default,    // DefaultAzureCredential
    SharedKey,  // StorageSharedKeyCredential
}

/// <summary>
/// ImplementationConfiguration
///     Type: Custom/Configuration
///     Credentials
///         Type: Default/SharedKey
///         AccountName:     # Type = SharedKey
///         AccountKey:      # Tppe = SharedKey
///     Config:
///         ServiceUri: 
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
        Exception InvalidConfigurationTypeException(string? configurationTypeStr)
        {
            return new ApplicationException($"Unknown Azure blob file system implementation configuration type [{configurationTypeStr ?? "<null>"}], valid values are [{string.Join(", ", Enum.GetNames<ImplementationConfigurationType>())}]");
        }

        string? configurationTypeStr = implementationConfiguration["Type"];
        if (!Enum.TryParse(configurationTypeStr, true, out ImplementationConfigurationType configurationType))
        {
            throw InvalidConfigurationTypeException(configurationTypeStr);
        }

        BlobServiceClient client = configurationType switch
        {
            ImplementationConfigurationType.Custom => this.ServiceProvider.GetRequiredKeyedService<BlobServiceClient>(CustomClientServiceKey),
            ImplementationConfigurationType.Configuration => this.CreateBlobServiceClientFromConfiguration(implementationConfiguration),
            _ => throw InvalidConfigurationTypeException(configurationTypeStr),
        };

        return new AzureBlobFileSystem(client);
    }

    private BlobServiceClient CreateBlobServiceClientFromConfiguration(IConfiguration implementationConfiguration)
    {
        Exception InvalidCredentialsTypeException(string? credentialsTypeStr)
        {
            return new ApplicationException($"Unkown credentials type [{credentialsTypeStr ?? "<null>"}], valid values are [{string.Join(", ", Enum.GetNames<CredentialType>())}]");
        }

        string GetServiceUri() => implementationConfiguration.GetValue<string>("Config:ServiceUri", () => throw new ApplicationException("ServiceUri is not set"));

        BlobServiceClient CreateDefaultCredentailClient()
        {
            string serviceUri = GetServiceUri();
            return new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential());
        }

        BlobServiceClient CreateSharedKeyCredentailClient()
        {
            string serviceUri = GetServiceUri();
            string accountName = implementationConfiguration.GetValue<string>("Credentials:AccountName", () => throw new ApplicationException("AccountName is not set"));
            string accountKey = implementationConfiguration.GetValue<string>("Credentials:AccountKey", () => throw new ApplicationException("AccountKey is not set"));
            return new BlobServiceClient(new Uri(serviceUri), new StorageSharedKeyCredential(accountName, accountKey));
        }

        string? credentialsTypeStr = implementationConfiguration["Credentials:Type"];
        if (!Enum.TryParse(credentialsTypeStr, true, out CredentialType credentialsType))
        {
            throw InvalidCredentialsTypeException(credentialsTypeStr);
        }

        BlobServiceClient client = credentialsType switch
        {
            CredentialType.Default => CreateDefaultCredentailClient(),
            CredentialType.SharedKey => CreateSharedKeyCredentailClient(),
            _ => throw InvalidCredentialsTypeException(credentialsTypeStr),
        };
        return client;
    }
}
