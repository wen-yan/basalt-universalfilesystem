using System.Threading;
using Krotus.CommandLine;

namespace Krotus.UniversalFileSystem.Cli.Commands;

public abstract class UniversalFileSystemCommand<TOptions> : Command<TOptions>
{
    protected UniversalFileSystemCommand(CommandContext commandContext, UniversalFileSystem universalFileSystem) : base(commandContext)
    {
        this.UniversalFileSystem = universalFileSystem;
    }
    
    protected UniversalFileSystem UniversalFileSystem { get; }
    protected CancellationToken CancellationToken => this.CommandContext.InvocationContext?.GetCancellationToken() ?? CancellationToken.None;
}