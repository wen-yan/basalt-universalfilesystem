using System;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;

namespace Krotus.UniversalFileSystem.Cli.Commands;

partial class AppCommandOptions
{
}

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCommandBuilder : RootCliCommandBuilder<AppCommandOptions>
{
    public AppCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Krotus Universal File System";
    }
}