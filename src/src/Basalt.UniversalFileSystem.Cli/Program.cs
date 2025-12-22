using System;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.UniversalFileSystem.AliyunOss;
using Basalt.UniversalFileSystem.AwsS3;
using Basalt.UniversalFileSystem.AzureBlob;
using Basalt.UniversalFileSystem.Bootstrap;
using Basalt.UniversalFileSystem.Cli.Output;
using Basalt.UniversalFileSystem.File;
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
        using IHost host = new AppHostBuilder().Build(args);
        RootCommand rootCommand = host.Services.GetRequiredService<RootCommand>();

        int exitCode = await rootCommand.Parse(args).InvokeAsync();
        return exitCode;
    }

    public static string GetConfigurationFilePath(string fileName = "config.yaml")
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ufs", fileName);


    [Profiles("Production")]
    [AppConfigurationConfigurator]
    public static void ProductionAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        builder
#if DEBUG
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ApplicationException())
            .AddYamlFile("appsettings-test.yaml", false, false)
#else
            .AddYamlFile(GetConfigurationFilePath(), false, false)
#endif
            ;
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
            .AddAliyunOssFileSystem();
    }
    

    [Profiles("Production")]
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