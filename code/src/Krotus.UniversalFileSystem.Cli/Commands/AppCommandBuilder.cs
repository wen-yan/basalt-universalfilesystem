using System;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;

namespace Krotus.UniversalFileSystem.Cli.Commands;

[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
public partial class AppCommandBuilder : RootCliCommandBuilder
{
    public AppCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Krotus Universal File System";
    }
}