using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Basalt.CommandLine;
using Basalt.CommandLine.Annotations;
using Basalt.UniversalFileSystem.Cli.Output;
using Basalt.UniversalFileSystem.Core;

namespace Basalt.UniversalFileSystem.Cli.Commands.Configuration;

partial class ConfigureInitCommandOptions
{
}

[CliCommandBuilder("init", typeof(ConfigurationCommandBuilder))]
partial class ConfigurationInitCommandBuilder : CliCommandBuilder<ConfigurationInitCommand, ConfigureInitCommandOptions>
{
    public ConfigurationInitCommandBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Description = "Initialize configure file";
    }
}

class ConfigurationInitCommand : CommandBase<ConfigureInitCommandOptions>
{
    public ConfigurationInitCommand(IServiceProvider serviceProvider, IEnumerable<IFileSystemFactory> fileSystemFactories) : base(serviceProvider)
    {
        this.FileSystemFactories = fileSystemFactories;
    }

    private IEnumerable<IFileSystemFactory> FileSystemFactories { get; }

    public override async ValueTask ExecuteAsync()
    {
        string templateName = $"{typeof(ConfigurationInitCommand).Namespace}.ConfigurationTemplate.yaml";

        async ValueTask<string> GetAllFactoryTemplatesAsync(int indent)
        {
            string allFactoryTemplates = string.Join(Environment.NewLine + Environment.NewLine,
                this.FileSystemFactories.Select(x =>
                    {
                        FileSystemFactoryConfigurationTemplateAttribute? attribute =
                            x.GetType().GetCustomAttribute<FileSystemFactoryConfigurationTemplateAttribute>();

                        return attribute?.ConfigurationTemplate;
                    })
                    .Where(x => x != null));

            StringBuilder builder = new();
            using StringReader reader = new StringReader(allFactoryTemplates);
            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null) break;

                for (int i = 0; i < indent; i++) builder.Append(' ');
                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        async ValueTask<string> GetTemplateAsync()
        {
            await using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(templateName) ??
                                        throw new ApplicationException("Can't load configuration template");

            using TextReader reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        string allFactoryTemplates = await GetAllFactoryTemplatesAsync(4);
        string template = await GetTemplateAsync();
        string result = template.Replace("<allFactoryTemplates>", allFactoryTemplates);

        string configFilePath = Program.GetConfigurationFilePath("config-template.yaml");
        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath)!);
        await System.IO.File.WriteAllTextAsync(configFilePath, result, this.CancellationToken);
        
        await this.OutputWriter.WriteLineAsync($"Configuration is saved in file `{configFilePath}`. You need to update it and copy/move to {Program.GetConfigurationFilePath()} to start using it.", this.CancellationToken);
    }
}