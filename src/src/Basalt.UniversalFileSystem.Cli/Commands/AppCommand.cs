using System;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;

namespace Basalt.UniversalFileSystem.Cli.Commands;

partial class AppCommandOptions
{
}

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCommandBuilder : RootCliCommandBuilder<AppCommandOptions>
{
    public AppCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Basalt Universal File System Command Line Tool";
    }
}