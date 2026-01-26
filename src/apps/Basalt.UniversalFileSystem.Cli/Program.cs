using System;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.UniversalFileSystem.AliyunOss;
using Basalt.UniversalFileSystem.AwsS3;
using Basalt.UniversalFileSystem.AzureBlob;
using Basalt.UniversalFileSystem.Cli.Bootstrap;
using Basalt.UniversalFileSystem.Cli.Output;
using Basalt.UniversalFileSystem.File;
using Basalt.UniversalFileSystem.GoogleCloudStorage;
using Basalt.UniversalFileSystem.Sftp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Basalt.UniversalFileSystem.Cli;

static class Program
{
    private static async Task<int> Main(string[] args)
    {
        string? profiles = Environment.GetEnvironmentVariable("UFS_PROFILES");

        using IHost host = new AppHostBuilder(profiles).Build(args);
        RootCommand rootCommand = host.Services.GetRequiredService<RootCommand>();

        int exitCode = await rootCommand.Parse(args).InvokeAsync().ConfigureAwait(false);
        return exitCode;
    }

    public static string GetConfigurationFilePath(string fileName = "config.yaml")
    {
        string configPath = Environment.GetEnvironmentVariable("UFS_CONFIG_PATH")
                            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ufs", fileName);
        return configPath;
    }

    [Profiles("Production")]
    [AppConfigurationConfigurator]
    public static void ProductionAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        builder
            .AddYamlFile(GetConfigurationFilePath(), true, false);
    }

    [Profiles("Debug")]
    [AppConfigurationConfigurator]
    public static void DebugAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        builder
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ApplicationException())
            .AddYamlFile("appsettings-debug.yaml", false, false);
    }

    [ServicesConfigurator]
    public static void CommonServices(HostBuilderContext context, ServiceCollection services)
    {
        services
            // Configuration
            .AddSingleton(context.Configuration)
            // Command line
            .AddCommandLineSupport()
            // UniversalFileSystem
            .AddFileFileSystem()
            .AddAwsS3FileSystem()
            .AddAzureBlobFileSystem()
            .AddGoogleCloudStorageFileSystem()
            .AddAliyunOssFileSystem()
            .AddSftpFileSystem();
    }


    [Profiles("Production", "Debug")]
    [ServicesConfigurator]
    public static void ProductionServices(HostBuilderContext context, ServiceCollection services)
    {
        services
            // Log
            .AddLogging(builder => builder
                .AddSimpleConsole(options => { options.SingleLine = true; })
                .SetMinimumLevel(LogLevel.Trace)
            )
            // UniversalFileSystem
            .AddUniversalFileSystem("UniversalFileSystem-Cli:UniversalFileSystem")
            // Output
            .AddTransient<IOutputWriter, ConsoleOutputWriter>()
            .AddTransient<IDatasetWriter, TabularDatasetWriter>()
            ;
    }
}