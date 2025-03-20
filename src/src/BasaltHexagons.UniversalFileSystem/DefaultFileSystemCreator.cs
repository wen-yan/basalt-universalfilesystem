using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
class DefaultFileSystemCreator : IFileSystemCreator
{
    public DefaultFileSystemCreator(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.ServiceProvider = serviceProvider;
        this.Configuration = configuration;
    }

    private IServiceProvider ServiceProvider { get; }
    private IConfiguration Configuration { get; }

    public IFileSystem Create(string scheme)
    {
        IConfigurationSection configurationSection = this.Configuration.GetSection($"Schemes:{scheme}");

        string implementationFactoryClass = configurationSection["ImplementationFactoryClass"]
                                             ?? throw new ConfigurationMissingException($"Schemes:{scheme}");
        
        IFileSystemFactory factory = this.ServiceProvider.GetRequiredKeyedService<IFileSystemFactory>(implementationFactoryClass);
        IConfiguration implementationConfig = configurationSection.GetSection("Implementation");

        return new FileSystemWrapper(factory.Create(implementationConfig));
    }
}