using System;
using System.Threading.Tasks;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;
using Krotus.UniversalFileSystem.Core;

namespace Krotus.UniversalFileSystem.Cli.Commands.Ls;

#nullable disable
partial class LsCommandOptions
{
    public bool Recursive { get; init; }
    [CliCommandSymbol(CliCommandSymbolType.Argument)]
    public string Directory { get; init; }
}
#nullable restore

class LsCommand : UniversalFileSystemCommand<LsCommandOptions>
{
    public LsCommand(CommandContext commandContext, IUniversalFileSystem universalFileSystem) : base(commandContext, universalFileSystem)
    {
    }

    public override async ValueTask ExecuteAsync()
    {
        await Console.Out.WriteLineAsync("from ls");
        await foreach (ObjectMetadata metadata in this.UniversalFileSystem.ListObjectsAsync(this.Options.Directory, this.Options.Recursive))
        {
            string size = metadata.ContentSize.HasValue ? metadata.ContentSize.Value.ToString("00000000") : "        ";
            string type = metadata.IsFile ? "     " : "<dir>";
            string time = metadata.LastModifiedTime.HasValue ? metadata.LastModifiedTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffff") : "                        ";
            string path = metadata.Path.Substring(this.Options.Directory.Length);
            await Console.Out.WriteLineAsync($"{type}  {time}  {size}  {path}");
        }
    }
}