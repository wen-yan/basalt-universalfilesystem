using System;
using Krotus.CommandLine;
using Krotus.CommandLine.Annotations;
using Krotus.UniversalFileSystem.Cli.Output;

namespace Krotus.UniversalFileSystem.Cli.Commands;


#nullable disable
partial class AppCommandOptions
{
    [CliCommandSymbol(CliCommandSymbolType.GlobalOption)]
    public DatasetOutputType DatasetOutputType { get; init; } 
}
#nullable restore


[CliCommandBuilder(CliCommandBuilderAttribute.DefaultRootCommandName, null)]
partial class AppCommandBuilder : RootCliCommandBuilder<AppCommandOptions>
{
    public AppCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Krotus Universal File System";

        this.DatasetOutputTypeOption = new(["-o", "--output"], () => DatasetOutputType.Tabular, "Dataset output type");
    }
}