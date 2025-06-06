using System;
using System.CommandLine;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;
using Basalt.UniversalFileSystem.Cli.Output;

namespace Basalt.UniversalFileSystem.Cli.Commands.FileSystem;

partial class RemoveCommandOptions
{
    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public Uri[] Uris { get; init; }
    [CliCommandSymbol]
    public bool NoConfirm { get; init; }
}

[CliCommandBuilder("rm", typeof(AppCommandBuilder))]
partial class RemoveCommandBuilder : CliCommandBuilder<RemoveCommand, RemoveCommandOptions>
{
    public RemoveCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Remove files";
        this.UrisArgument = new("uris", "Uri of files to remove")
        {
            Arity = ArgumentArity.OneOrMore
        };
        this.NoConfirmOption = new("--no-confirm", () => false, "Don't prompt to confirm.");
    }
}

class RemoveCommand : FileSystemCommand<RemoveCommandOptions>
{
    public RemoveCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        // bool deleted = await this.UniversalFileSystem.DeleteObjectAsync(this.Options.Uri, this.CancellationToken);
        bool deleted = true;
        await this.OutputWriter.WriteLineAsync(deleted ? "Deleted object" : "Failed to delete object", this.CancellationToken);
    }
}