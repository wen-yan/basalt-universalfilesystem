using System;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;

namespace Basalt.UniversalFileSystem.Cli.Commands.Configuration;

[CliCommandBuilder("config", typeof(AppCommandBuilder))]
partial class ConfigurationCommandBuilder : CliCommandBuilder
{
    public ConfigurationCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Configuration command";
    }
}