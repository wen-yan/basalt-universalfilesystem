using System;
using System.Threading;
using BasaltHexagons.CommandLine;
using BasaltHexagons.UniversalFileSystem.Cli.Output;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem.Cli.Commands;

abstract class CommandBase<TOptions> : Command<TOptions>
{
    protected CommandBase(IServiceProvider serviceProvider) : base(serviceProvider.GetRequiredService<CommandContext>())
    {
        this.ServiceProvider = serviceProvider;
        this.OutputWriter = this.ServiceProvider.GetRequiredService<IOutputWriter>();
    }

    protected CancellationToken CancellationToken => this.CommandContext.InvocationContext?.GetCancellationToken() ?? CancellationToken.None;
    protected IServiceProvider ServiceProvider { get; }
    protected IOutputWriter OutputWriter { get; }
}