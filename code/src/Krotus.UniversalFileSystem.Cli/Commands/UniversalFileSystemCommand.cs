using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Krotus.CommandLine;
using Krotus.UniversalFileSystem.Cli.Output;
using Microsoft.Extensions.DependencyInjection;

namespace Krotus.UniversalFileSystem.Cli.Commands;

abstract class UniversalFileSystemCommand<TOptions> : Command<TOptions>
{
    protected UniversalFileSystemCommand(IServiceProvider serviceProvider)
        : base(serviceProvider.GetRequiredService<CommandContext>())
    {
        this.ServiceProvider = serviceProvider;
        this.UniversalFileSystem = this.ServiceProvider.GetRequiredService<UniversalFileSystem>();

        // file a better way?
        PropertyInfo property = typeof(TOptions).GetProperties()
            .First(x => x.Name == nameof(AppCommandOptions.DatasetOutputType));

        DatasetOutputType datasetOutputType = (DatasetOutputType)property.GetValue(this.Options)!;
        this.DatasetConsole = this.ServiceProvider.GetRequiredKeyedService<IDatasetConsole>(datasetOutputType);
    }

    protected IServiceProvider ServiceProvider { get; }
    protected UniversalFileSystem UniversalFileSystem { get; }
    protected IDatasetConsole DatasetConsole { get; }
    protected CancellationToken CancellationToken => this.CommandContext.InvocationContext?.GetCancellationToken() ?? CancellationToken.None;
}