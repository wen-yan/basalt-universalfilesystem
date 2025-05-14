using System;
using BasaltHexagons.CommandLine;
using BasaltHexagons.CommandLine.Annotations;

namespace BasaltHexagons.UniversalFileSystem.Cli.Commands.Configuration;

[CliCommandBuilder("config", typeof(AppCommandBuilder))]
partial class ConfigurationCommandBuilder : CliCommandBuilder
{
    public ConfigurationCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Configuration command";
    }
}