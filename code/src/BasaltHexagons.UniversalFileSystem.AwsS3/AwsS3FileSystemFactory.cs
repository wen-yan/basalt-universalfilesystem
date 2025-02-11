using System;

using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;

using BasaltHexagons.UniversalFileSystem.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.AwsS3;

enum ImplementationConfigurationType
{
    // IAmazonS3 instance is provided by keyed service from IServiceProvider, using key `AwsS3FileSystemFactory.CustomClientServiceKey`
    Custom,

    // Use configuration to create IAmazonS3 instance
    Configuration,
}

enum CredentialType
{
    Anonymous,                  // AnonymousAWSCredentials
    Basic,                      // BasicAWSCredentials
    EnvironmentVariables,       // EnvironmentVariablesAWSCredentials
    Profile,                    // StoredProfileAWSCredentials
}

/// <summary>
/// ImplementationConfiguration
///     Type: Custom/Configuration
///     Credentials
///         Type: Anonymous/Basic/EnvironmentVariables/Profile
///         AccessKey: <access-key>     # Type = Basic
///         SecretKey: <secret-key>     # Type = Basic
///         ProfileName: <profile-name> # Type = Profile
///     Config
///         RegionEndpoint:
///         ServiceURL:
///         ForcePathStyle: true/false
/// </summary>
class AwsS3FileSystemFactory : IFileSystemFactory
{
    public const string CustomClientServiceKey = "BasaltHexagons.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory.CustomAmazonS3Client";

    public AwsS3FileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfiguration implementationConfiguration)
    {
        Exception InvalidConfigurationTypeException(string? configurationTypeStr)
        {
            return new ApplicationException($"Unknown S3 file system implementation configuration type [{configurationTypeStr ?? "<null>"}], valid values are [{string.Join(", ", Enum.GetNames<ImplementationConfigurationType>())}]");
        }

        string? configurationTypeStr = implementationConfiguration["Type"];
        if (!Enum.TryParse(configurationTypeStr, true, out ImplementationConfigurationType configurationType))
        {
            throw InvalidConfigurationTypeException(configurationTypeStr);
        }

        IAmazonS3 amazonS3Client = configurationType switch
        {
            ImplementationConfigurationType.Custom => this.ServiceProvider.GetRequiredKeyedService<IAmazonS3>(CustomClientServiceKey),
            ImplementationConfigurationType.Configuration => this.CreateAmazonS3ClientFromConfiguration(implementationConfiguration),
            _ => throw InvalidConfigurationTypeException(configurationTypeStr),
        };

        return new AwsS3FileSystem(amazonS3Client);
    }

    private IAmazonS3 CreateAmazonS3ClientFromConfiguration(IConfiguration implementationConfiguration)
    {
        AWSCredentials CreateBasicAWSCredentials()
        {
            string? accessKey = implementationConfiguration["Credentials:AccessKey"];
            string? secretKey = implementationConfiguration["Credentials:SecretKey"];
            return new BasicAWSCredentials(accessKey, secretKey);
        }

        AWSCredentials CreateStoredProfileAWSCredentials()
        {
            string? profileName = implementationConfiguration["Credentials:ProfileName"];

            SharedCredentialsFile credentialsFile = new SharedCredentialsFile();
            CredentialProfile? credentialProfile = profileName == null
                ? null
                : credentialsFile.TryGetProfile(profileName, out CredentialProfile value) ? value : null;

            if (credentialProfile == null)
            {
                throw new ApplicationException($"Unknown profile name [{profileName}]");
            }

            return credentialProfile.GetAWSCredentials(credentialsFile);
        }

        Exception InvalidCredentialsTypeException(string? credentialsTypeStr)
        {
            return new ApplicationException($"Unkown S3 file system credentials type [{credentialsTypeStr ?? "<null>"}], valid values are [{string.Join(", ", Enum.GetNames<CredentialType>())}]");
        }

        // credentials
        string? credentialsTypeStr = implementationConfiguration["Credentials:Type"];
        if (!Enum.TryParse(credentialsTypeStr, true, out CredentialType credentialsType))
        {
            throw InvalidCredentialsTypeException(credentialsTypeStr);
        }

        AWSCredentials credentials = credentialsType switch
        {
            CredentialType.Anonymous => new AnonymousAWSCredentials(),
            CredentialType.Basic => CreateBasicAWSCredentials(),
            CredentialType.EnvironmentVariables => new EnvironmentVariablesAWSCredentials(),
            CredentialType.Profile => CreateStoredProfileAWSCredentials(),
            _ => throw InvalidCredentialsTypeException(credentialsTypeStr),
        };

        // config
        AmazonS3Config config = new AmazonS3Config();
        string? regionEndpoint = implementationConfiguration["Config:RegionEndpoint"];
        string? serviceURL = implementationConfiguration["Config:ServiceURL"];
        string? forcePathStyleStr = implementationConfiguration["Config:ForcePathStyle"];

        if (regionEndpoint != null) config.RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpoint);
        if (serviceURL != null) config.ServiceURL = serviceURL;
        if (forcePathStyleStr != null)
        {
            if (!bool.TryParse(forcePathStyleStr, out bool forcePathStyle))
            {
                throw new ApplicationException($"Unknown force path style [{forcePathStyleStr}], valid values are [true, false or optional]");
            }
            config.ForcePathStyle = forcePathStyle;
        }

        return new AmazonS3Client(credentials, config);
    }
}
