using System;
using System.CommandLine;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;
using Basalt.UniversalFileSystem.Cli.Utils;

namespace Basalt.UniversalFileSystem.Cli.Commands.FileSystem;

partial class CopyCommandOptions
{
    [CliCommandSymbol] public bool Overwrite { get; }

    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public Uri Source { get; }

    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public Uri Destination { get; }
}

[CliCommandBuilder("cp", typeof(AppCommandBuilder))]
partial class CopyCommandBuilder : CliCommandBuilder<CopyCommand, CopyCommandOptions>
{
    public CopyCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Description = "Copy files.";

        this.OverwriteOption = new("--overwrite")
        {
            Description = "Overwrite existing files if exist.",
            DefaultValueFactory = _ => false,
            Required = false,
        };
        this.SourceArgument = new("Source file")
        {
            Description = "Source file uri.",
            Arity = ArgumentArity.ExactlyOne,
            CustomParser = CommandLineTokenParsers.UriParser,
        };
        this.DestinationArgument = new("Destination")
        {
            Description = "Destination uri.",
            Arity = ArgumentArity.ExactlyOne,
            CustomParser = CommandLineTokenParsers.UriParser,
        };
    }
}

class CopyCommand : FileSystemCommand<CopyCommandOptions>
{
    public CopyCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await this.UniversalFileSystem.CopyFilesRecursivelyAsync(this.Options.Source, this.Options.Destination, this.Options.Overwrite, this.CancellationToken);
    }
}