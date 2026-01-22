using System;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.GoogleCloudStorage;

enum ClientCredentialType
{
    None,
    ApplicationDefault,
    FromFile,
    FromJson,
    FromAccessToken,
}

[FileSystemFactoryConfigurationTemplate(
    """
    Gs:
      UriRegexPattern: ^gs://.*$        # Use regex to match different buckets and/or paths
      FileSystemFactoryClass: Basalt.UniversalFileSystem.GoogleCloudStorage.GoogleCloudStorageFileSystemFactory
      Client:                           # Use custom client if missing
        Credentials:
          Type:                         # None         | ApplicationDefault | FromFile     | FromJson     | FromAccessToken
          CredentialFilePath:           # Not Required | Not Required       | Required     | Not Required | Not Required
          CredentialJson:               # Not Required | Not Required       | Not Required | Required     | Not Required
          AccessToken:                  # Not Required | Not Required       | Not Required | Not Required | Required
        Uri:                            # Required for emulator
      Settings:
        ProjectId:                      # Required
        CreateBucketIfNotExists: false  # Optional, boolean, default is false
    """
)]
class GoogleCloudStorageFileSystemFactory : IFileSystemFactory
{
    public GoogleCloudStorageFileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfigurationSection configuration)
    {
        string name = configuration.Key;
        IConfigurationSection clientConfig = configuration.GetSection("Client");

        StorageClient client = clientConfig.Exists()
            ? CreateStorageClientFromConfiguration(clientConfig)
            : this.ServiceProvider.GetRequiredKeyedService<StorageClient>(GetCustomClientServiceKey(name));

        return new GoogleCloudStorageFileSystem(client, configuration.GetSection("Settings"));
    }

    internal static string GetCustomClientServiceKey(string name) => $"{typeof(GoogleCloudStorageFileSystemFactory).FullName!}.CustomStorageClient.{name}";

    private static StorageClient CreateStorageClientFromConfiguration(IConfiguration clientConfiguration)
    {
        // credentials
        ClientCredentialType clientCredentialType = clientConfiguration.GetEnumValue<ClientCredentialType>("Credentials:Type");

        GoogleCredential? credential = clientCredentialType switch
        {
            ClientCredentialType.None => null,
            ClientCredentialType.ApplicationDefault => GoogleCredential.GetApplicationDefault(),
            ClientCredentialType.FromFile => CreateFromFileCredential(clientConfiguration),
            ClientCredentialType.FromJson => CreateFromJsonCredential(clientConfiguration),
            ClientCredentialType.FromAccessToken => CreateFromAccessTokenCredential(clientConfiguration),
            _ => throw new InvalidEnumConfigurationValueException<ClientCredentialType>("Credentials:Type", clientCredentialType),
        };

        string? uri = clientConfiguration.GetValue<string>("Uri", () => null);

        return new StorageClientBuilder()
        {
            UnauthenticatedAccess = credential == null,
            GoogleCredential = credential,
            BaseUri = uri,
        }.Build();
    }

    private static GoogleCredential CreateFromFileCredential(IConfiguration clientConfiguration)
    {
        string credentialFilePath = clientConfiguration.GetValue<string>("Credential:CredentialFilePath");
        return GoogleCredential.FromFile(credentialFilePath);
    }

    private static GoogleCredential CreateFromJsonCredential(IConfiguration clientConfiguration)
    {
        string credentialJson = clientConfiguration.GetValue<string>("Credential:CredentialJson");
        return GoogleCredential.FromJson(credentialJson);
    }

    private static GoogleCredential CreateFromAccessTokenCredential(IConfiguration clientConfiguration)
    {
        string accessToken = clientConfiguration.GetValue<string>("Credential:AccessToken");
        return GoogleCredential.FromAccessToken(accessToken);
    }
}