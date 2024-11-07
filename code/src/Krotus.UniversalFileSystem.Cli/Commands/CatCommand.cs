using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;
using Krotus.UniversalFileSystem.Cli.Output;

namespace Krotus.UniversalFileSystem.Cli.Commands;

#nullable disable
partial class CatCommandOptions
{
    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public string Path { get; init; }
}
#nullable restore

[CliCommandBuilder("cat", typeof(AppCommandBuilder))]
partial class CatCommandBuilder : CliCommandBuilder<CatCommand, CatCommandOptions>
{
    public CatCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "cat";
        this.PathArgument = new Argument<string>("path", "File path");
    }
}

class CatCommand : UniversalFileSystemCommand<CatCommandOptions>
{
    public CatCommand(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await using Stream stream = await this.UniversalFileSystem.GetObjectAsync(new Uri(this.Options.Path), this.CancellationToken);
        using StreamReader reader = new(stream, leaveOpen: true);
        while (true)
        {
            string? line = await reader.ReadLineAsync(this.CancellationToken);
            if (line == null) break;

            await this.OutputWriter.WriteLineAsync(line, this.CancellationToken);
        }
    }
}