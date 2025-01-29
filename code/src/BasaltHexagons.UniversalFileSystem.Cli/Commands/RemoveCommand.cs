using System;
using System.CommandLine;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;
using BasaltHexagons.UniversalFileSystem.Cli.Output;

namespace BasaltHexagons.UniversalFileSystem.Cli.Commands;

#nullable disable
partial class RemoveCommandOptions
{
    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public Uri[] Paths { get; init; }
    [CliCommandSymbol]
    public bool NoConfirm { get; init; }
}
#nullable restore

[CliCommandBuilder("rm", typeof(AppCommandBuilder))]
partial class RemoveCommandBuilder : CliCommandBuilder<RemoveCommand, RemoveCommandOptions>
{
    public RemoveCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Remove files";
        this.PathsArgument = new("paths", "Path of files to remove")
        {
            Arity = ArgumentArity.OneOrMore
        };
        this.NoConfirmOption = new("--no-confirm", () => false, "Don't prompt to confirm.");
    }
}

class RemoveCommand : UniversalFileSystemCommand<RemoveCommandOptions>
{
    public RemoveCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        // bool deleted = await this.UniversalFileSystem.DeleteObjectAsync(this.Options.Path, this.CancellationToken);
        bool deleted = true;
        await this.OutputWriter.WriteLineAsync(deleted ? "Deleted object" : "Failed to delete object", this.CancellationToken);
    }
}