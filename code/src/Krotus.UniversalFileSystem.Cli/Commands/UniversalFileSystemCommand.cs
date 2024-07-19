using Krotus.CommandLine;

namespace Krotus.UniversalFileSystem.Cli.Commands;

public abstract class UniversalFileSystemCommand<TOptions> : Command<TOptions>
{
    protected UniversalFileSystemCommand(CommandContext commandContext, IUniversalFileSystem universalFileSystem) : base(commandContext)
    {
        this.UniversalFileSystem = universalFileSystem;
    }
    
    protected IUniversalFileSystem UniversalFileSystem { get; }
}