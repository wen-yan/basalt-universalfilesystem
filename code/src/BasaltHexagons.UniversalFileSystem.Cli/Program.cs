using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.UniversalFileSystem.Cli.Output;
using BasaltHexagons.UniversalFileSystem.File;
using Microsoft.Extensions.Configuration;
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
                    .AddInMemoryCollection(
                    [
                        new KeyValuePair<string, string?>("Schemes:file:ImplementationClass", typeof(FileFileSystem).FullName)
                    ])
                    .AddJsonFile("appsettings.json", false, false);
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

                    // UniversalFileSystem
                    .AddTransient<IFileSystemImplFactory>(serviceProvider =>
                    {
                        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection("Schemes");
                        return new DefaultFileSystemImplFactory(serviceProvider, configuration);
                    })
                    .AddTransient<UniversalFileSystem>()

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