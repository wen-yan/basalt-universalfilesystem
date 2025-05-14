using System;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.UniversalFileSystem.AliyunOss;
using BasaltHexagons.UniversalFileSystem.AwsS3;
using BasaltHexagons.UniversalFileSystem.AzureBlob;
using BasaltHexagons.UniversalFileSystem.Cli.Output;
using BasaltHexagons.UniversalFileSystem.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BasaltHexagons.UniversalFileSystem.Cli;

static class Program
{
    private static async Task<int> Main(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        hostBuilder
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ApplicationException())
#if DEBUG
                    .AddYamlFile("appsettings-test.yaml", false, false)
#else
                    .AddYamlFile($"{GetConfigurationFilePath()}", false, false)
#endif
                    ;
            })
            .ConfigureServices((context, services) =>
            {
                services
                    // Configuration
                    .AddSingleton(context.Configuration)

                    // Log
                    .AddLogging(builder => builder
                        .AddSimpleConsole(options => { options.SingleLine = true; })
                        .SetMinimumLevel(LogLevel.Trace)
                    )

                    // Command line
                    .AddCommandLineSupport()

                    // UniversalFileSystem
                    .AddUniversalFileSystem("UniversalFileSystem-Cli:UniversalFileSystem")
                    .AddFileFileSystem()
                    .AddAwsS3FileSystem()
                    .AddAzureBlobFileSystem()
                    .AddAliyunOssFileSystem()

                    // Output
                    .AddTransient<IOutputWriter, ConsoleOutputWriter>()
                    .AddTransient<IDatasetWriter, TabularDatasetWriter>()
                    ;
            });

        IHost host = hostBuilder.Build();
        RootCommand rootCommand = host.Services.GetRequiredService<RootCommand>();

        int exitCode = await rootCommand.InvokeAsync(args);
        return exitCode;
    }

    public static string GetConfigurationFilePath(string fileName = "config.yaml")
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ufs", fileName);
}