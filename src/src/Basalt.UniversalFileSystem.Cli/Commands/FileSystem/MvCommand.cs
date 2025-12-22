using System;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;
using Basalt.UniversalFileSystem.Cli.Output;
using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.Core.Exceptions;

namespace Basalt.UniversalFileSystem.Cli.Commands.FileSystem;

partial class MvCommandOptions
{
    [CliCommandSymbol] public Uri Source { get; init; }
    [CliCommandSymbol] public Uri Destination { get; init; }
    [CliCommandSymbol] public bool Overwrite { get; init; }
}

[CliCommandBuilder("mv", typeof(AppCommandBuilder))]
partial class MvCommandBuilder : CliCommandBuilder<MvCommand, MvCommandOptions>
{
    public MvCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "mv";

        this.SourceOption = new("--source", "-s")
        {
            Description = "Source file or prefix",
        };
        this.DestinationOption = new("--dest", "-d")
        {
            Description = "Destination",
        };
        this.OverwriteOption = new("--overwrite")
        {
            Description = "Overwrite destination if existing",
            DefaultValueFactory = _ => false,
        };
    }
}

class MvCommand : FileSystemCommand<MvCommandOptions>
{
    public MvCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        ObjectMetadata? metadata = await this.UniversalFileSystem.GetFileMetadataAsync(this.Options.Source, this.CancellationToken);
        if (metadata == null)
        {
            throw new FileNotExistsException(this.Options.Source);
        }
        else if (metadata.ObjectType == ObjectType.File)
        {
            await this.MoveObjectAsync(this.Options.Source, this.Options.Destination);
        }
        else
        {
            await foreach (ObjectMetadata obj in this.UniversalFileSystem.ListObjectsAsync(this.Options.Source, true, this.CancellationToken))
            {
                Uri relativeUri = this.Options.Source.MakeRelativeUri(obj.Uri);
                bool success = Uri.TryCreate(this.Options.Destination, relativeUri, out Uri? destUri);
                if (success && destUri != null)
                {
                    await this.MoveObjectAsync(obj.Uri, destUri);
                }
                else
                {
                    // TODO
                }
            }
        }
    }

    private async ValueTask MoveObjectAsync(Uri source, Uri destination)
    {
        await this.UniversalFileSystem.MoveFileAsync(source, destination, this.Options.Overwrite, this.CancellationToken);
        await this.OutputWriter.WriteLineAsync($"Moved file {source} to {destination}", this.CancellationToken);
    }
}