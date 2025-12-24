using System;
using System.Runtime.CompilerServices;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Basalt.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.AwsS3;

enum ClientCredentialType
{
    Basic,                  // BasicAWSCredentials, tested using integration test
    EnvironmentVariables,   // EnvironmentVariablesAWSCredentials
    Profile,                // StoredProfileAWSCredentials
}

[FileSystemFactoryConfigurationTemplate(
    """
    S3:
      UriRegexPattern: ^s3://.*$        # Use regex to match different buckets and/or paths
      FileSystemFactoryClass: Basalt.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory
      Client:                           # Use custom client if missing
        Credentials:
          Type:                         # Basic        | EnvironmentVariables | Profile
          AccessKey:                    # Required     | Not Required         | Not Required
          SecretKey:                    # Required     | Not Required         | Not Required
          Profile:                      # Not Required | Not Required         | Optional, default: `default`
        Options:
          RegionEndpoint:               # Required or ServiceURL
          ServiceURL:                   # Required or RegionEndpoint, for example http://localhost:4566 for LocalStack
          ForcePathStyle:               # Optional, boolean
      Settings:
        CreateBucketIfNotExists: false  # Optional, boolean, default is false
    """)]
[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class AwsS3FileSystemFactory : IFileSystemFactory
{
    public AwsS3FileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfigurationSection configuration)
    {
        string name = configuration.Key;
        IConfigurationSection clientConfig = configuration.GetSection("Client");

        IAmazonS3 client = clientConfig.Exists()
            ? CreateAmazonS3ClientFromConfiguration(clientConfig)
            : this.ServiceProvider.GetRequiredKeyedService<IAmazonS3>(GetCustomClientServiceKey(name));

        return new AwsS3FileSystem(client, configuration.GetSection("Settings"));
    }
    
    internal static string GetCustomClientServiceKey(string name) => $"{typeof(AwsS3FileSystemFactory).FullName!}.CustomS3Client.{name}";

    private static IAmazonS3 CreateAmazonS3ClientFromConfiguration(IConfiguration clientConfiguration)
    {
        // credentials
        ClientCredentialType clientCredentialType = clientConfiguration.GetEnumValue<ClientCredentialType>("Credentials:Type");

        AWSCredentials credentials = clientCredentialType switch
        {
            ClientCredentialType.Basic => CreateBasicAWSCredentials(clientConfiguration),
            ClientCredentialType.EnvironmentVariables => new EnvironmentVariablesAWSCredentials(),
            ClientCredentialType.Profile => CreateStoredProfileAWSCredentials(clientConfiguration),
            _ => throw new InvalidEnumConfigurationValueException<ClientCredentialType>("Credentials:Type", clientCredentialType),
        };

        // config
        AmazonS3Config config = new();
        string? regionEndpoint = clientConfiguration.GetValue<string>("Options:RegionEndpoint", () => null);
        string? serviceUrl = clientConfiguration.GetValue<string>("Options:ServiceURL", () => null);
        bool? forcePathStyle = clientConfiguration.GetBoolValue("Options:ForcePathStyle", () => null);

        if (regionEndpoint != null) config.RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint);
        if (serviceUrl != null) config.ServiceURL = serviceUrl;
        if (forcePathStyle != null) config.ForcePathStyle = forcePathStyle.Value;

        return new AmazonS3Client(credentials, config);
    }

    // Create client
    private static AWSCredentials CreateBasicAWSCredentials(IConfiguration implementationConfiguration)
    {
        string accessKey = implementationConfiguration.GetValue<string>("Credentials:AccessKey");
        string secretKey = implementationConfiguration.GetValue<string>("Credentials:SecretKey");
        return new BasicAWSCredentials(accessKey, secretKey);
    }

    private static AWSCredentials CreateStoredProfileAWSCredentials(IConfiguration implementationConfiguration)
    {
        string profile = implementationConfiguration.GetValue<string>("Credentials:Profile", () => SharedCredentialsFile.DefaultProfileName)!;

        SharedCredentialsFile credentialsFile = new SharedCredentialsFile();
        CredentialProfile? credentialProfile = credentialsFile.TryGetProfile(profile, out CredentialProfile value) ? value : null;

        if (credentialProfile == null)
            throw new InvalidConfigurationValueException("Credentials:Profile", profile);

        return credentialProfile.GetAWSCredentials(credentialsFile);
    }
}