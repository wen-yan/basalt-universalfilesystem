using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;
using Krotus.UniversalFileSystem.Cli.Output;
using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem.Cli.Commands;

#nullable disable
partial class LsCommandOptions
{
    public bool Recursive { get; init; }

    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public string Directory { get; init; }
}
#nullable restore

[CliCommandBuilder("ls", typeof(AppCommandBuilder))]
partial class LsCommandBuilder : CliCommandBuilder<LsCommand, LsCommandOptions>
{
    public LsCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "ls";

        this.RecursiveOption = new Option<bool>(["--recursive", "-r"], () => false, "Include subdirectories, default is false");
        this.DirectoryArgument = new Argument<string>("directory", "Directory");
    }
}

class LsCommandOutput
{
    public string? Path { get; set; }
    public ObjectType ObjectType { get; set; }
    public DateTime? LastModifiedTime { get; set; }

    [TabularDatasetConsole(Alignment = TabularDatasetConsoleAlignment.Right)]
    public long? ContentSize { get; set; }
}

class LsCommand : UniversalFileSystemCommand<LsCommandOptions>
{
    public LsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await Console.Out.WriteLineAsync("from ls");

        IAsyncEnumerable<LsCommandOutput> results = this.UniversalFileSystem
            .ListObjectsAsync(this.Options.Directory, this.Options.Recursive, this.CancellationToken)
            .Select(metadata => new LsCommandOutput
            {
                Path = metadata.Path.Substring(this.Options.Directory.Length),
                ObjectType = metadata.ObjectType,
                LastModifiedTime = metadata.LastModifiedTime,
                ContentSize = metadata.ContentSize
            });

        await this.DatasetConsole.WriteAsync(results, this.CancellationToken);
    }
}