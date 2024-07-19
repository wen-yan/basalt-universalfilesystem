using System;
using System.IO;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;

namespace Krotus.UniversalFileSystem.Cli.Commands.Ls;

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