using System;
using System.Threading;
using Basalt.CommandLine;
using Basalt.UniversalFileSystem.Cli.Output;
using Microsoft.Extensions.DependencyInjection;

namespace Basalt.UniversalFileSystem.Cli.Commands;

abstract class CommandBase<TOptions> : Command<TOptions>
{
    protected CommandBase(IServiceProvider serviceProvider) : base(serviceProvider.GetRequiredService<CommandContext>())
    {
        this.ServiceProvider = serviceProvider;
        this.OutputWriter = this.ServiceProvider.GetRequiredService<IOutputWriter>();
    }

    protected CancellationToken CancellationToken => this.CommandContext.CancellationToken;
    protected IServiceProvider ServiceProvider { get; }
    protected IOutputWriter OutputWriter { get; }
}