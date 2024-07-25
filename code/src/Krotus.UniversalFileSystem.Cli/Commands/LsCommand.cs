using System;
using System.Threading.Tasks;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;
using Spectre.Console;

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

        this.RecursiveOption = new(["--recursive", "-r"], () => false, "Include subdirectories, default is false");
        this.DirectoryArgument = new("directory", "Directory");
    }
}


class LsCommand : UniversalFileSystemCommand<LsCommandOptions>
{
    public LsCommand(CommandContext commandContext, UniversalFileSystem universalFileSystem) : base(commandContext, universalFileSystem)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await Console.Out.WriteLineAsync("from ls");
        // await foreach (ObjectMetadata metadata in this.UniversalFileSystem.ListObjectsAsync(this.Options.Directory, this.Options.Recursive, this.CancellationToken))
        // {
        //     string size = metadata.ContentSize.HasValue ? metadata.ContentSize.Value.ToString("00000000") : "        ";
        //     string type = metadata.IsFile ? "     " : "<dir>";
        //     string time = metadata.LastModifiedTime.HasValue ? metadata.LastModifiedTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffff") : "                        ";
        //     string path = metadata.Path.Substring(this.Options.Directory.Length);
        //     await Console.Out.WriteLineAsync($"{type}  {time}  {size}  {path}");
        // }
        
        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Age");
        table.AddColumn("Occupation");

        table.AddRow("Alice", "23", "Software Engineer");
        table.AddRow("Bob", "32", "Accountant");
        table.AddRow("Charlie", "28", "Teacher");

        table.Title = new TableTitle("[underline yellow]People[/]");
        table.Caption = new TableTitle("[grey]Some random people[/]");

        AnsiConsole.Write(table);
    }
}
