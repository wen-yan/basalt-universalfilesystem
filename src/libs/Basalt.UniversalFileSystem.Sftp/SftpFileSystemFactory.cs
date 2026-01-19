using System;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;

namespace Basalt.UniversalFileSystem.Sftp;

[FileSystemFactoryConfigurationTemplate(
    """
    Sftp:
      UriRegexPattern: ^sftp://.*$       # Use regex to match different server and/or paths
      FileSystemFactoryClass: Basalt.UniversalFileSystem.Sftp.SftpFileSystemFactory
      Client:                           # Use custom client if missing
        Host:                           # Required, connection host name or address
        Port:                           # Required, connection host port
        Username:                       # Required, authentication username
        Password:                       # Required, authentication password
    """)]
class SftpFileSystemFactory : IFileSystemFactory
{
    private const int DefaultPort = 22;

    public SftpFileSystemFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    private IServiceProvider ServiceProvider { get; }

    public IFileSystem Create(IConfigurationSection configuration)
    {
        string name = configuration.Key;
        IConfigurationSection clientConfig = configuration.GetSection("Client");

        SftpClient client = clientConfig.Exists()
            ? CreateSftpClientFromConfiguration(clientConfig)
            : this.ServiceProvider.GetRequiredKeyedService<SftpClient>(GetCustomClientServiceKey(name));

        return new SftpFileSystem(client);
    }

    internal static string GetCustomClientServiceKey(string name) => $"{typeof(SftpFileSystemFactory).FullName!}.CustomSftpClient.{name}";

    private SftpClient CreateSftpClientFromConfiguration(IConfiguration clientConfiguration)
    {
        string host = clientConfiguration.GetValue<string>("Host");
        int port = clientConfiguration.GetValue<int>("Port", () => DefaultPort);
        string username = clientConfiguration.GetValue<string>("Username");
        string password = clientConfiguration.GetValue<string>("Password");
        SftpClient client = new SftpClient(host, port, username, password);
        client.Connect();
        return client;
    }
}