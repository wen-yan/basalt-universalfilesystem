using System;
using System.Threading.Tasks;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;
using Krotus.UniversalFileSystem.Cli.Output;
using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem.Cli.Commands;

#nullable disable
partial class MvCommandOptions
{
    public Uri Source { get; init; }
    public Uri Destination { get; init; }
    public bool Overwrite { get; init; }
}
#nullable restore

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