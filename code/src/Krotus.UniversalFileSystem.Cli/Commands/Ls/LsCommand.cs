using System;
using System.Threading.Tasks;
using Krotus.CommandLine;

namespace Krotus.UniversalFileSystem.Cli.Commands.Ls;

#nullable disable
partial class LsCommandOptions
{
}
#nullable restore

class LsCommand : Command<LsCommandOptions>
{
    public LsCommand(CommandContext commandContext) : base(commandContext)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await Console.Out.WriteLineAsync("from ls");
    }
}