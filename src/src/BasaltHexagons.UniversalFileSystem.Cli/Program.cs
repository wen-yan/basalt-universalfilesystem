using System;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
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
                    .AddYamlFile($"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ufs", "config.yaml")}", false, false)
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

                    // FileSystems
                    .AddFileFileSystem()
                    .AddAwsS3FileSystem()
                    .AddAzureBlobFileSystem()

                    // UniversalFileSystem
                    .AddTransient<IUniversalFileSystem>(serviceProvider =>
                    {
                        IConfigurationSection config = serviceProvider.GetRequiredService<IConfiguration>().GetSection("BasaltHexagons:UniversalFileSystem");
                        return UniversalFileSystemFactory.Create(serviceProvider, config);
                    })

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
}