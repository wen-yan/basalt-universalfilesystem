using System;
using System.Threading.Tasks;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;
using BasaltHexagons.UniversalFileSystem.Cli.Output;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.Cli.Commands;

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

        this.SourceOption = new(["--source", "-s"], "Source file or prefix");
        this.DestinationOption = new(["--dest", "-d"], "Destination");
        this.OverwriteOption = new(["--overwrite"], () => false, "Overwrite destination if existing");
    }
}

class MvCommand : UniversalFileSystemCommand<MvCommandOptions>
{
    public MvCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        ObjectMetadata metadata = await this.UniversalFileSystem.GetObjectMetadataAsync(this.Options.Source, this.CancellationToken);
        if (metadata.ObjectType == ObjectType.File)
        {
            await this.MoveObjectAsync(this.Options.Source, this.Options.Destination);
        }
        else
        {
            await foreach (ObjectMetadata obj in this.UniversalFileSystem.ListObjectsAsync(this.Options.Source, true, this.CancellationToken))
            {
                Uri relativeUri = this.Options.Source.MakeRelativeUri(obj.Path);
                bool success = Uri.TryCreate(this.Options.Destination, relativeUri, out Uri? destUri);
                if (success && destUri != null)
                {
                    await this.MoveObjectAsync(obj.Path, destUri);
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
        await this.UniversalFileSystem.MoveObjectAsync(source, destination, this.Options.Overwrite, this.CancellationToken);
        await this.OutputWriter.WriteLineAsync($"Moved file {source} to {destination}", this.CancellationToken);
    }
}